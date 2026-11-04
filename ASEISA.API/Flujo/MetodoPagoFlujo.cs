using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;

namespace Flujo
{
    public class MetodoPagoFlujo : IMetodoPagoFlujo
    {
        private readonly IMetodoPagoDA _metodoPagoDA;

        public MetodoPagoFlujo(IMetodoPagoDA metodoPagoDA)
        {
            _metodoPagoDA = metodoPagoDA;
        }

        public async Task<IEnumerable<MetodoPagoResponse>> Obtener(
            int idEmpresa, int idUsuario)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));

            return await _metodoPagoDA.Obtener(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario);
        }

        public async Task<MetodoPagoCreadoResponse> Agregar(
            int idEmpresa,
            int idUsuario,
            MetodoPagoRequest metodo)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));
            ValidarMetodo(metodo);

            return await _metodoPagoDA.Agregar(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario,
                metodo: metodo);
        }

        public async Task Editar(
            int idEmpresa,
            int idUsuario,
            int idMetodoPago,
            MetodoPagoRequest metodo)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));
            ValidarId(idMetodoPago, nameof(idMetodoPago));
            ValidarMetodo(metodo);

            await _metodoPagoDA.Editar(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario,
                idMetodoPago: idMetodoPago,
                metodo: metodo);
        }

        public async Task CambiarEstado(
            int idEmpresa,
            int idUsuario,
            int idMetodoPago,
            bool activo)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));
            ValidarId(idMetodoPago, nameof(idMetodoPago));

            await _metodoPagoDA.CambiarEstado(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario,
                idMetodoPago: idMetodoPago,
                activo: activo);
        }

        private static void ValidarMetodo(MetodoPagoRequest metodo)
        {
            ArgumentNullException.ThrowIfNull(metodo);

            metodo.Nombre = metodo.Nombre?.Trim()
                ?? string.Empty;

            if (metodo.Nombre.Length < 2 ||
                metodo.Nombre.Length > 100)
            {
                throw new ArgumentException(
                    "El nombre del método de pago debe tener " +
                    "entre 2 y 100 caracteres.");
            }
        }

        private static void ValidarId(int id, string nombre)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "El identificador debe ser mayor que cero.",
                    nombre);
            }
        }
    }
}
