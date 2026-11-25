using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Web.Servicios;

public class ProductoApiService(HttpClient cliente, IConfiguracion configuracion)
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ProductoCatalogoRespuesta> ObtenerProductosAsync(
        string token,
        int idEmpresa,
        string? nombre,
        int pagina,
        int tamanoPagina,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "No se encontró el token de autenticación.");
        }

        var endpoint = configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerProductos");
        var consulta = new List<string>
        {
            $"idEmpresa={idEmpresa}",
            $"pagina={pagina}",
            $"tamanoPagina={tamanoPagina}"
        };

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            consulta.Add($"nombre={Uri.EscapeDataString(nombre.Trim())}");
        }

        using var solicitud = new HttpRequestMessage(
            HttpMethod.Get,
            $"{endpoint}?{string.Join("&", consulta)}");

        solicitud.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var respuesta = await cliente.SendAsync(solicitud, ct);

        if (respuesta.StatusCode == HttpStatusCode.BadRequest)
        {
            var texto = (await respuesta.Content.ReadAsStringAsync(ct)).Trim().Trim('"');
            throw new HttpRequestException(
                string.IsNullOrWhiteSpace(texto)
                    ? "No se pudo consultar los productos."
                    : texto,
                null,
                HttpStatusCode.BadRequest);
        }

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new HttpRequestException(
                "La sesión no está autorizada para consultar productos.",
                null,
                HttpStatusCode.Unauthorized);
        }

        if (respuesta.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new HttpRequestException(
                "No tiene permisos para consultar productos.",
                null,
                HttpStatusCode.Forbidden);
        }

        respuesta.EnsureSuccessStatusCode();

        var resultado = await respuesta.Content
            .ReadFromJsonAsync<ProductoCatalogoRespuesta>(Json, ct);

        if (resultado == null)
        {
            throw new InvalidOperationException(
                "La API no devolvió el catálogo de productos.");
        }

        resultado.Items ??= new List<ProductoCatalogoItem>();
        return resultado;
    }

    public static bool EsErrorDeCarga(Exception ex, CancellationToken ct) =>
        ex is HttpRequestException or JsonException or InvalidOperationException
        || (ex is OperationCanceledException && !ct.IsCancellationRequested);
}
