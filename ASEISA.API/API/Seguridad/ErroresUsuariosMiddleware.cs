using Microsoft.Data.SqlClient;
namespace API.Seguridad
{
    public sealed class ErroresUsuariosMiddleware
    {
        private readonly RequestDelegate _siguiente;
        private readonly ILogger<ErroresUsuariosMiddleware> _logger;
        public ErroresUsuariosMiddleware(RequestDelegate siguiente,ILogger<ErroresUsuariosMiddleware> logger){_siguiente=siguiente;_logger=logger;}
        public async Task InvokeAsync(HttpContext context)
        {
            if(!context.Request.Path.StartsWithSegments("/api/Usuario") && !context.Request.Path.StartsWithSegments("/api/Plataforma")){await _siguiente(context);return;}
            try { await _siguiente(context); }
            catch(Exception ex) when(!context.Response.HasStarted && !context.RequestAborted.IsCancellationRequested)
            {
                var status=StatusCodes.Status503ServiceUnavailable;
                if(ex is SqlException sql)
                    status=sql.Number is 2601 or 2627 or 51016 or 51017 or 51032 or 51033 ? 409
                        : sql.Number is 51025 or 51026 or 51027 or 51035 or 51036 or 51037 or 51102 ? 403
                        : sql.Number==51015 ? 404 : sql.Number>=51000 && sql.Number<52000 ? 400 : 503;
                else if(ex is HttpRequestException http && http.StatusCode is {} code)
                    status=(int)code is 400 or 401 or 403 or 404 or 409 or 429 ? (int)code : 503;
                else if(ex is ArgumentException) status=400;
                _logger.LogWarning(new EventId(1004,"OperacionUsuarioRechazada"),"Operacion de usuarios no completada. Estado {Estado}. Tipo {Tipo}. Codigo SQL {Numero}. Correlacion {Id}.",status,ex.GetType().Name,ex is SqlException detalle?detalle.Number:0,context.TraceIdentifier);
                context.Response.Clear();context.Response.StatusCode=status;
                if(status==401)context.Response.Headers.WWWAuthenticate="Bearer";
                await context.Response.WriteAsJsonAsync(new {codigo=ex is SqlException controlado ? controlado.Number switch {51210=>"USUARIO_INMUTABLE",51211=>"ULTIMO_ROL",51303=>"ULTIMO_ADMINISTRADOR",51016 or 51032=>"CORREO_DUPLICADO",51017 or 51033=>"USUARIO_DUPLICADO",51031 or 51214=>"ROLES_INVALIDOS",51212=>"DATOS_INVALIDOS",51213=>"IDENTIFICACION_INVALIDA",_=>"OPERACION_RECHAZADA"} : ex is HttpRequestException servicio && (servicio.Message is "CORREO_DUPLICADO" or "USUARIO_DUPLICADO" or "ROLES_INVALIDOS" or "DATOS_INVALIDOS" or "IDENTIFICACION_INVALIDA") ? servicio.Message : "OPERACION_RECHAZADA",mensaje=status==503 ? "Servicio temporalmente no disponible." : "No fue posible completar la operacion con los datos y permisos indicados."});
            }
        }
    }
}