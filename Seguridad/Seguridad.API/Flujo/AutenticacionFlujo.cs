using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Abstracciones.Reglas;
using Microsoft.AspNetCore.Mvc;
namespace Flujo
{
    public sealed class AutenticacionFlujo:IAutenticacionFlujo
    {
        private readonly IAutenticacionBC _autenticacion;
        private readonly IRecuperarContrasena _recuperacion;
        private readonly ICredencialesFlujo _credenciales;
        public AutenticacionFlujo(IAutenticacionBC autenticacion,IRecuperarContrasena recuperacion,ICredencialesFlujo credenciales)
        {_autenticacion=autenticacion;_recuperacion=recuperacion;_credenciales=credenciales;}
        public Task<Token> LoginAsync(LoginBase login)=>_autenticacion.LoginAync(login);
        public async Task<IActionResult> RecuperarContrasena(UsuarioRecuperar correo)=>await _recuperacion.GestionRecuperarContrasena(correo);
        public async Task<IActionResult> CambiarContrasena(CambiarContrasena usuario)
        {
            if(usuario==null || string.IsNullOrWhiteSpace(usuario.Token) || string.IsNullOrWhiteSpace(usuario.ContrasenaHash))
                return new BadRequestObjectResult("Solicitud no valida.");
            var exito=await _credenciales.Consumir(usuario.Token,usuario.ContrasenaHash,CancellationToken.None);
            return exito?new OkResult():new BadRequestObjectResult("Enlace no valido o vencido, o contrasena fuera de la longitud permitida (12 a 128).");
        }
    }
}