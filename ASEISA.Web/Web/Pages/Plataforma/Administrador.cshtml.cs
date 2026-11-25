using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ASEISA.Plataforma;
using Web.Servicios;
namespace Web.Pages.Plataforma;
[Authorize(Roles="Superadministrador")]
public sealed class AdministradorModel:PlataformaPageModel
{
    public AdministradorModel(PlataformaApiService api):base(api){}
    [BindProperty,ValidateNever]public PersonaPlataforma Alta{get;set;}=new();
    [BindProperty,ValidateNever]public EdicionAdministradorPlataforma Edicion{get;set;}=new();
    public VistaPlataforma Vista{get;set;}=new();
    public AdministradorPlataforma? Usuario{get;set;}
    public EmpresaPlataforma Empresa=>Vista.Empresas.FirstOrDefault()??new();
    private async Task Cargar(int empresa,int? id)
    {
        Vista=await Api.Consultar(Token,empresa);
        if(id.HasValue)Usuario=Vista.Administradores.FirstOrDefault(u=>u.IdUsuario==id)??throw new UsuariosApiException(System.Net.HttpStatusCode.NotFound);
    }
    public async Task<IActionResult> OnGetAsync(int empresa,int? id)
    {try{await Cargar(empresa,id);if(Usuario!=null)Edicion=new(){Nombre=Usuario.Nombre,PrimerApellido=Usuario.PrimerApellido,SegundoApellido=Usuario.SegundoApellido,Correo=Usuario.Correo};return Page();}catch(Exception e){return Error(e);}}
    public async Task<IActionResult> OnPostGuardarAsync(int empresa,int? id)
    {
        try{
            await Cargar(empresa,id);ModelState.Clear();
            if(id.HasValue){if(!TryValidateModel(Edicion,nameof(Edicion)))return Page();await Api.Editar(Token,empresa,id.Value,Edicion);TempData["Mensaje"]="Datos personales del administrador actualizados.";}
            else{if(!TryValidateModel(Alta,nameof(Alta)))return Page();await Api.Registrar(Token,empresa,Alta);TempData["Mensaje"]="Administrador registrado. Invitación programada; no implica entrega confirmada.";}
            return RedirectToPage("Empresa",new{id=empresa});
        }catch(Exception e){return Error(e);}
    }
    public async Task<IActionResult> OnPostAccionAsync(int empresa,int id,string accion,bool? activo)
    {
        ModelState.Clear();try{
            await Cargar(empresa,id);Edicion=new(){Nombre=Usuario!.Nombre,PrimerApellido=Usuario.PrimerApellido,SegundoApellido=Usuario.SegundoApellido,Correo=Usuario.Correo};
            if(accion=="enlace"){await Api.Enlace(Token,empresa,id);TempData["Mensaje"]="Enlace de contraseña programado; no implica entrega confirmada.";}
            else if(accion=="estado" && activo.HasValue){await Api.Estado(Token,empresa,id,activo.Value);TempData["Mensaje"]=activo.Value?"Administrador activado.":"Administrador desactivado.";}
            else return BadRequest();
            return RedirectToPage(new{empresa,id});
        }catch(Exception e){return Error(e);}
    }
}