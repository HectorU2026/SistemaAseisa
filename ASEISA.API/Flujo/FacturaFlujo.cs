using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace Flujo
{
    public class FacturaFlujo : IFacturaFlujo
    {
        private readonly IFacturaDA _facturaDA;
        private readonly IUsuarioDA _usuarioDA;

        public FacturaFlujo(IFacturaDA facturaDA, IUsuarioDA usuarioDA)
        {
            _facturaDA = facturaDA;
            _usuarioDA = usuarioDA;
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
    }
}
