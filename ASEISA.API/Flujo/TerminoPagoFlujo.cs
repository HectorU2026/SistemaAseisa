using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;

namespace Flujo
{
    public class TerminoPagoFlujo : ITerminoPagoFlujo
    {
        private readonly ITerminoPagoDA _terminoPagoDA;

        public TerminoPagoFlujo(ITerminoPagoDA terminoPagoDA)
        {
            _terminoPagoDA = terminoPagoDA;
        }

        public async Task<IEnumerable<TerminoPagoResponse>> Obtener(
            int idEmpresa, int idUsuario)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));

            return await _terminoPagoDA.Obtener(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario);
        }

        public async Task<TerminoPagoCreadoResponse> Agregar(
            int idEmpresa,
            int idUsuario,
            TerminoPagoRequest termino)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));
            ValidarTermino(termino);

            return await _terminoPagoDA.Agregar(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario,
                termino: termino);
        }

        public async Task Editar(
            int idEmpresa,
            int idUsuario,
            int idTerminoPago,
            TerminoPagoRequest termino)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));
            ValidarId(idTerminoPago, nameof(idTerminoPago));
            ValidarTermino(termino);

            await _terminoPagoDA.Editar(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario,
                idTerminoPago: idTerminoPago,
                termino: termino);
        }

        public async Task CambiarEstado(
            int idEmpresa,
            int idUsuario,
            int idTerminoPago,
            bool activo)
        {
            ValidarId(idEmpresa, nameof(idEmpresa));
            ValidarId(idUsuario, nameof(idUsuario));
            ValidarId(idTerminoPago, nameof(idTerminoPago));

            await _terminoPagoDA.CambiarEstado(
                idEmpresa: idEmpresa,
                idUsuario: idUsuario,
                idTerminoPago: idTerminoPago,
                activo: activo);
        }

        private static void ValidarTermino(TerminoPagoRequest termino)
        {
            ArgumentNullException.ThrowIfNull(termino);

            termino.Nombre = termino.Nombre?.Trim()
                ?? string.Empty;

            if (termino.Nombre.Length < 2 ||
                termino.Nombre.Length > 100)
            {
                throw new ArgumentException(
                    "El nombre del término de pago debe tener " +
                    "entre 2 y 100 caracteres.");
            }

            if (!termino.DiasCredito.HasValue ||
                termino.DiasCredito.Value < 0)
            {
                throw new ArgumentException(
                    "Los días de crédito son obligatorios " +
                    "y no pueden ser negativos.");
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
