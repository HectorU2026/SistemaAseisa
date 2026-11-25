using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace ASEISA.Usuarios
{
    public sealed class CorreoUsuarioAttribute : RegularExpressionAttribute
    {
        public CorreoUsuarioAttribute():base(@"^[^\s@]+@[^\s@]+\.[^\s@]+$") {ErrorMessage="Ingrese un correo electrónico válido.";}
    }
    public sealed class TelefonoUsuarioAttribute : RegularExpressionAttribute
    {
        public TelefonoUsuarioAttribute():base(@"^[0-9]{4}-?[0-9]{4}$") {ErrorMessage="El teléfono debe contener 8 dígitos (1234-5678).";}
    }
    // Fuente enlazada: ambas API y Web comparten la generación de identidad y las reglas del formulario.
    public static class ReglasRegistroUsuario
    {
        public static string NormalizarIdentificacion(string? valor)=>(valor??"").Replace("-","").Trim();
        public static string? NormalizarTelefono(string? valor)
        {
            if(string.IsNullOrWhiteSpace(valor))return null;
            var digitos=valor.Trim().Replace("-","");
            return digitos.Length==8?digitos.Substring(0,4)+"-"+digitos.Substring(4):valor.Trim();
        }
        public static string Generar(string? nombre,string? apellido,string? identificacion)
        {
            var digitos=new string((identificacion??"").Where(char.IsDigit).ToArray());
            if(string.IsNullOrWhiteSpace(nombre)||string.IsNullOrWhiteSpace(apellido)||digitos.Length<4)return "";
            var usuario=$"{nombre.Trim()[0]}{apellido.Trim().Split(' ')[0]}.{digitos.Substring(digitos.Length-4)}";
            return new string(usuario.Normalize(NormalizationForm.FormD).Where(c=>CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark).ToArray()).ToLowerInvariant();
        }
        public static IEnumerable<ValidationResult> Identificacion(string? tipo,string? identificacion,string? razon,string campoRazon)
        {
            var original=(identificacion??"").Trim();
            var valor=NormalizarIdentificacion(original);
            if(tipo!="Persona física" && tipo!="Persona jurídica")
                yield return new ValidationResult("Seleccione un tipo de cliente válido.",new[]{"TipoCliente"});
            else if(!Regex.IsMatch(valor,tipo=="Persona física"?@"^[1-9][0-9]{8}$":@"^3[0-9]{9}$")
                || (original.Contains("-") && !Regex.IsMatch(original,tipo=="Persona física"?@"^[1-9]-[0-9]{4}-[0-9]{4}$":@"^3-[0-9]{3}-[0-9]{6}$")))
                yield return new ValidationResult(tipo=="Persona física"?"La cédula física debe contener 9 dígitos y comenzar del 1 al 9.":"La cédula jurídica debe contener 10 dígitos y comenzar con 3.",new[]{"Identificacion"});
            if(tipo=="Persona jurídica" && string.IsNullOrWhiteSpace(razon))
                yield return new ValidationResult("La razón social es obligatoria para personas jurídicas.",new[]{campoRazon});
        }
        public static IEnumerable<ValidationResult> Roles(IEnumerable<int>? roles,int rolCompatible)
        {
            var seleccion=roles?.ToArray()??Array.Empty<int>();
            if(seleccion.Length==0 && rolCompatible>0)seleccion=new[]{rolCompatible};
            if(seleccion.Length==0 || seleccion.Any(r=>r<=0) || seleccion.Distinct().Count()!=seleccion.Length)
                yield return new ValidationResult("Seleccione al menos un rol válido, sin duplicados.",new[]{"IdsRoles"});
        }
    }
}