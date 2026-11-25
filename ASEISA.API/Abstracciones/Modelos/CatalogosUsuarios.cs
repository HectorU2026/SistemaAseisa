namespace Abstracciones.Modelos
{
    public class OpcionUsuario { public int Id {get;set;} public string Nombre {get;set;}=""; }
    public class CatalogosUsuarios
    {
        public string NombreEmpresa {get;set;}="";
        public List<OpcionUsuario> Roles {get;set;}=new();
        public List<OpcionUsuario> Estados {get;set;}=new();
    }
    public class ResultadoInvitacion
    {
        public int IdUsuario {get;set;}
        public bool EnvioProgramado { get; set; }
        public bool EnlaceEnviado {get;set;}
    }
}