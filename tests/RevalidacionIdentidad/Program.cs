#if SEGURIDAD
using Abstracciones.DA;
using Abstracciones.Flujo;
#else
using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
#endif
using Abstracciones.Modelos;
using API.Seguridad;
using Flujo;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Pruebas en memoria: no servidor HTTP, configuracion externa, SQL ni endpoints de negocio.
var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ClaveSinteticaSoloPruebasAisladasRevalidacion-2026-0001-LongitudMayorA64Bytes"));
var pruebas = 0;
void Comprobar(bool cumple, string mensaje)
{
    if (!cumple) throw new InvalidOperationException(mensaje);
}
string Token(string? usuario = "7", string? empresa = "8", string rol = "Administrador", string issuer = "pruebas",
    DateTime? vence = null, SecurityKey? key = null, string algoritmo = SecurityAlgorithms.HmacSha256,
    bool duplicado = false, bool firmado = true, string audience = "pruebas-api", string? version = "0", bool duplicadoVersion = false, bool plataforma = false)
{
    var claims = new List<Claim> { new(ClaimTypes.Role, rol), new(ClaimTypes.Name, "nombre-antiguo"),
        new(ClaimTypes.Email, "antiguo@example.invalid"), new("claim-antiguo", "descartar") };
    if(plataforma)claims.Add(new Claim("Ambito","Plataforma"));
    if (usuario != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, usuario));
    if (empresa != null) claims.Add(new Claim("Empresa", empresa));
    if(version!=null)claims.Add(new Claim("VersionCredencial",version));
    if(duplicadoVersion)claims.Add(new Claim("VersionCredencial","0"));
    if (duplicado) claims.Add(new Claim("Empresa", "9"));
    return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer, audience, claims,
        expires: vence ?? DateTime.UtcNow.AddMinutes(2),
        signingCredentials: firmado ? new SigningCredentials(key ?? llave, algoritmo) : null));
}
ContextoUsuarioAutorizado Vigente(params string[] roles) => new()
{
    VersionCredencial=0, IdUsuario = 7, IdEmpresa = 8, NombreUsuario = "actual", Correo = "actual@example.invalid",
    Roles = roles.Select((nombre, i) => new RolUsuarioAutorizado { IdRol = i + 1, NombreRol = nombre }).ToList()
};

async Task Caso(string nombre, string? token, ContextoUsuarioAutorizado? vigente, int esperado,
    string? rolRequerido = "Administrador", bool error = false, bool anonimo = false, Action<ClaimsPrincipal>? verificar = null)
{
    var fake = new ContextoDAFalso { Resultado = vigente, Fallar = error };
    var servicios = new ServiceCollection();
    servicios.AddLogging();
    servicios.AddRouting();
    servicios.AddSingleton<IContextoUsuarioDA>(fake);
    servicios.AddScoped<IContextoUsuarioFlujo, ContextoUsuarioFlujo>();
    servicios.AddScoped<RevalidacionJwtEvents>();
    servicios.AddAuthorization();
    servicios.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.MapInboundClaims = true;
        options.IncludeErrorDetails = false;
        options.EventsType = typeof(RevalidacionJwtEvents);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = "pruebas", ValidateAudience = true, ValidAudience = "pruebas-api",
            ValidateLifetime = true, RequireExpirationTime = true, RequireSignedTokens = true,
            ValidateIssuerSigningKey = true, IssuerSigningKey = llave, ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.Zero, NameClaimType = ClaimTypes.Name, RoleClaimType = ClaimTypes.Role
        };
    });
    await using var proveedor = servicios.BuildServiceProvider();
    await using var scope = proveedor.CreateAsyncScope();
    var ejecutado = false;
    var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
    http.Response.Body = new MemoryStream();
    if (token != null) http.Request.Headers.Authorization = "Bearer " + token;
    var metadata = anonimo ? new EndpointMetadataCollection(new AllowAnonymousAttribute())
        : new EndpointMetadataCollection(new AuthorizeAttribute { Roles = rolRequerido });
    http.SetEndpoint(new Endpoint(_ => Task.CompletedTask, metadata, "OperacionSintetica"));
    var app = new ApplicationBuilder(proveedor);
    app.UseAuthentication();
    app.UseAuthorization();
    app.Run(async context =>
    {
        ejecutado = true;
        verificar?.Invoke(context.User);
        if (!anonimo)
        {
            // Simular dos consumidores en la misma solicitud: no deben repetir la consulta.
            var servicio = context.RequestServices.GetRequiredService<IContextoUsuarioFlujo>();
            await servicio.Obtener(7, vigente?.IdEmpresa, default);
            await servicio.Obtener(7, vigente?.IdEmpresa, default);
        }
        context.Response.StatusCode = 200;
    });
    await app.Build()(http);
    Comprobar(http.Response.StatusCode == esperado, nombre + ": codigo HTTP incorrecto");
    Comprobar(ejecutado == (esperado == 200), nombre + ": operacion protegida ejecutada indebidamente");
    Comprobar(fake.Consultas <= 1, nombre + ": consulta repetida");
    if (esperado == 503)
    {
        http.Response.Body.Position = 0;
        var cuerpo = await new StreamReader(http.Response.Body).ReadToEndAsync();
        Comprobar(cuerpo.Contains("Servicio temporalmente no disponible.") && !cuerpo.Contains("SECRETO_SQL"), "Error tecnico expuesto");
    }
    Console.WriteLine("OK " + nombre + " HTTP=" + esperado);
    pruebas++;
}

await Caso("Identidad y dos roles actuales reemplazan claims antiguos", Token(rol: "RolObsoleto"), Vigente("Administrador", "Cliente"), 200,
    verificar: p =>
    {
        Comprobar(p.Identities.Count() == 1 && p.Identity?.Name == "actual", "Identidad antigua conservada");
        Comprobar(p.FindFirst(ClaimTypes.Email)?.Value == "actual@example.invalid", "Correo antiguo conservado");
        Comprobar(p.FindFirst("Empresa")?.Value == "8" && p.FindFirst(ClaimTypes.NameIdentifier)?.Value == "7", "Identificadores incorrectos");
        Comprobar(p.IsInRole("Administrador") && p.IsInRole("Cliente") && !p.IsInRole("RolObsoleto"), "Roles incorrectos");
        Comprobar(p.FindFirst("claim-antiguo") == null, "Claim antiguo conservado");
    });
await Caso("Un rol vigente", Token(), Vigente("Administrador"), 200);
await Caso("Rol retirado del contexto", Token(), Vigente("Cliente"), 403);
await Caso("Asignacion inactiva excluida del contexto", Token(), Vigente("Cliente"), 403);
await Caso("Rol de catalogo inactivo excluido del contexto", Token(), Vigente(), 403);
await Caso("Sin roles no satisface permiso", Token(), Vigente(), 403);
await Caso("Sin roles conserva identidad autenticada", Token(), Vigente(), 200, rolRequerido: null);
foreach (var motivo in new[] { "Usuario desactivado", "Usuario eliminado", "Aprobacion pendiente", "Aprobacion rechazada", "Empresa inactiva", "Empresa inexistente" })
    await Caso(motivo + " (SQL sin contexto)", Token(), null, 401);
await Caso("Empresa devuelta no coincide", Token(), new ContextoUsuarioAutorizado { IdUsuario = 7, IdEmpresa = 9 }, 401);
await Caso("Identidad devuelta no coincide", Token(), new ContextoUsuarioAutorizado { IdUsuario = 9, IdEmpresa = 8 }, 401);
await Caso("Identificador ausente", Token(usuario: null), Vigente("Administrador"), 401);
await Caso("Empresa ausente", Token(empresa: null), Vigente("Administrador"), 401);
await Caso("Empresa duplicada", Token(duplicado: true), Vigente("Administrador"), 401);
await Caso("Identificador invalido", Token(usuario: "-7"), Vigente("Administrador"), 401);
var sinVersionSql=Vigente("Administrador");sinVersionSql.VersionCredencial=null;
await Caso("Consulta SQL sin version falla cerrada",Token(),sinVersionSql,503);
await Caso("JWT sin version requiere nuevo login",Token(version:null),Vigente("Administrador"),401);
await Caso("JWT version ambigua",Token(duplicadoVersion:true),Vigente("Administrador"),401);
await Caso("JWT version invalida",Token(version:"-1"),Vigente("Administrador"),401);
var despuesDelCambio=Vigente("Administrador");despuesDelCambio.VersionCredencial=2;
await Caso("JWT anterior al cambio revocado",Token(),despuesDelCambio,401);
await Caso("JWT posterior al cambio permitido",Token(version:"2"),despuesDelCambio,200);
await Caso("JWT ausente", null, Vigente("Administrador"), 401);
await Caso("JWT vencido", Token(vence: DateTime.UtcNow.AddMinutes(-2)), Vigente("Administrador"), 401);
await Caso("JWT emisor incorrecto", Token(issuer: "otro"), Vigente("Administrador"), 401);
await Caso("JWT audiencia incorrecta", Token(audience: "otra"), Vigente("Administrador"), 401);
await Caso("JWT firma incorrecta", Token(key: new SymmetricSecurityKey(new byte[64])), Vigente("Administrador"), 401);
await Caso("JWT algoritmo no permitido", Token(algoritmo: SecurityAlgorithms.HmacSha512), Vigente("Administrador"), 401);
await Caso("JWT sin firma", Token(firmado: false), Vigente("Administrador"), 401);
await Caso("Fallo tecnico cierra acceso sin filtrar detalles", Token(), null, 503, error: true);
await Caso("Contrato de contexto invalido", Token(), new ContextoUsuarioAutorizado { IdUsuario = 7, IdEmpresa = 8 }, 503);
await Caso("Endpoint anonimo permanece anonimo", null, null, 200, anonimo: true);

var plataformaVigente=Vigente("Superadministrador");plataformaVigente.IdEmpresa=null;
await Caso("Plataforma sin empresa autenticada",Token(empresa:null,plataforma:true),plataformaVigente,200,rolRequerido:"Superadministrador",verificar:p=>Comprobar(p.FindFirst("Empresa")==null && p.FindFirst("Ambito")?.Value=="Plataforma","Se invento una empresa"));
await Caso("Plataforma no hereda Administrador",Token(empresa:null,plataforma:true),plataformaVigente,403);
await Caso("Ambito plataforma con empresa rechazado",Token(plataforma:true),plataformaVigente,401);
await Caso("Usuario ordinario sin empresa rechazado",Token(empresa:null),plataformaVigente,401);
var plataformaMixta=Vigente("Superadministrador","Administrador");plataformaMixta.IdEmpresa=null;
await Caso("Plataforma con roles ordinarios rechazada",Token(empresa:null,plataforma:true),plataformaMixta,401);
await Caso("Superadministrador asociado a empresa rechazado",Token(),Vigente("Superadministrador"),401);
var sinRoles=Vigente();sinRoles.IdEmpresa=null;
await Caso("Plataforma sin rol valido rechazada",Token(empresa:null,plataforma:true),sinRoles,401);
await Caso("Plataforma revocada rechazada",Token(empresa:null,plataforma:true),null,401);
var plataformaVersion=Vigente("Superadministrador");plataformaVersion.IdEmpresa=null;plataformaVersion.VersionCredencial=1;
await Caso("Password revoca JWT de plataforma",Token(empresa:null,plataforma:true),plataformaVersion,401);

// Un token anterior se revalida en un scope nuevo despues de revocar permisos.
{
    var fake = new ContextoDAFalso { Resultado = Vigente("Administrador") };
    var primera = new ContextoUsuarioFlujo(fake);
    Comprobar((await primera.Obtener(7, 8, default))!.Roles.Count == 1, "Primera solicitud incorrecta");
    fake.Resultado = null;
    var segunda = new ContextoUsuarioFlujo(fake);
    Comprobar(await segunda.Obtener(7, 8, default) == null && fake.Consultas == 2, "Se mantuvo cache entre solicitudes");
    Console.WriteLine("OK Sin cache entre solicitudes"); pruebas++;
}
pruebas+=await PruebasCookies.Ejecutar();
#if SEGURIDAD
pruebas+=await PruebasCredenciales.Ejecutar();
pruebas+=await PruebasLoginEstados.Ejecutar();
#endif
pruebas+=PruebasRegistroUsuarios.Ejecutar();
pruebas+=await PruebasAutoadministracion.Ejecutar();
Console.WriteLine("TOTAL=" + pruebas + "; Sin SQL ni red.");

sealed class ContextoDAFalso : IContextoUsuarioDA
{
    public ContextoUsuarioAutorizado? Resultado { get; set; }
    public bool Fallar { get; set; }
    public int Consultas { get; private set; }
    public Task<ContextoUsuarioAutorizado?> Obtener(int idUsuario, int? idEmpresa, CancellationToken cancellationToken)
    {
        Consultas++;
        if (Fallar) throw new InvalidOperationException("SECRETO_SQL: no mostrar");
        return Task.FromResult(Resultado);
    }
}