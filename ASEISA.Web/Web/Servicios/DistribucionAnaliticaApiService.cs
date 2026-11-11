using Abstracciones.Modelos;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace Web.Servicios;

public class DistribucionAnaliticaApiService(HttpClient cliente, IConfiguration configuracion)
{
    public async Task<List<DistribucionAnaliticaResponse>> ObtenerAsync(
        string token, CancellationToken cancelacion)
    {
        using var respuesta = await EnviarAsync(HttpMethod.Get, "", token, cancelacion);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<List<DistribucionAnaliticaResponse>>(
            cancellationToken: cancelacion) ?? new();
    }

    public async Task InhabilitarAsync(int id, string token, CancellationToken cancelacion)
    {
        using var respuesta = await EnviarAsync(HttpMethod.Put, $"/{id}/inhabilitar", token, cancelacion);
        respuesta.EnsureSuccessStatusCode();
    }

    public async Task<DistribucionAnaliticaResponse?> ObtenerDetalleAsync(
        int id, string token, CancellationToken cancelacion)
    {
        using var respuesta = await EnviarAsync(HttpMethod.Get, $"/{id}", token, cancelacion);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<DistribucionAnaliticaResponse>(
            cancellationToken: cancelacion);
    }

    public async Task<Catalogos> ObtenerCatalogosAsync(string token, CancellationToken cancelacion)
    {
        using var respuesta = await EnviarAsync(HttpMethod.Get, "/catalogos", token, cancelacion,
            recurso: "Compra");
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Catalogos>(cancellationToken: cancelacion)
            ?? throw new InvalidOperationException("No se encontraron los catálogos.");
    }

    public async Task<string?> GuardarAsync(int? id, DistribucionAnaliticaRequest datos,
        string token, CancellationToken cancelacion)
    {
        using var respuesta = await EnviarAsync(id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/{id}" : "", token, cancelacion, datos);
        if (respuesta.IsSuccessStatusCode) return null;
        if (respuesta.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            if (respuesta.Content.Headers.ContentType?.MediaType == "application/problem+json")
            {
                var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>(
                    cancellationToken: cancelacion);
                var mensaje = problema?.Errors.Values.SelectMany(v => v).FirstOrDefault();
                return mensaje ?? "Revisá los datos del formulario.";
            }

            if (respuesta.Content.Headers.ContentType?.MediaType == "text/plain")
                return await respuesta.Content.ReadAsStringAsync(cancelacion);

            return "Revisá los datos del formulario.";
        }

        respuesta.EnsureSuccessStatusCode();
        return null;
    }

    public class Catalogos
    {
        public List<ProveedorCatalogo> Proveedores { get; set; } = new();
        public List<ProductoCatalogo> Productos { get; set; } = new();
    }

    public record ProveedorCatalogo(int IdProveedor, string Nombre);
    public record ProductoCatalogo(int IdProducto, string Nombre);

    private async Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string ruta,
        string token, CancellationToken cancelacion, DistribucionAnaliticaRequest? datos = null,
        string recurso = "DistribucionAnalitica")
    {
        var baseApi = configuracion["ApiEndPoints:UrlBase"]
            ?? throw new InvalidOperationException("No se configuró la URL de la API.");
        using var solicitud = new HttpRequestMessage(metodo,
            $"{baseApi.TrimEnd('/')}/{recurso}{ruta}");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (datos is not null) solicitud.Content = JsonContent.Create(datos);
        return await cliente.SendAsync(solicitud, cancelacion);
    }
}
