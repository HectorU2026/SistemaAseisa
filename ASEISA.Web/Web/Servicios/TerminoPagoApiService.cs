using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Web.Servicios
{
    public class TerminoPagoApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguracion _configuracion;

        public TerminoPagoApiService(
            HttpClient httpClient,
            IConfiguracion configuracion)
        {
            _httpClient = httpClient;
            _configuracion = configuracion;
        }

        private string ObtenerEndpoint()
        {
            return _configuracion.ObtenerMetodo(
                "ApiEndPoints",
                "ObtenerTerminosPago");
        }

        private static void AgregarToken(
            HttpRequestMessage solicitud,
            string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException(
                    "No se encontró el token de autenticación.");
            }

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private static void ValidarRespuesta(
            HttpResponseMessage respuesta)
        {
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "La sesión no está autorizada.");
            }

            if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "No tiene permisos para realizar esta operación.");
            }

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException(
                    "El término de pago solicitado no existe.");
            }

            if (respuesta.StatusCode == HttpStatusCode.Conflict)
            {
                throw new HttpRequestException(
                    "La operación no pudo completarse debido a un conflicto.",
                    null,
                    HttpStatusCode.Conflict);
            }

            respuesta.EnsureSuccessStatusCode();
        }

        public async Task<List<TerminoPagoResponse>> ObtenerTerminosPagoAsync(
            string token)
        {
            using var solicitud = new HttpRequestMessage(
                HttpMethod.Get,
                ObtenerEndpoint());

            AgregarToken(solicitud, token);

            using var respuesta =
                await _httpClient.SendAsync(solicitud);

            if (respuesta.StatusCode == HttpStatusCode.NoContent)
            {
                return new List<TerminoPagoResponse>();
            }

            ValidarRespuesta(respuesta);

            return await respuesta.Content
                .ReadFromJsonAsync<List<TerminoPagoResponse>>()
                ?? new List<TerminoPagoResponse>();
        }

        public async Task<TerminoPagoCreadoResponse> AgregarTerminoPagoAsync(
            TerminoPagoRequest termino,
            string token)
        {
            using var solicitud = new HttpRequestMessage(
                HttpMethod.Post,
                ObtenerEndpoint());

            AgregarToken(solicitud, token);

            solicitud.Content = JsonContent.Create(termino);

            using var respuesta =
                await _httpClient.SendAsync(solicitud);

            ValidarRespuesta(respuesta);

            return await respuesta.Content
                .ReadFromJsonAsync<TerminoPagoCreadoResponse>()
                ?? throw new InvalidOperationException(
                    "La API no devolvió el término de pago registrado.");
        }

        public async Task EditarTerminoPagoAsync(
            int idTerminoPago,
            TerminoPagoRequest termino,
            string token)
        {
            if (idTerminoPago <= 0)
            {
                throw new ArgumentException(
                    "El identificador del término de pago no es válido.",
                    nameof(idTerminoPago));
            }

            string endpoint =
                $"{ObtenerEndpoint().TrimEnd('/')}/{idTerminoPago}";

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Put,
                endpoint);

            AgregarToken(solicitud, token);

            solicitud.Content = JsonContent.Create(termino);

            using var respuesta =
                await _httpClient.SendAsync(solicitud);

            ValidarRespuesta(respuesta);
        }

        public async Task ActivarTerminoPagoAsync(
            int idTerminoPago,
            string token)
        {
            await CambiarEstadoTerminoPagoAsync(
                idTerminoPago,
                token,
                "activar");
        }

        public async Task DesactivarTerminoPagoAsync(
            int idTerminoPago,
            string token)
        {
            await CambiarEstadoTerminoPagoAsync(
                idTerminoPago,
                token,
                "desactivar");
        }

        private async Task CambiarEstadoTerminoPagoAsync(
            int idTerminoPago,
            string token,
            string accion)
        {
            if (idTerminoPago <= 0)
            {
                throw new ArgumentException(
                    "El identificador del término de pago no es válido.",
                    nameof(idTerminoPago));
            }

            if (accion != "activar" && accion != "desactivar")
            {
                throw new ArgumentException(
                    "La acción solicitada no es válida.",
                    nameof(accion));
            }

            string endpoint =
                $"{ObtenerEndpoint().TrimEnd('/')}/{idTerminoPago}/{accion}";

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Put,
                endpoint);

            AgregarToken(solicitud, token);

            using var respuesta =
                await _httpClient.SendAsync(solicitud);

            ValidarRespuesta(respuesta);
        }
    }
}
