using API.Seguridad;
using Abstracciones.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace API.Controllers
{
    [ApiController,Authorize(Roles="Administrador"),Route("api/AdministracionUsuarios")]
    [ImpedirAutoadministracion]
    public sealed class AdministracionUsuariosController : ControllerBase
    {
        private readonly ICredencialesFlujo _flujo;
        public AdministracionUsuariosController(ICredencialesFlujo flujo){_flujo=flujo;}
        [HttpPost("Registrar")]
        public async Task<ActionResult<ResultadoInvitacion>> Registrar(RegistroAdministrativoRequest solicitud,CancellationToken ct)
            =>Ok(await _flujo.Registrar(solicitud,int.Parse(User.FindFirstValue("Empresa")!),int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),ct));
        [HttpPost("Restablecer/{idUsuario:int:min(1)}")]
        public async Task<ActionResult<ResultadoInvitacion>> Restablecer(int idUsuario,CancellationToken ct)
            =>Ok(await _flujo.Restablecer(idUsuario,int.Parse(User.FindFirstValue("Empresa")!),int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),ct));
    }
}