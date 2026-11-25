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

    public async Task<FlujoFacturaConsulta> ObtenerFlujoFacturasAsync(
        string token,
        FlujoFacturaFiltro filtro,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "No se encontró el token de autenticación.");
        }

        var consulta = new List<string>();

        if (filtro.IdCotizacion is > 0)
            consulta.Add($"idCotizacion={filtro.IdCotizacion.Value}");

        if (!string.IsNullOrWhiteSpace(filtro.NumeroFactura))
            consulta.Add($"numeroFactura={Uri.EscapeDataString(filtro.NumeroFactura.Trim())}");

        if (!string.IsNullOrWhiteSpace(filtro.OrdenarPor))
            consulta.Add($"ordenarPor={Uri.EscapeDataString(filtro.OrdenarPor.Trim())}");

        var endpoint = configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerFlujoFacturas");
        var url = consulta.Count == 0
            ? endpoint
            : $"{endpoint}?{string.Join("&", consulta)}";

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

        var resultado = await respuesta.Content
            .ReadFromJsonAsync<FlujoFacturaConsulta>(Json, ct);

        if (resultado == null)
        {
            throw new InvalidOperationException(
                "La API no devolvió el flujo de facturación.");
        }

        resultado.Items ??= new List<FlujoFacturaListado>();
        return resultado;
    }

    public async Task<ReporteFacturacionConsulta> ObtenerReporteFacturacionAsync(
        string token,
        ReporteFacturacionFiltro filtro,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "No se encontró el token de autenticación.");
        }

        var consulta = new List<string>();

        if (!string.IsNullOrWhiteSpace(filtro.Periodo))
            consulta.Add($"periodo={Uri.EscapeDataString(filtro.Periodo.Trim())}");

        if (!string.IsNullOrWhiteSpace(filtro.TipoFactura))
            consulta.Add($"tipoFactura={Uri.EscapeDataString(filtro.TipoFactura.Trim())}");

        if (!string.IsNullOrWhiteSpace(filtro.OrdenarPor))
            consulta.Add($"ordenarPor={Uri.EscapeDataString(filtro.OrdenarPor.Trim())}");

        var endpoint = configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerReporteFacturacion");
        var url = consulta.Count == 0
            ? endpoint
            : $"{endpoint}?{string.Join("&", consulta)}";

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

        var resultado = await respuesta.Content
            .ReadFromJsonAsync<ReporteFacturacionConsulta>(Json, ct);

        if (resultado == null)
        {
            throw new InvalidOperationException(
                "La API no devolvió el reporte de facturación.");
        }

        resultado.Grupos ??= new List<ReporteFacturacionGrupo>();
        foreach (var grupo in resultado.Grupos)
            grupo.Facturas ??= new List<ReporteFacturacionLinea>();

        return resultado;
    }

    public async Task<ReporteFacturacionDetalle> ObtenerDetalleReporteFacturacionAsync(
        string token,
        int idFactura,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "No se encontró el token de autenticación.");
        }

        var endpoint = configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerReporteFacturacion");
        using var solicitud = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/{idFactura}");
        solicitud.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var respuesta = await cliente.SendAsync(solicitud, ct);

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            throw new HttpRequestException(
                await LeerErrorValidacionAsync(respuesta, ct),
                null,
                HttpStatusCode.NotFound);
        }

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

        var detalle = await respuesta.Content
            .ReadFromJsonAsync<ReporteFacturacionDetalle>(Json, ct);

        if (detalle?.Factura == null)
        {
            throw new InvalidOperationException(
                "La API no devolvió el detalle del reporte de facturación.");
        }

        detalle.NotasCredito ??= new List<ReporteNotaCredito>();
        return detalle;
    }

    public async Task<FlujoFacturaDetalle> ObtenerFlujoFacturaAsync(
        string token,
        int idFactura,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "No se encontró el token de autenticación.");
        }

        var endpoint = configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerFlujoFacturas");
        using var solicitud = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/{idFactura}");
        solicitud.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var respuesta = await cliente.SendAsync(solicitud, ct);

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            throw new HttpRequestException(
                await LeerErrorValidacionAsync(respuesta, ct),
                null,
                HttpStatusCode.NotFound);
        }

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

        var detalle = await respuesta.Content
            .ReadFromJsonAsync<FlujoFacturaDetalle>(Json, ct);

        if (detalle?.Factura == null || detalle.Resumen == null)
        {
            throw new InvalidOperationException(
                "La API no devolvió el detalle del flujo de facturación.");
        }

        return detalle;
    }

    public async Task<FacturaPagadaConsulta> ObtenerFacturasPagadasAsync(
        string token,
        FacturaPagadaFiltro filtro,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "No se encontró el token de autenticación.");
        }

        var consulta = new List<string>();

        if (!string.IsNullOrWhiteSpace(filtro.NombreCliente))
            consulta.Add($"nombreCliente={Uri.EscapeDataString(filtro.NombreCliente.Trim())}");

        if (filtro.FechaPagoDesde.HasValue)
        {
            consulta.Add(
                "fechaPagoDesde=" +
                filtro.FechaPagoDesde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        if (filtro.FechaPagoHasta.HasValue)
        {
            consulta.Add(
                "fechaPagoHasta=" +
                filtro.FechaPagoHasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        var endpoint = configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerFacturasPagadas");
        var url = consulta.Count == 0
            ? endpoint
            : $"{endpoint}?{string.Join("&", consulta)}";

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

        var resultado = await respuesta.Content
            .ReadFromJsonAsync<FacturaPagadaConsulta>(Json, ct);

        if (resultado == null)
        {
            throw new InvalidOperationException(
                "La API no devolvió las facturas pagadas.");
        }

        resultado.Facturas ??= new List<FacturaPagadaListado>();
        return resultado;
    }

    public async Task<FacturaPagadaDetalle> ObtenerFacturaPagadaAsync(
        string token,
        int idFactura,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "No se encontró el token de autenticación.");
        }

        var endpoint = configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerFacturasPagadas");
        using var solicitud = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/{idFactura}");
        solicitud.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var respuesta = await cliente.SendAsync(solicitud, ct);

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            throw new HttpRequestException(
                await LeerErrorValidacionAsync(respuesta, ct),
                null,
                HttpStatusCode.NotFound);
        }

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

        var detalle = await respuesta.Content
            .ReadFromJsonAsync<FacturaPagadaDetalle>(Json, ct);

        if (detalle?.Factura == null)
        {
            throw new InvalidOperationException(
                "La API no devolvió el detalle de la factura pagada.");
        }

        detalle.Pagos ??= new List<FacturaPagadaPago>();
        return detalle;
    }

    public async Task<FacturaDetalle> ObtenerFacturaAsync(
        string token,
        int idFactura,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "No se encontró el token de autenticación.");
        }

        var endpoint = configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerFacturas");
        using var solicitud = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/{idFactura}");
        solicitud.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var respuesta = await cliente.SendAsync(solicitud, ct);

        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            throw new HttpRequestException(
                await LeerErrorValidacionAsync(respuesta, ct),
                null,
                HttpStatusCode.NotFound);
        }

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

        return await respuesta.Content.ReadFromJsonAsync<FacturaDetalle>(Json, ct)
            ?? throw new InvalidOperationException("La API no devolvió el detalle de la factura.");
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
