using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Servicios;
using System.Net;
namespace Web.Pages.Usuarios
{
    public abstract class UsuariosPageModel:PageModel
    {
        protected readonly UsuariosApiService Api;
        protected UsuariosPageModel(UsuariosApiService api){Api=api;}
        protected int? IdUsuarioAutenticado=>int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,out var id) && id>0?id:null;
        public bool EsCuentaPropia(int id)=>IdUsuarioAutenticado==id;
        protected string Token=>User.FindFirst("Token")?.Value??"";
        protected IActionResult Error(Exception ex)
        {
            if(ex is UsuariosApiException e)
            {
                if(e.Estado==HttpStatusCode.Unauthorized)return Challenge();
                if(e.Estado==HttpStatusCode.Forbidden && e.Codigo=="AUTOADMINISTRACION_NO_PERMITIDA")
                {TempData["Aviso"]="No podés administrar tu propia cuenta desde este módulo.";return RedirectToPage("/Usuarios/Index");}
                if(e.Estado==HttpStatusCode.Forbidden)return Forbid();
                if(e.Estado==HttpStatusCode.NotFound)return NotFound();
                var mensaje=e.Codigo switch {
                    "ULTIMO_ROL"=>"No es posible retirar este rol. El usuario debe conservar al menos un rol activo.",
                    "USUARIO_INMUTABLE"=>"El nombre de usuario no puede modificarse.",
                    "CORREO_DUPLICADO"=>"El correo electrónico ya está registrado.",
                    "USUARIO_DUPLICADO"=>"El nombre de usuario generado ya está registrado. Consulte con la administración.",
                    "ROLES_INVALIDOS"=>"Seleccione al menos un rol activo y permitido, sin duplicados.",
                    "IDENTIFICACION_INVALIDA"=>"Revise la identificación, su tipo y la razón social.",
                    "DATOS_INVALIDOS"=>"Revise los datos personales y la longitud de los campos.",_=>null};
                if(mensaje!=null){var campo=e.Codigo switch {"CORREO_DUPLICADO"=>"Correo","ROLES_INVALIDOS"=>"IdsRoles","IDENTIFICACION_INVALIDA"=>"Identificacion",_=>""};
                    var prefijo=this is CrearModel?"Formulario.":"Datos.";
                    ModelState.AddModelError(campo==""?"":prefijo+campo,mensaje);return Page();}
                if(e.Estado==HttpStatusCode.Conflict){ModelState.AddModelError("","El correo o nombre de usuario ya esta registrado.");return Page();}
                if(e.Estado==HttpStatusCode.BadRequest){ModelState.AddModelError("","La operacion no es valida para el usuario, rol o estado seleccionado.");return Page();}
            }
            ModelState.AddModelError("","Servicio temporalmente no disponible. Verifique el resultado antes de reintentar.");return Page();
        }
    }
}