using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Abstracciones.Reglas;
using Microsoft.AspNetCore.Mvc;
namespace Reglas
{
    public sealed class RecuperarContrasena : IRecuperarContrasena
    {
        private readonly ICredencialesFlujo _credenciales;
        public RecuperarContrasena(ICredencialesFlujo credenciales){_credenciales=credenciales;}
        public async Task<ActionResult> GestionRecuperarContrasena(UsuarioRecuperar correo)
        {
            if(string.IsNullOrWhiteSpace(correo?.correo) || correo.correo.Length>150
                || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(correo.correo)) return new BadRequestObjectResult("Solicitud no valida.");
            await _credenciales.Recuperar(correo.correo,CancellationToken.None);
            return new AcceptedResult((string?)null,new {mensaje="Si la cuenta es elegible, recibira instrucciones."});
        }

    }
}