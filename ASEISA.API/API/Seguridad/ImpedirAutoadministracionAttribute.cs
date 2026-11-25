using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace API.Seguridad
{
    // Compartido por los controladores administrativos de ambas API.
    // Sólo examina el objetivo de la ruta; la identidad procede del JWT revalidado.
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ImpedirAutoadministracionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ActionArguments.TryGetValue("idUsuario", out var objetivo) || objetivo is not int idUsuario)
                return;

            if (!int.TryParse(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var actor) || actor<=0)
            {
                context.Result=new UnauthorizedResult();
                return;
            }
            if (actor==idUsuario)
                context.Result=new ObjectResult(new {
                    codigo="AUTOADMINISTRACION_NO_PERMITIDA",
                    mensaje="No podés administrar tu propia cuenta desde este módulo."
                }) {StatusCode=StatusCodes.Status403Forbidden};
        }
    }
}