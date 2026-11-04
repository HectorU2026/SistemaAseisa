using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Web.Servicios
{
    public class MetodoPagoApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguracion _configuracion;

        public MetodoPagoApiService(
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
                "ObtenerMetodosPago");
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
                    "El método de pago solicitado no existe.");
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

        // GET: api/MetodoPago
        public async Task<List<MetodoPagoResponse>> ObtenerMetodosPagoAsync(
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
                return new List<MetodoPagoResponse>();
            }

            ValidarRespuesta(respuesta);

            return await respuesta.Content
                .ReadFromJsonAsync<List<MetodoPagoResponse>>()
                ?? new List<MetodoPagoResponse>();
        }

        // POST: api/MetodoPago
        public async Task<MetodoPagoCreadoResponse> AgregarMetodoPagoAsync(
            MetodoPagoRequest metodo,
            string token)
        {
            using var solicitud = new HttpRequestMessage(
                HttpMethod.Post,
                ObtenerEndpoint());

            AgregarToken(solicitud, token);

            solicitud.Content = JsonContent.Create(metodo);

            using var respuesta =
                await _httpClient.SendAsync(solicitud);

            ValidarRespuesta(respuesta);

            return await respuesta.Content
                .ReadFromJsonAsync<MetodoPagoCreadoResponse>()
                ?? throw new InvalidOperationException(
                    "La API no devolvió la información del método de pago registrado.");
        }

        // PUT: api/MetodoPago/{id}
        public async Task EditarMetodoPagoAsync(
            int idMetodoPago,
            MetodoPagoRequest metodo,
            string token)
        {
            if (idMetodoPago <= 0)
            {
                throw new ArgumentException(
                    "El identificador del método de pago no es válido.",
                    nameof(idMetodoPago));
            }

            string endpoint =
                $"{ObtenerEndpoint().TrimEnd('/')}/{idMetodoPago}";

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Put,
                endpoint);

            AgregarToken(solicitud, token);

            solicitud.Content = JsonContent.Create(metodo);

            using var respuesta =
                await _httpClient.SendAsync(solicitud);

            ValidarRespuesta(respuesta);
        }

        // PUT: api/MetodoPago/{id}/activar
        public async Task ActivarMetodoPagoAsync(
            int idMetodoPago,
            string token)
        {
            await CambiarEstadoMetodoPagoAsync(
                idMetodoPago,
                token,
                "activar");
        }

        // PUT: api/MetodoPago/{id}/desactivar
        public async Task DesactivarMetodoPagoAsync(
            int idMetodoPago,
            string token)
        {
            await CambiarEstadoMetodoPagoAsync(
                idMetodoPago,
                token,
                "desactivar");
        }

        private async Task CambiarEstadoMetodoPagoAsync(
            int idMetodoPago,
            string token,
            string accion)
        {
            if (idMetodoPago <= 0)
            {
                throw new ArgumentException(
                    "El identificador del método de pago no es válido.",
                    nameof(idMetodoPago));
            }

            if (accion != "activar" && accion != "desactivar")
            {
                throw new ArgumentException(
                    "La acción solicitada no es válida.",
                    nameof(accion));
            }

            string endpoint =
                $"{ObtenerEndpoint().TrimEnd('/')}/{idMetodoPago}/{accion}";

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
