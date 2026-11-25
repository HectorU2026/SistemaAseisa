using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASEISA.Plataforma;
using Web.Servicios;
namespace Web.Pages.Plataforma;
[Authorize(Roles="Superadministrador")]
public sealed class IndexModel:PlataformaPageModel
{
    public IndexModel(PlataformaApiService api):base(api){}
    public VistaPlataforma Datos{get;set;}=new();
    public async Task<IActionResult> OnGetAsync(){try{Datos=await Api.Consultar(Token);return Page();}catch(Exception e){return Error(e);}}
}