using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ASEISA.Plataforma;
using Web.Servicios;
namespace Web.Pages.Plataforma;
[Authorize(Roles="Superadministrador")]
public sealed class EmpresaModel:PlataformaPageModel
{
    public EmpresaModel(PlataformaApiService api):base(api){}
    [BindProperty,ValidateNever]public EmpresaPlataforma Datos{get;set;}=new();
    public VistaPlataforma Vista{get;set;}=new();
    public async Task<IActionResult> OnGetAsync(int? id){try{if(id.HasValue){Vista=await Api.Consultar(Token,id);Datos=Vista.Empresas.Single();}return Page();}catch(Exception e){return Error(e);}}
    public async Task<IActionResult> OnPostGuardarAsync(int? id)
    {try{if(id.HasValue)Vista=await Api.Consultar(Token,id);if(!TryValidateModel(Datos,nameof(Datos)))return Page();Datos.Activo=null;var resultado=await Api.Guardar(Token,id,Datos);TempData["Mensaje"]="Datos de empresa guardados.";return RedirectToPage(new{id=resultado});}catch(Exception e){return Error(e);}}
    public async Task<IActionResult> OnPostEstadoAsync(int id,bool activo)
    {ModelState.Clear();try{Vista=await Api.Consultar(Token,id);Datos=Vista.Empresas.Single();await Api.EstadoEmpresa(Token,id,activo);TempData["Mensaje"]=activo?"Empresa activada.":"Empresa desactivada; sus usuarios perderán acceso en la siguiente revalidación.";return RedirectToPage(new{id});}catch(Exception e){return Error(e);}}
}