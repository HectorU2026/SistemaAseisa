using Microsoft.Extensions.Configuration;
using Abstracciones.DA;
using Abstracciones.Reglas;
namespace API.Seguridad
{
    public sealed class DespachadorEnlaces:BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<DespachadorEnlaces> _logger;
        private readonly IConfiguration? _configuracion;
        public DespachadorEnlaces(IServiceScopeFactory scopes,ILogger<DespachadorEnlaces> logger,IConfiguration? configuracion=null){_scopes=scopes;_logger=logger;_configuracion=configuracion;}
        internal async Task<bool> Procesar(CancellationToken ct)
        {
            using var scope=_scopes.CreateScope();
            var protector=scope.ServiceProvider.GetRequiredService<IProtectorEnlace>();
            var entrega=scope.ServiceProvider.GetRequiredService<IEntregaEnlace>();
            protector.ValidarConfiguracion();entrega.ValidarConfiguracion();
            var da=scope.ServiceProvider.GetRequiredService<ICorreoEnlaceDA>();
            var correo=await da.Tomar(ct);
            if(correo==null)return false;
            if(!correo.Procesable)return true; // SQL ya anulo/finalizo este trabajo; continuar sin demora.
            var enviado=false;
            try
            {
                var token=protector.Descifrar(correo.TokenCifrado,correo.HashToken);
                enviado=await entrega.Enviar(correo,token);
            }
            catch(Exception){_logger.LogWarning("No se pudo procesar un envio de enlace.");}
            ct.ThrowIfCancellationRequested();
            await da.Finalizar(correo.IdCorreo,correo.Reserva,enviado,ct);
            return true;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try{if(_configuracion?.GetValue<bool>("CorreoEnlaces:EnviosHabilitados")==true && await Procesar(stoppingToken))continue;}
                catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested){break;}
                catch(Exception){_logger.LogWarning("Despachador de enlaces temporalmente no disponible.");}
                try{await Task.Delay(TimeSpan.FromSeconds(5),stoppingToken);}
                catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested){break;}
            }
        }
    }
}