using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace API.Controllers
{
    [ApiController,Authorize,Route("api/Sesion")]
    public sealed class SesionController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Obtener()
        {
            var resultado=await HttpContext.AuthenticateAsync();
            Response.Headers.CacheControl="no-store";
            return Ok(new {
                IdUsuario=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                IdEmpresa=int.Parse(User.FindFirstValue("Empresa")!),
                NombreUsuario=User.FindFirstValue(ClaimTypes.Name),Correo=User.FindFirstValue(ClaimTypes.Email),
                Roles=User.FindAll(ClaimTypes.Role).Select(c=>c.Value).Distinct().ToArray(),
                ExpiraUtc=resultado.Properties?.ExpiresUtc
            });
        }
    }
}