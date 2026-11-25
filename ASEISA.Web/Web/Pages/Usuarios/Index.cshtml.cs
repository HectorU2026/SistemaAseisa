using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Modelos;
using Web.Servicios;
namespace Web.Pages.Usuarios
{
    [Authorize(Roles="Administrador")]
    public sealed class IndexModel:UsuariosPageModel
    {
        public IndexModel(UsuariosApiService api):base(api){}
        public List<UsuarioAdministrado> Usuarios{get;set;}=new();
        public CatalogosUsuarios Catalogos{get;set;}=new();
        [BindProperty(SupportsGet=true)]public bool Pendientes{get;set;}
        [BindProperty(SupportsGet=true)]public string? Busqueda{get;set;}
        [BindProperty(SupportsGet=true)]public int? IdEstado{get;set;}
        [BindProperty(SupportsGet=true)]public string? Aprobacion{get;set;}
        [BindProperty(SupportsGet=true)]public int? Rol{get;set;}
        [BindProperty(SupportsGet=true)]public int Pagina{get;set;}=1;
        public int Total{get;private set;}
        public int Paginas=>Math.Max(1,(Total+14)/15);
        public string Estado(int id)=>Catalogos.Estados.FirstOrDefault(e=>e.Id==id)?.Nombre??"No disponible";
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Catalogos=await Api.Catalogos(Token);
                IEnumerable<UsuarioAdministrado> consulta=await Api.Listar(Token);
                if(Pendientes)consulta=consulta.Where(u=>u.EstadoAprobacion=="Pendiente");
                if(!string.IsNullOrWhiteSpace(Busqueda))consulta=consulta.Where(u=>$"{u.Nombre} {u.PrimerApellido} {u.SegundoApellido} {u.Correo} {u.NombreUsuario}".Contains(Busqueda.Trim(),StringComparison.OrdinalIgnoreCase));
                if(IdEstado>0)consulta=consulta.Where(u=>u.IdEstado==IdEstado);
                if(!string.IsNullOrEmpty(Aprobacion))consulta=consulta.Where(u=>u.EstadoAprobacion==Aprobacion);
                if(Rol>0)consulta=consulta.Where(u=>u.Roles.Any(r=>r.IdRol==Rol));
                var filtrados=consulta.OrderBy(u=>u.Nombre).ThenBy(u=>u.PrimerApellido).ThenBy(u=>u.IdUsuario).ToList();
                Total=filtrados.Count;Pagina=Math.Clamp(Pagina,1,Paginas);
                Usuarios=filtrados.Skip((Pagina-1)*15).Take(15).ToList();return Page();
            }
            catch(Exception e){return Error(e);}
        }
    }
}