using System.ComponentModel.DataAnnotations;
namespace ASEISA.Plataforma
{
    public class PersonaPlataforma : IValidatableObject
    {
        [Required,StringLength(80)]public string Nombre{get;set;}="";
        [Required,StringLength(80)]public string PrimerApellido{get;set;}="";
        [StringLength(80)]public string? SegundoApellido{get;set;}
        [Required,EmailAddress,StringLength(150),ASEISA.Usuarios.CorreoUsuario]public string Correo{get;set;}="";
        [Required]public string TipoCliente{get;set;}="Persona física";
        [Required,StringLength(50)]public string Identificacion{get;set;}="";
        [StringLength(200)]public string? NombreRazonSocial{get;set;}
        [ASEISA.Usuarios.TelefonoUsuario]public string? Telefono{get;set;}
        [StringLength(250)]public string? Direccion{get;set;}
        public IEnumerable<ValidationResult> Validate(ValidationContext context)=>ASEISA.Usuarios.ReglasRegistroUsuario.Identificacion(TipoCliente,Identificacion,NombreRazonSocial,nameof(NombreRazonSocial));
    }
    public class EmpresaPlataforma
    {
        public int IdEmpresa{get;set;}
        [Required,StringLength(200)]public string RazonSocial{get;set;}="";
        [StringLength(200)]public string? NombreComercial{get;set;}
        [Required,RegularExpression(@"^(3[0-9]{9}|3-[0-9]{3}-[0-9]{6})$",ErrorMessage="Ingrese la cédula jurídica: 3-101-123456.")]public string CedulaJuridica{get;set;}="";
        [EmailAddress,StringLength(150),ASEISA.Usuarios.CorreoUsuario]public string? Correo{get;set;}
        [ASEISA.Usuarios.TelefonoUsuario]public string? Telefono{get;set;}
        [StringLength(300)]public string? Direccion{get;set;}
        [Required,StringLength(10)]public string MonedaBase{get;set;}="CRC";
        public bool? Activo{get;set;}
        public string Estado{get;set;}="";
        public int AdministradoresHabilitados{get;set;}
    }
    public class AdministradorPlataforma
    {
        public int IdUsuario{get;set;}public int IdEmpresa{get;set;}
        public string Nombre{get;set;}="";public string PrimerApellido{get;set;}="";public string? SegundoApellido{get;set;}
        public string Correo{get;set;}="";public string NombreUsuario{get;set;}="";public string Estado{get;set;}="";public string EstadoAprobacion{get;set;}="";
    }
    public class EdicionAdministradorPlataforma
    {
        [Required,StringLength(80)]public string Nombre{get;set;}="";
        [Required,StringLength(80)]public string PrimerApellido{get;set;}="";
        [StringLength(80)]public string? SegundoApellido{get;set;}
        [Required,EmailAddress,StringLength(150),ASEISA.Usuarios.CorreoUsuario]public string Correo{get;set;}="";
    }
    public class EventoPlataforma
    {
        public int IdBitacora{get;set;}public int IdUsuario{get;set;}public int? IdEmpresaObjetivo{get;set;}
        public string Tabla{get;set;}="";public string? Registro{get;set;}public string Accion{get;set;}="";public DateTime Fecha{get;set;}
    }
    public class VistaPlataforma
    {
        public List<EmpresaPlataforma> Empresas{get;set;}=new();
        public List<AdministradorPlataforma> Administradores{get;set;}=new();
        public List<EventoPlataforma> Eventos{get;set;}=new();
    }
}