using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Servicios;
using System.Net;
namespace Web.Pages.Plataforma;
public abstract class PlataformaPageModel:PageModel
{
    protected readonly PlataformaApiService Api;
    protected PlataformaPageModel(PlataformaApiService api){Api=api;}
    protected string Token=>User.FindFirst("Token")?.Value??"";
    protected IActionResult Error(Exception error)
    {
        if(error is UsuariosApiException api){if(api.Estado==HttpStatusCode.Unauthorized)return Challenge();if(api.Estado==HttpStatusCode.Forbidden)return Forbid();if(api.Estado==HttpStatusCode.NotFound)return NotFound();
            if(api.Estado==HttpStatusCode.Conflict){ModelState.AddModelError("","Los datos ya están registrados.");return Page();}
            if(api.Estado==HttpStatusCode.BadRequest){ModelState.AddModelError("","No se pudo completar la operación. Verifique los datos; una empresa activa debe conservar un administrador habilitado.");return Page();}}
        ModelState.AddModelError("","Servicio temporalmente no disponible. Verifique el resultado antes de reintentar.");return Page();
    }
}