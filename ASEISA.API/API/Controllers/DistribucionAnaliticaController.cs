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
[FiltroDistribucionAnalitica]
public class DistribucionAnaliticaController(
    IDistribucionAnaliticaFlujo flujo, IUsuarioFlujo usuarioFlujo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        var contexto = await ObtenerContexto();
        return contexto is { } c ? Ok(await flujo.Obtener(c.Empresa)) : Forbid();
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> ObtenerDetalle(int id)
    {
        if (await ObtenerContexto() is not { } c) return Forbid();
        var registro = (await flujo.Obtener(c.Empresa, id)).SingleOrDefault();
        return registro is null ? NotFound() : Ok(registro);
    }

    [HttpPost]
    public async Task<IActionResult> Agregar(DistribucionAnaliticaRequest datos)
    {
        if (await ObtenerContexto() is not { } c) return Forbid();
        var id = await flujo.Agregar(c.Empresa, c.Usuario, datos);
        return CreatedAtAction(nameof(ObtenerDetalle), new { id }, new { idDistribucion = id });
    }

    [HttpPut("{id:int:min(1)}")]
    public async Task<IActionResult> Editar(int id, DistribucionAnaliticaRequest datos)
    {
        if (await ObtenerContexto() is not { } c) return Forbid();
        var resultado = await flujo.Editar(c.Empresa, c.Usuario, id, datos);
        return Ok(new { idDistribucion = resultado });
    }

    [HttpPut("{id:int:min(1)}/inhabilitar")]
    [Authorize(Roles = "Administrador,Contador")]
    public async Task<IActionResult> Inhabilitar(int id)
    {
        if (await ObtenerContexto() is not { } c) return Forbid();
        await flujo.Inhabilitar(c.Empresa, c.Usuario, id);
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

public sealed class FiltroDistribucionAnaliticaAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        int? codigo = context.Exception switch
        {
            ValidationException => 400,
            SqlException { Number: 51301 } => 404,
            SqlException { Number: 51302 } => 409,
            SqlException { Number: 51303 or 51304 or 51305 } => 400,
            _ => null
        };
        if (codigo is null) return;
        context.Result = new ObjectResult(context.Exception.Message) { StatusCode = codigo.Value };
        context.ExceptionHandled = true;
    }
}
