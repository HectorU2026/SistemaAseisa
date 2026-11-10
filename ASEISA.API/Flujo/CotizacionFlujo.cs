using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Interfaces.Servicio;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static Abstracciones.Modelos.CotizacionResponseDetallada;
using static Abstracciones.Modelos.Facturacion;

namespace Flujo
{
    public class CotizacionFlujo : ICotizacionFlujo
    {
        private ICotizacionDA _cotizacionDA;
        private IEnviarCotizacion _enviarCotizacion;
        private IUsuarioDA _usuarioDA;

        public CotizacionFlujo(
            ICotizacionDA cotizacionDA,
            IEnviarCotizacion enviarCotizacion,
            IUsuarioDA usuarioDA)
        {
            _cotizacionDA = cotizacionDA;
            _enviarCotizacion = enviarCotizacion;
            _usuarioDA = usuarioDA;
        }

        public async Task<int> RegistrarCotizacionFlujo(int idEmpresa, int idUsuario, CotizacionRequest cotizacion)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));
            var datos = NormalizarYValidar(cotizacion);
            return await _cotizacionDA.RegistrarCotizacion(idEmpresa, idUsuario, datos);
        }

        // avanzar cotizacion enviada
        public async Task<int> AvanzarCotizacionEnviadaFlujo(int idEmpresa, int idUsuario, int idCotizacion)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));
            ValidarId(idCotizacion, nameof(idCotizacion));
            return await _cotizacionDA.AvanzarCotizacionEnviada(idEmpresa, idUsuario, idCotizacion);
        }

        public async Task<CotizacionResponseDetallada?> ObtenerCotizacionPorId(int idCotizacion)
        {
            ValidarId(idCotizacion, nameof(idCotizacion));
            return await _cotizacionDA.ObtenerCotizacionPorId(idCotizacion);
        }

        public async Task<IActionResult> EnviarCotizacionPorCorreo(int idCotizacion, string correo)
        {
            ValidarId(idCotizacion, nameof(idCotizacion));
            if (string.IsNullOrWhiteSpace(correo))
                return new BadRequestObjectResult("El correo es obligatorio.");
            correo = correo.Trim();
            if (correo.Length > 150 || !EsCorreoValido(correo))
                return new BadRequestObjectResult("El correo no tiene un formato válido.");

            var resultado = await _enviarCotizacion.EnviarCotizacionPorGmail(idCotizacion, correo);
            return resultado;
        }

        public async Task<int> RetrocederCotizacionEnviada(int idCotizacion)
        {
            ValidarId(idCotizacion, nameof(idCotizacion));
            return await _cotizacionDA.RetrocederCotizacionEnviada(idCotizacion);
        }

        public async Task<ActionResult> AvanzarCotizacionAOrdenDeVenta(int idCotizacion)
        {
            ValidarId(idCotizacion, nameof(idCotizacion));

            var faltantes = await _cotizacionDA.ValidarInventarioCotizacion(idCotizacion);

            if (faltantes.Any())
            {
                return new BadRequestObjectResult(new
                {
                    Mensaje = "No hay suficiente inventario para avanzar la cotización.",
                    Faltantes = faltantes
                });
            }

            var resultado = await _cotizacionDA.AvanzarCotizacionAOrdenDeVenta(idCotizacion);

            if (resultado == 0)
                return new BadRequestObjectResult("No avanzo la cotizacion");

            return new OkObjectResult("Se actualizo a Orden de Venta");
        }

        public async Task<ActionResult> Facturar(int idCotizacion, int idUsuario, FacturarRequest request)
        {
            ValidarId(idCotizacion, nameof(idCotizacion));
            ValidarId(idUsuario, nameof(idUsuario));
            ArgumentNullException.ThrowIfNull(request);

            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var cotizacion = await _cotizacionDA.ObtenerCotizacionPorId(idCotizacion);

            if (cotizacion == null)
                return new NotFoundObjectResult("Cotización no encontrada.");
            if (cotizacion.IdEmpresa != idEmpresaUsuario.Value)
                return new ForbidResult();

            if (cotizacion.NombreEstado != "Orden de Venta")
                return new BadRequestObjectResult("La cotización debe estar en estado 'Orden de Venta'");

            if (!request.FechaFactura.HasValue || request.FechaFactura.Value == DateTime.MinValue)
                return new BadRequestObjectResult("La fecha de factura es obligatoria");

            ValidarTerminoPago(request);

            var factura = new FacturarCotizacion
            {
                IdCotizacion = cotizacion.IdCotizacion,
                IdEmpresa = cotizacion.IdEmpresa,
                IdCliente = cotizacion.IdCliente,
                IdUsuario = idUsuario,
                NumeroVenta = $"VEN-{cotizacion.NumeroCotizacion}",
                TipoFactura = request.TipoFactura,
                CondicionVenta = request.CondicionVenta,
                FechaFactura = request.FechaFactura.Value,
                IdTerminoPago = request.IdTerminoPago ?? 0,
                Moneda = cotizacion.Moneda,
                TipoCambio = cotizacion.TipoCambio,
                Subtotal = cotizacion.Subtotal,
                Impuesto = cotizacion.Impuesto,
                Descuento = cotizacion.Descuento,
                Total = cotizacion.Total
            };

            var resultado = await _cotizacionDA.FacturarOrdenDeVenta(factura);

            if (resultado == null || resultado.IdFactura == 0)
                return new BadRequestObjectResult(resultado?.Mensaje ?? "No se pudo generar la factura");

            return new OkObjectResult(resultado);
        }

        public async Task<ActionResult> ObtenerCanceladasRechazadas(int idEmpresa, string? busqueda, DateTime? fecha)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            var filtro = new
            {
                Busqueda = Limpiar(busqueda),
                Fecha = fecha
            };

            if (!string.IsNullOrEmpty(filtro.Busqueda) && filtro.Busqueda.Length > 100)
                return new BadRequestObjectResult("El texto de búsqueda no puede exceder 100 caracteres.");

            var cotizaciones = await _cotizacionDA.ObtenerCanceladasRechazadas(idEmpresa, filtro.Busqueda, filtro.Fecha);
            return new OkObjectResult(cotizaciones);
        }

        public async Task<CotizacionCatalogoRespuesta> ObtenerCotizacionesFlujo(CotizacionFiltro filtro)
        {
            var pagina = 1;
            var tamanoPagina = 10;

            if (filtro != null)
            {
                if (filtro.Pagina.HasValue && filtro.Pagina.Value >= 1) pagina = filtro.Pagina.Value;
                if (filtro.TamanoPagina.HasValue && filtro.TamanoPagina.Value >= 1) tamanoPagina = filtro.TamanoPagina.Value;
            }

            if (tamanoPagina > 100) tamanoPagina = 100;

            if (filtro == null || !filtro.IdEmpresa.HasValue || filtro.IdEmpresa <= 0)
            {
                return new CotizacionCatalogoRespuesta
                {
                    Pagina = pagina,
                    TamanoPagina = tamanoPagina,
                    Total = 0,
                    Items = Enumerable.Empty<CotizacionResponse>()
                };
            }

            filtro.Pagina = pagina;
            filtro.TamanoPagina = tamanoPagina;
            filtro.Busqueda = Limpiar(filtro.Busqueda);
            if (filtro.Busqueda != null && filtro.Busqueda.Length > 100)
                filtro.Busqueda = filtro.Busqueda.Substring(0, 100);
            if (filtro.IdEstado.HasValue && filtro.IdEstado.Value <= 0)
                filtro.IdEstado = null;

            return await _cotizacionDA.ObtenerCotizaciones(filtro);
        }

        //editar cotizacion
        public async Task<int> EditarCotizacionFlujo(int idEmpresa, int idUsuario, CotizacionRequest cotizacion)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));
            var datos = NormalizarYValidar(cotizacion);
            ValidarId(datos.IdCotizacion, nameof(datos.IdCotizacion));
            return await _cotizacionDA.EditarCotizacion(idEmpresa, idUsuario, datos);
        }

        //cancelar cotizacion
        public async Task<int> CancelarCotizacionFlujo(int idEmpresa, int idUsuario, int idCotizacion)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));
            ValidarId(idCotizacion, nameof(idCotizacion));
            return await _cotizacionDA.CancelarCotizacion(idEmpresa, idUsuario, idCotizacion);
        }

        public async Task<int> RetrocederAEnviada(int idCotizacion)
        {
            ValidarId(idCotizacion, nameof(idCotizacion));
            return await _cotizacionDA.RetrocederCotizacionAEnviada(idCotizacion);
        }

        #region "Helpers"

        private static string? Limpiar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        private static void ValidarId(int valor, string nombre)
        {
            if (valor <= 0)
                throw new ValidationException($"{nombre} debe ser mayor que cero.");
        }

        private static void ValidarNoNegativo(decimal valor, string nombre)
        {
            if (valor < 0)
                throw new ValidationException($"{nombre} no puede ser negativo.");
        }

        private static void ValidarPositivo(decimal valor, string nombre)
        {
            if (valor <= 0)
                throw new ValidationException($"{nombre} debe ser mayor que cero.");
        }

        private static void ValidarModelo(object modelo)
        {
            var errores = new List<ValidationResult>();
            if (!Validator.TryValidateObject(modelo, new ValidationContext(modelo), errores, true))
                throw new ValidationException(string.Join(" ", errores.Select(e => e.ErrorMessage)));
        }

        private static bool EsCorreoValido(string correo)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(correo);
                return string.Equals(addr.Address, correo, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static CotizacionRequest NormalizarYValidar(CotizacionRequest cotizacion)
        {
            ArgumentNullException.ThrowIfNull(cotizacion);

            if (cotizacion.Detalles == null || cotizacion.Detalles.Count == 0)
                throw new ValidationException("La cotización debe tener al menos un detalle.");

            if (cotizacion.Detalles.Count > 500)
                throw new ValidationException("La cotización no puede tener más de 500 líneas de detalle.");

            ValidarId(cotizacion.IdCliente, "IdCliente");
            ValidarId(cotizacion.IdEstado, "IdEstado");

            if (cotizacion.FechaEmision == DateTime.MinValue)
                throw new ValidationException("La fecha de emisión es obligatoria.");

            if (cotizacion.FechaVencimiento.HasValue && cotizacion.FechaVencimiento.Value < cotizacion.FechaEmision)
                throw new ValidationException("La fecha de vencimiento no puede ser anterior a la fecha de emisión.");

            ValidarNoNegativo(cotizacion.Subtotal, "Subtotal");
            ValidarNoNegativo(cotizacion.Impuesto, "Impuesto");
            ValidarNoNegativo(cotizacion.Descuento, "Descuento");
            ValidarNoNegativo(cotizacion.Total, "Total");

            if (cotizacion.Descuento > cotizacion.Subtotal)
                throw new ValidationException("El descuento no puede ser mayor que el subtotal.");

            if (cotizacion.TipoCambio.HasValue && cotizacion.TipoCambio.Value <= 0)
                throw new ValidationException("El tipo de cambio debe ser mayor que cero.");

            var detalles = new List<DetalleCotizacionRequest>(cotizacion.Detalles.Count);

            foreach (var d in cotizacion.Detalles)
            {
                if (d == null)
                    throw new ValidationException("Hay una línea de detalle vacía.");

                ValidarId(d.IdProducto, "IdProducto del detalle");
                ValidarPositivo(d.Cantidad, "Cantidad del detalle");
                ValidarNoNegativo(d.PrecioUnitario, "PrecioUnitario del detalle");

                if (d.PorcentajeDescuento.HasValue)
                {
                    if (d.PorcentajeDescuento.Value < 0 || d.PorcentajeDescuento.Value > 100)
                        throw new ValidationException("El porcentaje de descuento del detalle debe estar entre 0 y 100.");
                }

                if (d.PorcentajeImpuesto.HasValue)
                {
                    if (d.PorcentajeImpuesto.Value < 0 || d.PorcentajeImpuesto.Value > 100)
                        throw new ValidationException("El porcentaje de impuesto del detalle debe estar entre 0 y 100.");
                }

                ValidarNoNegativo(d.Subtotal, "Subtotal del detalle");

                detalles.Add(new DetalleCotizacionRequest
                {
                    IdDetalleCotizacion = d.IdDetalleCotizacion,
                    IdCotizacion = d.IdCotizacion,
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    PorcentajeDescuento = d.PorcentajeDescuento,
                    PorcentajeImpuesto = d.PorcentajeImpuesto,
                    Subtotal = d.Subtotal
                });
            }

            var datos = new CotizacionRequest
            {
                IdCotizacion = cotizacion.IdCotizacion,
                IdEmpresa = cotizacion.IdEmpresa,
                IdCliente = cotizacion.IdCliente,
                NumeroCotizacion = Limpiar(cotizacion.NumeroCotizacion) ?? string.Empty,
                FechaEmision = cotizacion.FechaEmision,
                FechaVencimiento = cotizacion.FechaVencimiento,
                Moneda = Limpiar(cotizacion.Moneda) ?? string.Empty,
                TipoCambio = cotizacion.TipoCambio,
                Subtotal = cotizacion.Subtotal,
                Impuesto = cotizacion.Impuesto,
                Descuento = cotizacion.Descuento,
                Total = cotizacion.Total,
                IdEstado = cotizacion.IdEstado,
                Detalles = detalles
            };

            ValidarModelo(datos);

            return datos;
        }

        private static void ValidarTerminoPago(FacturarRequest request)
        {
            bool esContado = request.CondicionVenta == "01";
            bool esCredito = request.CondicionVenta == "02";

            if (!esContado && !esCredito)
                throw new ValidationException("La condición de venta debe ser '01' (contado) o '02' (crédito).");

            if (esContado && request.IdTerminoPago.HasValue && request.IdTerminoPago > 0)
                throw new ValidationException("Para ventas de contado no se debe especificar un término de pago.");

            if (esCredito && (!request.IdTerminoPago.HasValue || request.IdTerminoPago <= 0))
                throw new ValidationException("Para ventas a crédito es obligatorio seleccionar un término de pago válido.");
        }

        #endregion
    }
}