using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Interfaces.Servicio;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace Flujo
{
    public class FacturaFlujo : IFacturaFlujo
    {
        private readonly IFacturaDA _facturaDA;
        private readonly IUsuarioDA _usuarioDA;
        private readonly IEnviarFactura _enviarFactura;

        public FacturaFlujo(IFacturaDA facturaDA, IUsuarioDA usuarioDA, IEnviarFactura enviarFactura)
        {
            _facturaDA = facturaDA;
            _usuarioDA = usuarioDA;
            _enviarFactura = enviarFactura;
        }

        public async Task<ActionResult> ObtenerFacturas(int idUsuario, FacturaFiltro? filtro)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var facturas = await _facturaDA.ObtenerFacturas(idEmpresaUsuario.Value, filtro);
            return new OkObjectResult(facturas);
        }

        public async Task<ActionResult> ObtenerFactura(int idUsuario, int idFactura)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var factura = await _facturaDA.ObtenerFactura(idEmpresaUsuario.Value, idFactura);

            if (factura == null)
                return new NotFoundObjectResult("La factura no existe");

            return new OkObjectResult(factura);
        }

        public async Task<ActionResult> Confirmar(int idUsuario, int idFactura, byte[]? versionFila)
        {
            return await EjecutarCambioEstado(idUsuario, idFactura, versionFila, _facturaDA.Confirmar);
        }

        public async Task<ActionResult> Restablecer(int idUsuario, int idFactura, byte[]? versionFila)
        {
            return await EjecutarCambioEstado(idUsuario, idFactura, versionFila, _facturaDA.Restablecer);
        }

        public async Task<ActionResult> Inactivar(
            int idUsuario,
            int idFactura,
            byte[]? versionFila,
            string? motivoAnulacion)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            if (versionFila == null || versionFila.Length != 8)
                return new BadRequestObjectResult("La versión de la factura es obligatoria");

            var motivo = motivoAnulacion?.Trim();

            if (string.IsNullOrWhiteSpace(motivo) || motivo.Length > 500)
                return new BadRequestObjectResult("El motivo de anulación es obligatorio");

            var resultado = await _facturaDA.Inactivar(
                idEmpresaUsuario.Value,
                idUsuario,
                idFactura,
                versionFila,
                motivo);

            return MapearCambioEstado(resultado);
        }

        public async Task<ActionResult> Reactivar(int idUsuario, int idFactura, byte[]? versionFila)
        {
            return await EjecutarCambioEstado(idUsuario, idFactura, versionFila, _facturaDA.Reactivar);
        }

        public async Task<ActionResult> RegistrarPago(
            int idUsuario,
            int idFactura,
            decimal? monto,
            string? metodoPago)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var metodo = metodoPago?.Trim();

            if (!monto.HasValue
                || monto.Value <= 0
                || string.IsNullOrWhiteSpace(metodo)
                || metodo.Length > 50)
            {
                return new BadRequestObjectResult("Por favor, rellene los campos obligatorios");
            }

            var resultado = await _facturaDA.RegistrarPago(
                idEmpresaUsuario.Value,
                idFactura,
                monto.Value,
                metodo);

            return MapearPagoFactura(resultado);
        }

        public async Task<ActionResult> RegistrarNotaCredito(
            int idUsuario,
            int idFactura,
            string? motivo,
            decimal? monto,
            string? descripcionAdicional)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            var motivoNormalizado = motivo?.Trim();
            var descripcion = string.IsNullOrWhiteSpace(descripcionAdicional)
                ? null
                : descripcionAdicional.Trim();

            if (string.IsNullOrWhiteSpace(motivoNormalizado)
                || motivoNormalizado.Length > 500
                || !monto.HasValue
                || monto.Value <= 0
                || (descripcion != null && descripcion.Length > 500))
            {
                return new BadRequestObjectResult("Por favor, rellene los campos obligatorios");
            }

            var resultado = await _facturaDA.RegistrarNotaCredito(
                idEmpresaUsuario.Value,
                idUsuario,
                idFactura,
                motivoNormalizado,
                monto.Value,
                descripcion);

            return MapearNotaCredito(resultado);
        }

        public async Task<ActionResult> Enviar(int idUsuario, int idFactura)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            return await _enviarFactura.Enviar(idUsuario, idEmpresaUsuario.Value, idFactura);
        }

        private async Task<ActionResult> EjecutarCambioEstado(
            int idUsuario,
            int idFactura,
            byte[]? versionFila,
            Func<int, int, int, byte[], Task<FacturaCambioEstadoResultado>> operacion)
        {
            var idEmpresaUsuario = await _usuarioDA.ObtenerEmpresaUsuarioActivo(idUsuario);

            if (!idEmpresaUsuario.HasValue)
                return new ForbidResult();

            if (versionFila == null || versionFila.Length != 8)
                return new BadRequestObjectResult("La versión de la factura es obligatoria");

            var resultado = await operacion(idEmpresaUsuario.Value, idUsuario, idFactura, versionFila);

            return MapearCambioEstado(resultado);
        }

        private static ActionResult MapearCambioEstado(FacturaCambioEstadoResultado resultado)
        {
            return resultado.Codigo switch
            {
                1 => new OkObjectResult(new
                {
                    resultado.IdFactura,
                    resultado.NumeroFactura,
                    resultado.Estado,
                    resultado.VersionFila,
                    resultado.Mensaje
                }),
                0 => new NotFoundObjectResult(resultado.Mensaje),
                2 => new ConflictObjectResult(resultado.Mensaje),
                _ => new BadRequestObjectResult(resultado.Mensaje)
            };
        }

        private static ActionResult MapearPagoFactura(FacturaPagoResultado resultado)
        {
            return resultado.Codigo switch
            {
                1 => new OkObjectResult(new
                {
                    resultado.Mensaje,
                    resultado.IdFactura,
                    resultado.NumeroFactura,
                    resultado.Estado,
                    resultado.SaldoPendiente,
                    resultado.IdPagoCliente,
                    resultado.VersionFila
                }),
                0 => new NotFoundObjectResult(resultado.Mensaje),
                4 or 5 or 6 or 7 or 8 or 9 or 10 or 11 => new BadRequestObjectResult(resultado.Mensaje),
                _ => new BadRequestObjectResult(resultado.Mensaje)
            };
        }

        private static ActionResult MapearNotaCredito(FacturaNotaCreditoResultado resultado)
        {
            return resultado.Codigo switch
            {
                1 => new OkObjectResult(new
                {
                    resultado.Mensaje,
                    resultado.IdNotaCredito,
                    resultado.NumeroNotaCredito,
                    resultado.IdFactura,
                    resultado.NumeroFactura,
                    resultado.Monto
                }),
                0 => new NotFoundObjectResult(resultado.Mensaje),
                5 or 7 or 9 or 10 or 11 or 12 or 13 => new BadRequestObjectResult(resultado.Mensaje),
                _ => new BadRequestObjectResult(resultado.Mensaje)
            };
        }
    }
}
