using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASEISA.Plataforma;
using Web.Servicios;
namespace Web.Pages.Plataforma;
[Authorize(Roles="Superadministrador")]
public sealed class BitacoraModel:PlataformaPageModel
{
    public BitacoraModel(PlataformaApiService api):base(api){}
    public VistaPlataforma Datos{get;set;}=new();
    [BindProperty(SupportsGet=true)]public string? Busqueda{get;set;}
    [BindProperty(SupportsGet=true)]public string? Estado{get;set;}
    public async Task<IActionResult> OnGetAsync(){try{Datos=await Api.Consultar(Token);return Page();}catch(Exception e){return Error(e);}}
}