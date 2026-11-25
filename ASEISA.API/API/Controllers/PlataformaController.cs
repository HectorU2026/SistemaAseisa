using ASEISA.Plataforma;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Interfaces.Servicio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace API.Controllers;
[ApiController,Route("api/Plataforma"),Authorize(Roles="Superadministrador")]
public sealed class PlataformaController:ControllerBase
{
    private readonly IPlataformaFlujo flujo;private readonly IAdministracionCredenciales credenciales;
    public PlataformaController(IPlataformaFlujo flujo,IAdministracionCredenciales credenciales){this.flujo=flujo;this.credenciales=credenciales;}
    private int Actor=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet]public async Task<IActionResult> Consultar([FromQuery]int? empresa,CancellationToken ct)=>Ok(await flujo.Consultar(Actor,empresa,ct));
    [HttpPut("empresas/{empresa:int:min(1)}/estado")]
    public async Task<IActionResult> EstadoEmpresa(int empresa,Abstracciones.Modelos.UsuarioCambiarEstadoRequest datos,CancellationToken ct)=>Ok(await flujo.EstadoEmpresa(Actor,empresa,datos.Activo!.Value,ct));
    [HttpPost("empresas")]public async Task<IActionResult> Crear(EmpresaPlataforma datos,CancellationToken ct)=>Ok(await flujo.Guardar(Actor,null,datos,ct));
    [HttpPut("empresas/{empresa:int:min(1)}")]public async Task<IActionResult> Guardar(int empresa,EmpresaPlataforma datos,CancellationToken ct)=>Ok(await flujo.Guardar(Actor,empresa,datos,ct));
    [HttpPost("empresas/{empresa:int:min(1)}/administradores")]public async Task<IActionResult> Registrar(int empresa,PersonaPlataforma datos,CancellationToken ct)=>Ok(await credenciales.RegistrarPlataforma(empresa,datos,ct));
    [HttpPut("empresas/{empresa:int:min(1)}/administradores/{usuario:int:min(1)}")]
    public async Task<IActionResult> Editar(int empresa,int usuario,EdicionAdministradorPlataforma datos,CancellationToken ct)=>Ok(await flujo.EditarAdministrador(Actor,empresa,usuario,datos,ct));
    [HttpPut("empresas/{empresa:int:min(1)}/administradores/{usuario:int:min(1)}/estado")]
    public async Task<IActionResult> Estado(int empresa,int usuario,Abstracciones.Modelos.UsuarioCambiarEstadoRequest datos,CancellationToken ct)=>Ok(await flujo.EstadoAdministrador(Actor,empresa,usuario,datos.Activo!.Value,ct));
    [HttpPost("empresas/{empresa:int:min(1)}/administradores/{usuario:int:min(1)}/enlace")]
    public async Task<IActionResult> Enlace(int empresa,int usuario,CancellationToken ct)=>Ok(await credenciales.RestablecerPlataforma(empresa,usuario,ct));
}