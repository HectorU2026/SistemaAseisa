using Abstracciones.DA;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using BC;
using Microsoft.Extensions.Configuration;
using Seguridad.Criptografia;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
internal static class PruebasLoginEstados
{
    public static async Task<int> Ejecutar()
    {
        var configuracion=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
            ["Token:key"]=new string('K',80),["Token:Issuer"]="aislado",["Token:Audience"]="aislado",["Token:Expires"]="10"}).Build();
        var hash=ContrasenasSeguras.Crear("SoloPruebaSintetica-2026");int total=0;
        async Task Caso(string nombre,string aprobacion,bool activo,bool empresa,string[] roles,bool correcta,string? estado,bool exito,bool existe=true)
        {
            var da=new UsuariosFake();
            if(existe)foreach(var rol in roles.Length==0?new[]{""}:roles)da.Filas.Add(new LoginAutenticado {
                IdUsuario=7,IdEmpresa="8",NombreUsuario="uprueba.6789",Correo="prueba@example.invalid",ContrasenaHash=hash,
                VersionCredencial=2,EstadoAprobacion=aprobacion,UsuarioActivo=activo,EmpresaActiva=empresa,rol=rol});
            var auditoria=new AuditoriaFake();var reglas=new AutenticacionReglas(configuracion,da,auditoria);
            var respuesta=await reglas.LoginAync(new(){Correo="prueba@example.invalid",ContrasenaHash=correcta?"SoloPruebaSintetica-2026":"Incorrecta-2026"});
            if(respuesta.ValidacionExitosa!=exito || respuesta.CodigoEstado!=estado || (!exito && respuesta.AccessToken!="") || auditoria.Eventos!=(exito?1:0))throw new Exception(nombre);
            if(exito){var jwt=new JwtSecurityTokenHandler().ReadJwtToken(respuesta.AccessToken);
                if(!jwt.Claims.Any(c=>c.Type=="VersionCredencial" && c.Value=="2") || roles.Any(rol=>!jwt.Claims.Any(c=>c.Type==ClaimTypes.Role && c.Value==rol)))throw new Exception(nombre+" claims");}
            Console.WriteLine("OK "+nombre);total++;
        }
        await Caso("Administrador aprobado obtiene JWT y una auditoría","Aprobado",true,true,new[]{"Administrador"},true,null,true);
        await Caso("Cliente aprobado obtiene JWT","Aprobado",true,true,new[]{"Cliente"},true,null,true);
        await Caso("JWT conserva todos los roles activos","Aprobado",true,true,new[]{"Cliente","Vendedor"},true,null,true);
        await Caso("Pendiente con contraseña correcta informa aprobación","Pendiente",false,true,new[]{"Cliente"},true,"PENDIENTE",false);
        await Caso("Rechazado no obtiene JWT","Rechazado",false,true,new[]{"Cliente"},true,"RECHAZADO",false);
        await Caso("Inactivo no obtiene JWT","Aprobado",false,true,new[]{"Cliente"},true,"INACTIVO",false);
        await Caso("Empresa inactiva no obtiene JWT","Aprobado",true,false,new[]{"Cliente"},true,"NO_DISPONIBLE",false);
        await Caso("Sin roles válidos no obtiene JWT","Aprobado",true,true,Array.Empty<string>(),true,"NO_DISPONIBLE",false);
        await Caso("Contraseña incorrecta no revela cuenta pendiente","Pendiente",false,true,new[]{"Cliente"},false,null,false);
        await Caso("Correo inexistente tiene respuesta genérica","Aprobado",true,true,new[]{"Cliente"},true,null,false,false);
        return total;
    }
    private sealed class UsuariosFake:IUsuarioDA
    {
        public List<LoginAutenticado> Filas=new();
        public Task<IEnumerable<LoginAutenticado>> ObtenerCandidatoAutenticacion(LoginBase login)=>Task.FromResult<IEnumerable<LoginAutenticado>>(Filas);
        public Task<IEnumerable<LoginAutenticado>> ObtenerUsuarioConRoles(LoginBase login)=>throw new Exception("No usar consulta propia para diagnosticar login");
        public Task<LoginAutenticado> ObtenerUsuario(LoginBase login)=>throw new NotSupportedException();
        public Task<RegistroResponse> CrearUsuario(RegistroRequest solicitud)=>throw new NotSupportedException();
        public Task<UsuarioRecuperar> ValidarCorreoActivo(string correo)=>throw new NotSupportedException();
        public Task<bool> MigrarHashAutenticacion(LoginAutenticado usuario,string hash)=>Task.FromResult(true);
    }
    private sealed class AuditoriaFake:IAuditoriaSeguridad
    {
        public int Eventos;
        public Task RegistrarAsync(EventoAuditoriaSeguridad evento,int? usuario=null,string? correo=null){Eventos++;return Task.CompletedTask;}
    }
}