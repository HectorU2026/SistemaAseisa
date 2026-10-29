
using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Interfaces.Servicio;
using Abstracciones.Modelos;
using DA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Servicio;
using Servicios;
using Servicios.Correo;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
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
            return await _cotizacionDA.RegistrarCotizacion(idEmpresa, idUsuario, cotizacion);
        }

        // avanzar cotizacion enviada
        public async Task<int> AvanzarCotizacionEnviadaFlujo(int idEmpresa, int idUsuario, int idCotizacion)
        {
            return await _cotizacionDA.AvanzarCotizacionEnviada(idEmpresa, idUsuario, idCotizacion);
        }

        public async Task<CotizacionResponseDetallada?> ObtenerCotizacionPorId(int idCotizacion)
        {
            return await _cotizacionDA.ObtenerCotizacionPorId(idCotizacion);
        }

        public async Task<IActionResult> EnviarCotizacionPorCorreo(int idCotizacion, string correo)
        {
            var resultado = await _enviarCotizacion.EnviarCotizacionPorGmail(idCotizacion, correo);
            return resultado;
        }

        public async Task<int> RetrocederCotizacionEnviada(int idCotizacion)
        {
            return await _cotizacionDA.RetrocederCotizacionEnviada(idCotizacion);
        }

        public async Task<ActionResult> AvanzarCotizacionAOrdenDeVenta(int idCotizacion)
        {
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
                return new BadRequestObjectResult("No avnazo la cotizacion");

            return new OkObjectResult("Se actualizo a Orden de Venta");
        }

        public async Task<ActionResult> Facturar(int idCotizacion, int idUsuario, FacturarRequest request)
        {
            // FAC-001: Verificar que el usuario esté activo
            // y obtener la empresa a la que pertenece.
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var cotizacion = await _cotizacionDA.ObtenerCotizacionPorId(idCotizacion);

            if (cotizacion == null)
                return new NotFoundObjectResult("Cotización no encontrada.");

            // FAC-001: Impedir facturar cotizaciones de otra empresa.
            if (cotizacion.IdEmpresa != idEmpresaUsuario.Value)
                return new ForbidResult();

            if (cotizacion.NombreEstado != "Orden de Venta")
                return new BadRequestObjectResult("La cotización debe estar en estado 'Orden de Venta'");

            if (!request.FechaFactura.HasValue || request.FechaFactura.Value == DateTime.MinValue)
                return new BadRequestObjectResult("La fecha de factura es obligatoria");

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
                IdTerminoPago = request.IdTerminoPago,
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

        public async Task<ActionResult> ObtenerCanceladasRechazadas(int idEmpresa)
        {
            var cotizaciones = await _cotizacionDA.ObtenerCanceladasRechazadas(idEmpresa);
            return new OkObjectResult(cotizaciones);
        }

        public async Task<IEnumerable<CotizacionResponse>> ObtenerCotizacionesFlujo(int idEmpresa)
        {
            return await _cotizacionDA.ObtenerCotizaciones(idEmpresa);
        }

        //editar cotizacion
        public async Task<int> EditarCotizacionFlujo(int idEmpresa, int idUsuario, CotizacionRequest cotizacion)
        {
            return await _cotizacionDA.EditarCotizacion(idEmpresa, idUsuario, cotizacion);
        }

        //cancelar cotizacion
        public async Task<int> CancelarCotizacionFlujo(int idEmpresa, int idUsuario, int idCotizacion)
        {
            return await _cotizacionDA.CancelarCotizacion(idEmpresa, idUsuario, idCotizacion);
        }

        public async Task<int> RetrocederAEnviada(int idCotizacion)
        {
            return await _cotizacionDA.RetrocederCotizacionAEnviada(idCotizacion);
        }
    }
}
