
using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Web.Servicios
{
    public class ContactoApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguracion _configuracion;

        public ContactoApiService(
            HttpClient httpClient,
            IConfiguracion configuracion)
        {
            _httpClient = httpClient;
            _configuracion = configuracion;
        }

        public async Task<List<ContactoResponse>> ObtenerContactosAsync(
            string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException(
                    "No se encontró el token de autenticación.");
            }

            string endpoint = _configuracion.ObtenerMetodo(
                "ApiEndPoints",
                "ObtenerContactos");

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Get,
                endpoint);

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            using var respuesta =
                await _httpClient.SendAsync(solicitud);

            if (respuesta.StatusCode == HttpStatusCode.NoContent)
            {
                return new List<ContactoResponse>();
            }

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "La sesión no está autorizada para consultar contactos.");
            }

            if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "No tiene permisos para consultar contactos.");
            }

            respuesta.EnsureSuccessStatusCode();

            var contactos = await respuesta.Content
                .ReadFromJsonAsync<List<ContactoResponse>>();

            return contactos ?? new List<ContactoResponse>();
        }

        public async Task<ContactoCreadoResponse> AgregarContactoAsync(
            ContactoRequest contacto,
            string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException(
                    "No se encontró el token de autenticación.");
            }

            string endpoint = _configuracion.ObtenerMetodo(
                "ApiEndPoints",
                "ObtenerContactos");

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Post,
                endpoint);

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            solicitud.Content = JsonContent.Create(contacto);

            using var respuesta = await _httpClient.SendAsync(solicitud);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "La sesión no está autorizada para registrar contactos.");
            }

            if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "No tiene permisos para registrar contactos.");
            }

            respuesta.EnsureSuccessStatusCode();

            var resultado = await respuesta.Content
                .ReadFromJsonAsync<ContactoCreadoResponse>();

            return resultado ?? throw new InvalidOperationException(
                "La API no devolvió la información del contacto registrado.");
        }

        public async Task<ContactoCatalogosPagoResponse> ObtenerCatalogosPagoAsync(
            string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException(
                    "No se encontró el token de autenticación.");
            }

            string endpoint = _configuracion.ObtenerMetodo(
                "ApiEndPoints",
                "ObtenerCatalogosPagoContacto");

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Get,
                endpoint);

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var respuesta = await _httpClient.SendAsync(solicitud);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "La sesión no está autorizada para consultar los catálogos.");
            }

            if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "No tiene permisos para consultar los catálogos de pago.");
            }

            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content
                .ReadFromJsonAsync<ContactoCatalogosPagoResponse>()
                ?? new ContactoCatalogosPagoResponse();
        }

        public async Task<ContactoResponse?> ObtenerContactoPorIdAsync(
            int idContacto,
            string token)
        {
            if (idContacto <= 0)
                throw new ArgumentException(
                    "El identificador del contacto no es válido.",
                    nameof(idContacto));

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException(
                    "No se encontró el token de autenticación.");

            string endpointBase = _configuracion.ObtenerMetodo(
                "ApiEndPoints",
                "ObtenerContactos");

            string endpoint =
                $"{endpointBase.TrimEnd('/')}/{idContacto}";

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Get,
                endpoint);

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var respuesta = await _httpClient.SendAsync(solicitud);

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException(
                    "La sesión no está autorizada para consultar el contacto.");

            if (respuesta.StatusCode == HttpStatusCode.Forbidden)
                throw new InvalidOperationException(
                    "No tiene permisos para consultar este contacto.");

            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content
                .ReadFromJsonAsync<ContactoResponse>();
        }

        public async Task EditarContactoAsync(
            int idContacto,
            ContactoRequest contacto,
            string token)
        {
            if (idContacto <= 0)
                throw new ArgumentException(
                    "El identificador del contacto no es válido.",
                    nameof(idContacto));

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException(
                    "No se encontró el token de autenticación.");

            string endpointBase = _configuracion.ObtenerMetodo(
                "ApiEndPoints",
                "ObtenerContactos");

            string endpoint =
                $"{endpointBase.TrimEnd('/')}/{idContacto}";

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Put,
                endpoint);

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            solicitud.Content = JsonContent.Create(contacto);

            using var respuesta = await _httpClient.SendAsync(solicitud);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException(
                    "La sesión no está autorizada para editar contactos.");

            if (respuesta.StatusCode == HttpStatusCode.Forbidden)
                throw new InvalidOperationException(
                    "No tiene permisos para editar este contacto.");

            respuesta.EnsureSuccessStatusCode();
        }
        public async Task ActivarContactoAsync(
            int idContacto,
            string token)
        {
            await CambiarEstadoContactoAsync(
                idContacto,
                token,
                "activar");
        }

        public async Task DesactivarContactoAsync(
            int idContacto,
            string token)
        {
            await CambiarEstadoContactoAsync(
                idContacto,
                token,
                "desactivar");
        }

        private async Task CambiarEstadoContactoAsync(
            int idContacto,
            string token,
            string accion)
        {
            if (idContacto <= 0)
                throw new ArgumentException(
                    "El identificador del contacto no es válido.",
                    nameof(idContacto));

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException(
                    "No se encontró el token de autenticación.");

            if (accion != "activar" && accion != "desactivar")
                throw new ArgumentException(
                    "La acción solicitada no es válida.",
                    nameof(accion));

            string endpointBase = _configuracion.ObtenerMetodo(
                "ApiEndPoints",
                "ObtenerContactos");

            string endpoint =
                $"{endpointBase.TrimEnd('/')}/{idContacto}/{accion}";

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Put,
                endpoint);

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var respuesta = await _httpClient.SendAsync(solicitud);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException(
                    "La sesión no está autorizada para cambiar el estado del contacto.");

            if (respuesta.StatusCode == HttpStatusCode.Forbidden)
                throw new InvalidOperationException(
                    "No tiene permisos para cambiar el estado de este contacto.");

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
                throw new KeyNotFoundException(
                    "El contacto solicitado no existe.");

            if (respuesta.StatusCode == HttpStatusCode.Conflict)
                throw new HttpRequestException(
                    "No se pudo cambiar el estado del contacto debido a un conflicto.",
                    null,
                    HttpStatusCode.Conflict);

            respuesta.EnsureSuccessStatusCode();
        }
    }
}

