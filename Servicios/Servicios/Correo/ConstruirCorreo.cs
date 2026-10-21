using Servicios.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Servicios.Correo
{
    public interface IConstructorCorreo
    {
        Task<ContenidoCorreo> RecuperarContrasena(string nombre, string enlace);
        Task<ContenidoCorreo> Cotizacion(CotizacionResponseDetallada cotizacion);
    }

    public class ConstructorCorreo : IConstructorCorreo
    {
        private readonly string _rutaPlantillas;

        public ConstructorCorreo()
        {
            _rutaPlantillas = Path.Combine(AppContext.BaseDirectory, "Correo", "Plantillas");
        }

        public async Task<ContenidoCorreo> RecuperarContrasena(string nombre, string enlace)
        {
            var valores = new Dictionary<string, string>
            {
                { "Nombre", nombre },
                { "Enlace", enlace }
            };

            var html = await CargarPlantilla("RecuperarContrasena.html", valores);
            var texto = await CargarPlantilla("RecuperarContrasena.txt", valores);

            return new ContenidoCorreo
            {
                Asunto = "Restablecer Contraseña",
                Html = html,
                TextoPlano = texto
            };
        }

        public async Task<ContenidoCorreo> Cotizacion(CotizacionResponseDetallada cotizacion)
        {
            var valores = new Dictionary<string, string>
            {
                { "NumeroCotizacion", cotizacion.NumeroCotizacion },
                { "NombreCliente", cotizacion.Cliente.NombreRazonSocial ?? "" },
                { "FechaEmision", cotizacion.FechaEmision.ToString("dd/MM/yyyy") },
                { "Total", cotizacion.Total.ToString("C") },
                { "NombreEmpresa", cotizacion.Empresa.NombreComercial ?? "" }
            };

            var html = await CargarPlantilla("Cotizacion.html", valores);
            var texto = await CargarPlantilla("Cotizacion.txt", valores);

            return new ContenidoCorreo
            {
                Asunto = $"Cotización {cotizacion.NumeroCotizacion}",
                Html = html,
                TextoPlano = texto
            };
        }





        private async Task<string> CargarPlantilla(string archivo, Dictionary<string, string> valores)
        {
            var ruta = Path.Combine(_rutaPlantillas, archivo);
            if (!File.Exists(ruta))
                throw new FileNotFoundException($"Plantilla no encontrada: {ruta}");

            var contenido = await File.ReadAllTextAsync(ruta);

            foreach (var (clave, valor) in valores)
                contenido = contenido.Replace($"{{{{{clave}}}}}", valor ?? string.Empty);

            return contenido;
        }
    }
}