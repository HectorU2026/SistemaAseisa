using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Contador,Auxiliar")]
[FiltroCuentaContable]
public class CuentaContableController(
    ICuentaContableFlujo flujo, IUsuarioFlujo usuarioFlujo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] bool soloActivas = false)
    {
        if (await ObtenerContexto() is not { } c) return Forbid();
        return Ok(await flujo.Obtener(c.Empresa, soloActivas: soloActivas));
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> ObtenerDetalle(int id)
    {
        if (await ObtenerContexto() is not { } c) return Forbid();
        var cuenta = (await flujo.Obtener(c.Empresa, id)).SingleOrDefault();
        return cuenta is null ? NotFound() : Ok(cuenta);
    }

    [HttpPost]
    public async Task<IActionResult> Agregar(AgregarCuentaContableRequest datos)
    {
        if (await ObtenerContexto() is not { } c) return Forbid();
        var id = await flujo.Agregar(c.Empresa, c.Usuario, datos);
        return CreatedAtAction(nameof(ObtenerDetalle), new { id }, new { idCuentaContable = id });
    }

    [HttpPost("raiz")]
    [Authorize(Roles = "Administrador,Contador")]
    public async Task<IActionResult> AgregarRaiz(AgregarCuentaRaizRequest datos)
    {
        if (await ObtenerContexto() is not { } c) return Forbid();
        var id = await flujo.AgregarRaiz(c.Empresa, c.Usuario, datos);
        return CreatedAtAction(nameof(ObtenerDetalle), new { id }, new { idCuentaContable = id });
    }

    [HttpPut("{id:int:min(1)}")]
    public async Task<IActionResult> Editar(int id, EditarCuentaContableRequest datos)
    {
        if (await ObtenerContexto() is not { } c) return Forbid();
        var idCuentaContable = await flujo.Editar(c.Empresa, c.Usuario, id, datos);
        return Ok(new { idCuentaContable });
    }

    [HttpPut("{id:int:min(1)}/inhabilitar")]
    [Authorize(Roles = "Administrador,Contador")]
    public async Task<IActionResult> Inhabilitar(int id)
    {
        if (await ObtenerContexto() is not { } c) return Forbid();
        await flujo.Inhabilitar(c.Empresa, c.Usuario, id);
        return NoContent();
    }

    [HttpPut("{id:int:min(1)}/habilitar")]
    [Authorize(Roles = "Administrador,Contador")]
    public async Task<IActionResult> Habilitar(int id)
    {
        if (await ObtenerContexto() is not { } c) return Forbid();
        await flujo.Habilitar(c.Empresa, c.Usuario, id);
        return NoContent();
    }

    private async Task<(int Empresa, int Usuario)?> ObtenerContexto()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuario) || usuario <= 0)
            return null;

        var empresa = await usuarioFlujo.ObtenerEmpresaUsuarioActivo(usuario);
        return empresa is > 0 ? (empresa.Value, usuario) : null;
    }
}

public sealed class FiltroCuentaContableAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        int? estado = context.Exception switch
        {
            ValidationException => 400,
            SqlException { Number: 51401 or 51402 or 51403 or 51407 } => 400,
            SqlException { Number: 51405 } => 404,
            SqlException { Number: 51404 or 51406 or 51408 or 51409 or 51410 } => 409,
            _ => null
        };

        if (estado is null) return;
        context.Result = new ObjectResult(context.Exception.Message) { StatusCode = estado.Value };
        context.ExceptionHandled = true;
    }
}
