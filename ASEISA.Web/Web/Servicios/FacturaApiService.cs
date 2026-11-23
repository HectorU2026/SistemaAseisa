using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Web.Servicios;

public class FacturaApiService(HttpClient cliente, IConfiguracion configuracion)
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<List<FacturaListado>> ObtenerFacturasAsync(
        string token,
        FacturaFiltro filtro,
        CancellationToken ct)
    {
        var consulta = new List<string>();

        if (!string.IsNullOrWhiteSpace(filtro.NumeroFactura))
            consulta.Add($"numeroFactura={Uri.EscapeDataString(filtro.NumeroFactura.Trim())}");

        if (!string.IsNullOrWhiteSpace(filtro.NombreCliente))
            consulta.Add($"nombreCliente={Uri.EscapeDataString(filtro.NombreCliente.Trim())}");

        if (filtro.IdEstadoFactura is > 0)
            consulta.Add($"idEstadoFactura={filtro.IdEstadoFactura.Value}");

        if (filtro.FechaEmisionDesde.HasValue)
        {
            consulta.Add(
                "fechaEmisionDesde=" +
                filtro.FechaEmisionDesde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        if (filtro.FechaEmisionHasta.HasValue)
        {
            consulta.Add(
                "fechaEmisionHasta=" +
                filtro.FechaEmisionHasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        var endpoint = configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerFacturas");
        var url = consulta.Count == 0
            ? endpoint
            : $"{endpoint}?{string.Join("&", consulta)}";

        return LeerListaAsync<FacturaListado>(token, url, ct);
    }

    public Task<List<EstadoFactura>> ObtenerEstadosFacturaAsync(
        string token,
        CancellationToken ct)
    {
        var endpoint = configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerEstadosFactura");
        return LeerListaAsync<EstadoFactura>(token, endpoint, ct);
    }

    private async Task<List<T>> LeerListaAsync<T>(
        string token,
        string url,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "No se encontró el token de autenticación.");
        }

        using var solicitud = new HttpRequestMessage(HttpMethod.Get, url);
        solicitud.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var respuesta = await cliente.SendAsync(solicitud, ct);

        if (respuesta.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new HttpRequestException(
                await LeerErrorValidacionAsync(respuesta, ct),
                null,
                HttpStatusCode.BadRequest);
        }

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new HttpRequestException(
                "La sesión no está autorizada para consultar facturas.",
                null,
                HttpStatusCode.Unauthorized);
        }

        if (respuesta.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new HttpRequestException(
                "No tiene permisos para consultar facturas.",
                null,
                HttpStatusCode.Forbidden);
        }

        respuesta.EnsureSuccessStatusCode();

        var datos = await respuesta.Content.ReadFromJsonAsync<List<T>>(Json, ct);
        return datos ?? new List<T>();
    }

    private static async Task<string> LeerErrorValidacionAsync(
        HttpResponseMessage respuesta,
        CancellationToken ct)
    {
        var tipo = respuesta.Content.Headers.ContentType?.MediaType;
        var texto = (await respuesta.Content.ReadAsStringAsync(ct)).Trim();

        if (tipo == "application/problem+json")
        {
            var problema = JsonSerializer.Deserialize<ValidationProblemDetails>(texto, Json);
            return problema?.Errors.Values.SelectMany(v => v).FirstOrDefault()
                ?? problema?.Detail
                ?? problema?.Title
                ?? "Revisá los datos e intentá nuevamente.";
        }

        if (tipo == "text/plain")
            return string.IsNullOrWhiteSpace(texto)
                ? "No se pudo consultar las facturas."
                : texto;

        if (tipo == "application/json" && texto.StartsWith('"'))
        {
            var mensaje = JsonSerializer.Deserialize<string>(texto);
            if (!string.IsNullOrWhiteSpace(mensaje))
                return mensaje;
        }

        return string.IsNullOrWhiteSpace(texto)
            ? "No se pudo completar la operación. Revisá los datos e intentá nuevamente."
            : texto.Trim('"');
    }

    public static bool EsErrorDeCarga(Exception ex, CancellationToken ct) =>
        ex is HttpRequestException or JsonException or InvalidOperationException
        || (ex is OperationCanceledException && !ct.IsCancellationRequested);
}
