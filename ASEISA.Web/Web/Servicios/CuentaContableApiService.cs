using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Web.Servicios;

public class CuentaContableApiService(HttpClient cliente, IConfiguration configuracion)
{
    public Task<List<CuentaFila>> ObtenerAsync(string token, CancellationToken ct)
        => LeerAsync<List<CuentaFila>>("", token, ct);

    public Task<CuentaFila> ObtenerDetalleAsync(int id, string token, CancellationToken ct)
        => LeerAsync<CuentaFila>($"/{id}", token, ct);

    private async Task<T> LeerAsync<T>(string ruta, string token, CancellationToken ct) where T : class
    {
        using var respuesta = await EnviarAsync(HttpMethod.Get, ruta, token, ct);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<T>(cancellationToken: ct)
            ?? throw new InvalidOperationException("La API no devolvió datos.");
    }

    public async Task<string?> AgregarRaizAsync(object datos, string token, CancellationToken ct)
    {
        using var respuesta = await EnviarAsync(HttpMethod.Post, "/raiz", token, ct, datos);
        return await ObtenerErrorAsync(respuesta, ct);
    }

    public async Task<string?> GuardarAsync(int? id, object datos, string token, CancellationToken ct)
    {
        using var respuesta = await EnviarAsync(id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            id.HasValue ? $"/{id}" : "", token, ct, datos);
        return await ObtenerErrorAsync(respuesta, ct);
    }

    public async Task<string?> CambiarEstadoAsync(int id, bool habilitar, string token, CancellationToken ct)
    {
        var accion = habilitar ? "habilitar" : "inhabilitar";
        using var respuesta = await EnviarAsync(HttpMethod.Put, $"/{id}/{accion}", token, ct);
        return await ObtenerErrorAsync(respuesta, ct);
    }

    private static async Task<string?> ObtenerErrorAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        if (respuesta.IsSuccessStatusCode) return null;
        if (respuesta.StatusCode is not (HttpStatusCode.BadRequest or HttpStatusCode.Conflict))
            respuesta.EnsureSuccessStatusCode();

        var tipo = respuesta.Content.Headers.ContentType?.MediaType;
        if (tipo == "application/problem+json")
        {
            var problema = await respuesta.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: ct);
            return problema?.Errors.Values.SelectMany(v => v).FirstOrDefault()
                ?? "Revisá los datos e intentá nuevamente.";
        }
        if (tipo == "text/plain")
            return await respuesta.Content.ReadAsStringAsync(ct);

        return "No se pudo completar la operación. Revisá los datos e intentá nuevamente.";
    }

    private async Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string ruta,
        string token, CancellationToken ct, object? datos = null)
    {
        var baseApi = configuracion["ApiEndPoints:UrlBase"]
            ?? throw new InvalidOperationException("No se configuró la URL de la API.");
        using var solicitud = new HttpRequestMessage(metodo, $"{baseApi.TrimEnd('/')}/CuentaContable{ruta}");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (datos is not null) solicitud.Content = JsonContent.Create(datos);
        return await cliente.SendAsync(solicitud, ct);
    }

    public static bool EsErrorDeCarga(Exception ex, CancellationToken ct) =>
        ex is HttpRequestException or System.Text.Json.JsonException or InvalidOperationException
        || (ex is OperationCanceledException && !ct.IsCancellationRequested);

    public record CuentaFila(int IdCuentaContable, string Codigo, string Nombre, string TipoCuenta,
        string Naturaleza, int Nivel, int IdEstado, string Estado, int? IdCuentaPrincipal,
        string? CuentaPrincipal, bool ConciliacionPagos, decimal? Impuestos, string? Descripcion);
}
