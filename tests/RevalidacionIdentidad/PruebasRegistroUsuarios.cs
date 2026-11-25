using ASEISA.Usuarios;
using System.ComponentModel.DataAnnotations;
internal static class PruebasRegistroUsuarios
{
    public static int Ejecutar()
    {
        int casos=0;
        void Caso(bool cumple,string nombre){if(!cumple)throw new Exception(nombre);Console.WriteLine("OK "+nombre);casos++;}
        Caso(ReglasRegistroUsuario.Generar("Ángela","Álvarez Mora","1-2345-6789")=="aalvarez.6789","Identidad automática conserva regla pública y normaliza acentos");
        Caso(ReglasRegistroUsuario.Generar("","Álvarez","123456789")=="","Datos incompletos no generan identidad");
        Caso(!ReglasRegistroUsuario.Identificacion("Persona física","1-2345-6789",null,"RazonSocial").Any(),"Cédula física con separadores válida");
        Caso(ReglasRegistroUsuario.Identificacion("Persona física","023456789",null,"RazonSocial").Any(),"Cédula física inválida rechazada");
        Caso(ReglasRegistroUsuario.Identificacion("Persona jurídica","3123456789",null,"RazonSocial").Any(),"Persona jurídica exige razón social");
        Caso(!ReglasRegistroUsuario.Identificacion("Persona jurídica","3123456789","Empresa ficticia","RazonSocial").Any(),"Persona jurídica completa válida");
        Caso(ReglasRegistroUsuario.Identificacion("Tipo libre","123456789",null,"RazonSocial").Any(),"Tipo desconocido rechazado");
        Caso(ReglasRegistroUsuario.Roles(Array.Empty<int>(),0).Any(),"Alta sin roles rechazada");
        Caso(!ReglasRegistroUsuario.Roles(new[]{7,9},0).Any(),"Alta multirrol válida sin depender de IDs fijos");
        Caso(ReglasRegistroUsuario.Roles(new[]{7,7},0).Any(),"Roles duplicados rechazados");
        Caso(ReglasRegistroUsuario.Roles(new[]{0,7},0).Any(),"Rol no positivo rechazado");
        Caso(!ReglasRegistroUsuario.Roles(null,7).Any(),"Contrato anterior de rol único compatible");
#if ASEISA
        foreach(bool api in new[]{true,false})
        {
            object solicitud=api?new Abstracciones.Modelos.UsuarioRequest {Nombre="Prueba",PrimerApellido="Apellido",Correo="prueba@example.invalid",TipoCliente="Persona física",Identificacion="123456789",IdEstado=9,IdsRoles=new(){7,8}}
                :new Web.Modelos.FormularioUsuario {Nombre="Prueba",PrimerApellido="Apellido",Correo="prueba@example.invalid",TipoCliente="Persona física",Identificacion="123456789",IdEstado=9,IdsRoles=new(){7,8}};
            var resultados=new List<ValidationResult>();
            Caso(Validator.TryValidateObject(solicitud,new ValidationContext(solicitud),resultados,true),"Contrato "+(api?"API":"Web")+" admite multirrol y nombre generado por servidor");
        }
        var invalido=new Web.Modelos.FormularioUsuario {Nombre=new string('A',81),PrimerApellido="Apellido",Correo="invalido",TipoCliente="Persona física",Identificacion="123456789",IdEstado=9,IdsRoles=new(){7}};
        var errores=new List<ValidationResult>();Validator.TryValidateObject(invalido,new ValidationContext(invalido),errores,true);
        Caso(errores.Any(e=>e.MemberNames.Contains("Nombre")) && errores.Any(e=>e.MemberNames.Contains("Correo")),"Validaciones de longitud y correo identifican campos");
#endif
        Caso(ReglasRegistroUsuario.Identificacion("Persona física","1--2345-6789",null,"RazonSocial").Any(),"Backend rechaza guiones mal colocados");
        Caso(ReglasRegistroUsuario.Identificacion("Persona física","1-2345-67890",null,"RazonSocial").Any(),"Backend rechaza exceso de dígitos");
        Caso(ReglasRegistroUsuario.Identificacion("Persona jurídica","3-101-123456","Prueba","RazonSocial").Count()==0,"Cédula jurídica enmascarada válida");
        Caso(ReglasRegistroUsuario.Identificacion("Persona física","1-2345-ABCD",null,"RazonSocial").Any(),"Backend rechaza letras en identificación");
        Caso(ReglasRegistroUsuario.NormalizarIdentificacion(" 1-2345-6789 ")=="123456789","Identificación se almacena sin guiones");
        Caso(ReglasRegistroUsuario.NormalizarTelefono("12345678")=="1234-5678","Teléfono normalizado como registro público");
        var formatoCorreo=new CorreoUsuarioAttribute();
        Caso(formatoCorreo.IsValid("ficticio@example.invalid") && !formatoCorreo.IsValid("ficticio@sin-dominio"),"Correo coincide con la regla pública JavaScript");
        var formatoTelefono=new TelefonoUsuarioAttribute();
        Caso(formatoTelefono.IsValid(null) && formatoTelefono.IsValid("1234-5678") && formatoTelefono.IsValid("12345678"),"Teléfono opcional de ocho dígitos válido");
        Caso(!formatoTelefono.IsValid("1234") && !formatoTelefono.IsValid("123456789") && !formatoTelefono.IsValid("ABCD-5678"),"Backend rechaza teléfonos incompletos o con caracteres inválidos");
#if ASEISA
        foreach(var solicitud in new object[]{new Web.Modelos.DatosPersonalesUsuario {Nombre=new string('A',81),PrimerApellido="Apellido",Correo="ficticio@localhost",NombreUsuario="inmutable"},new Abstracciones.Modelos.UsuarioEditarRequest {Nombre=new string('A',81),PrimerApellido="Apellido",Correo="ficticio@localhost",NombreUsuario="inmutable"}})
        {
            var resultados=new List<ValidationResult>();Validator.TryValidateObject(solicitud,new ValidationContext(solicitud),resultados,true);
            Caso(resultados.Any(e=>e.MemberNames.Contains("Nombre")) && resultados.Any(e=>e.MemberNames.Contains("Correo")),"Edición valida longitud y correo sin depender de JavaScript");
        }
        var inyeccion=System.Text.Json.JsonSerializer.Deserialize<Abstracciones.Modelos.UsuarioRequest>("{\"IdEmpresa\":999,\"Nombre\":\"Prueba\"}");
        Caso(inyeccion!=null && inyeccion.GetType().GetProperty("IdEmpresa")==null,"Empresa enviada por cliente no forma parte del contrato administrativo");
#endif
        return casos;
    }
}