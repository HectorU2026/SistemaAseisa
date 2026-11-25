using API.Seguridad;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

internal static class PruebasAutoadministracion
{
    public static async Task<int> Ejecutar()
    {
        int casos=0;
        async Task Caso(string accion,string? actor,int? objetivo,int? esperado)
        {
            var http=new DefaultHttpContext();
            var claims=new List<Claim> {new(ClaimTypes.Name,"nombre-irrelevante"),new(ClaimTypes.Email,"ficticio@example.invalid")};
            if(actor!=null)claims.Add(new(ClaimTypes.NameIdentifier,actor));
            http.User=new ClaimsPrincipal(new ClaimsIdentity(claims,"Prueba"));
            var action=new ActionContext(http,new RouteData(),new ControllerActionDescriptor {ActionName=accion},new ModelStateDictionary());
            var argumentos=new Dictionary<string,object?>();
            if(objetivo.HasValue)argumentos["idUsuario"]=objetivo.Value;
            var context=new ActionExecutingContext(action,new List<IFilterMetadata>(),argumentos,new object());
            bool ejecutado=false;
            await new ImpedirAutoadministracionAttribute().OnActionExecutionAsync(context,()=>{
                ejecutado=true;return Task.FromResult(new ActionExecutedContext(action,new List<IFilterMetadata>(),new object()));});
            int? estado=context.Result switch {ObjectResult r=>r.StatusCode,StatusCodeResult r=>r.StatusCode,_=>null};
            if(estado!=esperado || ejecutado!=(esperado==null))throw new Exception("Autoadministracion: "+accion);
            Console.WriteLine("OK Autoadministración "+accion+"; operación ejecutada="+ejecutado);casos++;
        }
        foreach(var accion in new[]{"ObtenerUsuarioPorId","EditarUsuario","CambiarEstadoUsuario","AsignarRolUsuario","RetirarRol","AprobarRegistroCliente","Restablecer"})
            await Caso(accion,"7",7,403);
        await Caso("Administrar otra cuenta","7",8,null);
        await Caso("Sin identificador autenticado",null,7,401);
        await Caso("Identificador autenticado inválido","invalido",7,401);
        await Caso("Listado y creación permanecen disponibles","7",null,null);
        return casos;
    }
}