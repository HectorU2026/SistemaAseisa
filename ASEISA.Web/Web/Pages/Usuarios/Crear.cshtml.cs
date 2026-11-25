using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Modelos;
using Web.Servicios;
namespace Web.Pages.Usuarios
{
    [Authorize(Roles="Administrador")]
    public sealed class CrearModel:UsuariosPageModel
    {
        public CrearModel(UsuariosApiService api):base(api){}
        [BindProperty]public FormularioUsuario Formulario{get;set;}=new();
        public CatalogosUsuarios Catalogos{get;set;}=new();
        public async Task<IActionResult> OnGetAsync(){try{Catalogos=await Api.Catalogos(Token);return Page();}catch(Exception e){return Error(e);}}
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                Catalogos=await Api.Catalogos(Token);
                if(!ModelState.IsValid)return Page();
                if(Formulario.IdsRoles.Any(id=>!Catalogos.Roles.Any(r=>r.Id==id)))
                {ModelState.AddModelError("Formulario.IdsRoles","Seleccione roles disponibles en el catálogo.");return Page();}
                Formulario.NombreUsuario=ASEISA.Usuarios.ReglasRegistroUsuario.Generar(Formulario.Nombre,Formulario.PrimerApellido,Formulario.Identificacion);
                var resultado=await Api.Registrar(Formulario,Token);
                TempData["Mensaje"]=resultado.EnvioProgramado?"Usuario creado. El envio de instrucciones quedo programado.":"Usuario creado inactivo. Active la cuenta y solicite un enlace desde el detalle.";
                return RedirectToPage("Detalle",new{id=resultado.IdUsuario});
            }
            catch(Exception e){return Error(e);}
        }
    }
}