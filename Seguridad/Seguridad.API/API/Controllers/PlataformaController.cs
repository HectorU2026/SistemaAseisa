using ASEISA.Plataforma;
using Abstracciones.Flujo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace API.Controllers;
[ApiController,Authorize(Roles="Superadministrador"),Route("api/Plataforma")]
public sealed class PlataformaController:ControllerBase
{
    private readonly IPlataformaCredencialesFlujo flujo;
    public PlataformaController(IPlataformaCredencialesFlujo flujo){this.flujo=flujo;}
    private int Actor=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpPost("empresas/{empresa:int:min(1)}/administradores")]
    public async Task<IActionResult> Registrar(int empresa,PersonaPlataforma persona,CancellationToken ct)=>Ok(await flujo.Registrar(Actor,empresa,persona,ct));
    [HttpPost("empresas/{empresa:int:min(1)}/administradores/{usuario:int:min(1)}/enlace")]
    public async Task<IActionResult> Restablecer(int empresa,int usuario,CancellationToken ct)=>Ok(await flujo.Restablecer(Actor,empresa,usuario,ct));
}