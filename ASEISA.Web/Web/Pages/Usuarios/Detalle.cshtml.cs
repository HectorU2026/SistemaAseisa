using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Modelos;
using Web.Servicios;
namespace Web.Pages.Usuarios
{
    [Authorize(Roles="Administrador")]
    public sealed class DetalleModel:UsuariosPageModel
    {
        public DetalleModel(UsuariosApiService api):base(api){}
        public UsuarioAdministrado Usuario{get;set;}=new();
        public CatalogosUsuarios Catalogos{get;set;}=new();
        [BindProperty]public DatosPersonalesUsuario Datos{get;set;}=new();
        private async Task Cargar(int id,bool editar=false)
        {
            if(!IdUsuarioAutenticado.HasValue)throw new UsuariosApiException(System.Net.HttpStatusCode.Unauthorized);
            if(EsCuentaPropia(id))throw new UsuariosApiException(System.Net.HttpStatusCode.Forbidden,"AUTOADMINISTRACION_NO_PERMITIDA");
            Usuario=await Api.Obtener(id,Token);Catalogos=await Api.Catalogos(Token);
            if(editar)Datos=new(){Nombre=Usuario.Nombre,PrimerApellido=Usuario.PrimerApellido,SegundoApellido=Usuario.SegundoApellido,Correo=Usuario.Correo,NombreUsuario=Usuario.NombreUsuario};
        }
        public string Estado=>Catalogos.Estados.FirstOrDefault(e=>e.Id==Usuario.IdEstado)?.Nombre??"No disponible";
        public async Task<IActionResult> OnGetAsync(int id){try{await Cargar(id,true);return Page();}catch(Exception e){return Error(e);}}
        public async Task<IActionResult> OnPostEditarAsync(int id)
        {
            try{await Cargar(id);if(!ModelState.IsValid)return Page();await Api.Editar(id,Datos,Token);TempData["Mensaje"]="Datos actualizados.";return RedirectToPage(new{id});}
            catch(Exception e){return Error(e);}
        }
        public async Task<IActionResult> OnPostAccionAsync(int id,string accion,int? rol,bool? valor)
        {
            ModelState.Clear();
            try
            {
                await Cargar(id,true);
                switch(accion)
                {
                    case "estado" when valor.HasValue && Usuario.EstadoAprobacion=="Aprobado":await Api.Estado(id,valor.Value,Token);TempData["Mensaje"]=valor.Value?"Usuario activado.":"Usuario desactivado.";break;
                    case "aprobacion" when valor.HasValue && Usuario.EstadoAprobacion=="Pendiente":await Api.Aprobar(id,valor.Value,Token);TempData["Mensaje"]=valor.Value?"Solicitud aprobada.":"Solicitud rechazada.";break;
                    case "agregar" when rol>0 && Usuario.EstadoAprobacion=="Aprobado":await Api.Rol(id,rol.Value,true,Token);TempData["Mensaje"]="Rol agregado.";break;
                                        case "retirar" when rol>0 && Usuario.EstadoAprobacion=="Aprobado":
                        if(Usuario.Roles.Any(r=>r.IdRol==rol.Value) && Usuario.Roles.Select(r=>r.IdRol).Distinct().Count()<=1)
                        {ModelState.AddModelError("","No es posible retirar este rol. El usuario debe conservar al menos un rol activo.");return Page();}
                        await Api.Rol(id,rol.Value,false,Token);TempData["Mensaje"]="Rol retirado.";break;
                    case "restablecer" when Usuario.EstadoAprobacion=="Aprobado" && Estado=="Activo" :
                        var envio=await Api.Restablecer(id,Token);
                        TempData["Mensaje"]=envio.EnvioProgramado?"Solicitud registrada. El envio al correo registrado quedo programado.":"No se programo un envio. Revise el estado de la cuenta.";
                        return RedirectToPage(new{id});
                    default:return BadRequest();
                }
                return RedirectToPage(new{id});
            }
            catch(Exception e){return Error(e);}
        }
    }
}