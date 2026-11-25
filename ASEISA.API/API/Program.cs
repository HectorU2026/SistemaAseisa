using API.Seguridad;
using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Interfaces.Servicio;
using DA;
using DA.Repositorio;
using Flujo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using QuestPDF.Infrastructure;
using Servicio;
using Servicios;
using Servicios.Correo;
using System.Security.Claims;
using System.Text;

// Licencia gratuita para usar generar pdf
QuestPDF.Settings.License = LicenseType.Community;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pegue solo el accessToken, sin el prefijo Bearer."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var issuer = builder.Configuration["Token:Issuer"];
var audience = builder.Configuration["Token:Audience"];
var key = builder.Configuration["Token:key"];
if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience)
    || string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
    throw new InvalidOperationException("Configure Token:Issuer, Token:Audience y Token:key (minimo 32 bytes), iguales a Seguridad.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
        options.IncludeErrorDetails = false;
        options.EventsType = typeof(RevalidacionJwtEvents);
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IContextoUsuarioDA, ContextoUsuarioDA>();
builder.Services.AddScoped<IContextoUsuarioFlujo, ContextoUsuarioFlujo>();
builder.Services.AddScoped<RevalidacionJwtEvents>();

builder.Services.AddScoped<IRepositorioDapper, RepositorioDapper>();
builder.Services.AddScoped<IUsuarioDA, UsuarioDA>();
builder.Services.AddScoped<Abstracciones.Interfaces.DA.IPlataformaDA,DA.PlataformaDA>();
builder.Services.AddScoped<Abstracciones.Interfaces.Flujo.IPlataformaFlujo,Flujo.PlataformaFlujo>();
builder.Services.AddScoped<IUsuarioFlujo, UsuarioFlujo>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("CredencialesSeguridad",c=>c.Timeout=TimeSpan.FromSeconds(15))
    .ConfigurePrimaryHttpMessageHandler(()=>new HttpClientHandler {AllowAutoRedirect=false});
builder.Services.AddScoped<IAdministracionCredenciales, AdministracionCredenciales>();
builder.Services.AddScoped<IBitacoraDA, BitacoraDA>();
builder.Services.AddScoped<IBitacoraFlujo, BitacoraFlujo>();
builder.Services.AddScoped<IBodegaDA, BodegaDA>();
builder.Services.AddScoped<IBodegaFlujo, BodegaFlujo>();
builder.Services.AddScoped<IEmpresaDA, EmpresaDA>();
builder.Services.AddScoped<IEmpresaFlujo, EmpresaFlujo>();
builder.Services.AddScoped<IContactoDA, ContactoDA>();
builder.Services.AddScoped<IContactoFlujo, ContactoFlujo>();
builder.Services.AddScoped<IProductoDA, ProductoDA>();
builder.Services.AddScoped<IProductoFlujo, ProductoFlujo>();
builder.Services.AddScoped<ICompraDA, CompraDA>();
builder.Services.AddScoped<ICompraFlujo, CompraFlujo>();
builder.Services.AddScoped<IDistribucionAnaliticaDA, DistribucionAnaliticaDA>();
builder.Services.AddScoped<IDistribucionAnaliticaFlujo, DistribucionAnaliticaFlujo>();
builder.Services.AddScoped<ICotizacionDA, CotizacionDA>();
builder.Services.AddScoped<ICotizacionFlujo, CotizacionFlujo>();
builder.Services.AddScoped<IFacturaDA, FacturaDA>();
builder.Services.AddScoped<IFacturaFlujo, FacturaFlujo>();
builder.Services.AddScoped<IEnviarCorreo, EnviarCorreo>();
builder.Services.AddScoped<IConstructorCorreo, ConstructorCorreo>();
builder.Services.AddScoped<IGeneradorPdfCotizacion, GeneradorPdfCotizacion>();
builder.Services.AddScoped<IEnviarCotizacion, EnviarCotizacion>();
builder.Services.AddScoped<IGeneradorPdfFactura, GeneradorPdfFactura>();
builder.Services.AddScoped<IEnviarFactura, EnviarFactura>();
builder.Services.AddScoped<IGeneradorXmlFactura, GeneradorXmlFactura>();
builder.Services.AddScoped<IEnviarXmlFactura, EnviarXmlFactura>();
builder.Services.AddScoped<IGeneradorXmlFactura44Academico, GeneradorXmlFactura44Academico>();
builder.Services.AddScoped<IFirmaAcademicaSimulada, FirmaAcademicaSimulada>();
builder.Services.AddScoped<IInventarioFisicoDA, InventarioFisicoDA>();
builder.Services.AddScoped<IInventarioFisicoFlujo, InventarioFisicoFlujo>();
builder.Services.AddScoped<IIngresoProductoDA, IngresoProductoDA>();
builder.Services.AddScoped<IIngresoProductoFlujo, IngresoProductoFlujo>();
builder.Services.AddScoped<ICuentasPorPagarDA, CuentasPorPagarDA>();
builder.Services.AddScoped<ICuentasPorPagarFlujo, CuentasPorPagarFlujo>();
builder.Services.AddScoped<IBancoDA, BancoDA>();
builder.Services.AddScoped<IBancoFlujo, BancoFlujo>();
builder.Services.AddScoped<ITerminoPagoFlujo, TerminoPagoFlujo>();
builder.Services.AddScoped<ITerminoPagoDA, TerminoPagoDA>();
builder.Services.AddScoped<IMetodoPagoFlujo, MetodoPagoFlujo>();
builder.Services.AddScoped<IMetodoPagoDA, MetodoPagoDA>();
builder.Services.AddScoped<ICuentaContableDA, CuentaContableDA>();
builder.Services.AddScoped<ICuentaContableFlujo, CuentaContableFlujo>();

builder.Services.AddScoped<IReportesDA, ReportesDA>();
builder.Services.AddScoped<IReportesFlujo, ReportesFlujo>();

builder.Services.AddScoped<IActivoFijoDA, ActivoFijoDA>();
builder.Services.AddScoped<IActivoFijoFlujo, ActivoFijoFlujo>();

var app = builder.Build();
app.UseMiddleware<API.Seguridad.ErroresUsuariosMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.Use(async(context,next)=>
{
    if(context.User.IsInRole("Superadministrador") && !context.Request.Path.StartsWithSegments("/api/Plataforma") && context.Request.Path!="/api/Sesion")
    {context.Response.StatusCode=403;await context.Response.WriteAsJsonAsync(new{mensaje="Esta identidad no tiene contexto empresarial."});return;}
    await next(context);
});
app.UseAuthorization();

app.MapControllers();

app.Run();
