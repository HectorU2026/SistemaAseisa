CREATE PROCEDURE dbo.ObtenerReporteFacturacion
    @id_empresa   INT,
    @periodo      VARCHAR(20) = NULL,
    @tipo_factura VARCHAR(50) = NULL,
    @ordenar_por  VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @periodo = NULLIF(LTRIM(RTRIM(@periodo)), '');
    SET @tipo_factura = NULLIF(LTRIM(RTRIM(@tipo_factura)), '');
    SET @ordenar_por = NULLIF(LTRIM(RTRIM(@ordenar_por)), '');

    IF @ordenar_por IS NULL OR @ordenar_por <> N'tipo_factura'
        SET @ordenar_por = N'periodo';

    ;WITH nc_por_factura AS
    (
        SELECT
            nc.id_factura,
            SUM(nc.monto) AS TotalNotasCredito
        FROM dbo.NOTA_CREDITO nc
        INNER JOIN dbo.ESTADO e
            ON e.id_estado = nc.id_estado
           AND e.nombre_estado = N'Activo'
        INNER JOIN dbo.FACTURA f
            ON f.id_factura = nc.id_factura
           AND f.id_empresa = @id_empresa
        GROUP BY nc.id_factura
    )
    SELECT
        CONVERT(CHAR(7), f.fecha_emision, 126) AS Periodo,
        f.tipo_factura                         AS TipoFactura,
        f.moneda                               AS Moneda,
        f.id_factura                           AS IdFactura,
        f.numero_factura                       AS NumeroFactura,
        cl.nombre_razon_social                 AS NombreCliente,
        f.fecha_emision                        AS FechaEmision,
        ef.nombre_estado                       AS Estado,
        f.subtotal                             AS Subtotal,
        f.impuesto                             AS Impuesto,
        f.descuento                            AS Descuento,
        f.total                                AS Total,
        f.saldo_pendiente                      AS SaldoPendiente,
        ISNULL(nc.TotalNotasCredito, 0)        AS TotalNotasCreditoFactura
    INTO #reporte
    FROM dbo.FACTURA f
    INNER JOIN dbo.ESTADO_FACTURA ef
        ON ef.id_estado_factura = f.id_estado_factura
    INNER JOIN dbo.VENTAS v
        ON v.id_venta = f.id_venta
       AND v.id_empresa = f.id_empresa
    INNER JOIN dbo.CLIENTE cl
        ON cl.id_cliente = v.id_cliente
       AND cl.id_empresa = f.id_empresa
    LEFT JOIN nc_por_factura nc
        ON nc.id_factura = f.id_factura
    WHERE f.id_empresa = @id_empresa
      AND ef.nombre_estado IN (N'Emitida', N'Pagada')
      AND (@periodo IS NULL OR CONVERT(CHAR(7), f.fecha_emision, 126) = @periodo)
      AND (@tipo_factura IS NULL OR f.tipo_factura = @tipo_factura);

    SELECT
        r.Periodo,
        r.TipoFactura,
        r.Moneda,
        COUNT(*) AS CantidadFacturas,
        SUM(r.Total) AS TotalFacturado,
        SUM(r.TotalNotasCreditoFactura) AS TotalNotasCredito
    FROM #reporte r
    GROUP BY
        r.Periodo,
        r.TipoFactura,
        r.Moneda
    ORDER BY
        CASE WHEN @ordenar_por = N'tipo_factura' THEN r.TipoFactura END ASC,
        CASE WHEN @ordenar_por = N'periodo' THEN r.Periodo END ASC,
        CASE WHEN @ordenar_por = N'tipo_factura' THEN r.Periodo END ASC,
        CASE WHEN @ordenar_por = N'periodo' THEN r.TipoFactura END ASC,
        r.Moneda ASC;

    SELECT
        r.Periodo,
        r.TipoFactura,
        r.Moneda,
        r.IdFactura,
        r.NumeroFactura,
        r.NombreCliente,
        r.FechaEmision,
        r.Estado,
        r.Subtotal,
        r.Impuesto,
        r.Descuento,
        r.Total,
        r.SaldoPendiente,
        r.TotalNotasCreditoFactura
    FROM #reporte r
    ORDER BY
        CASE WHEN @ordenar_por = N'tipo_factura' THEN r.TipoFactura END ASC,
        CASE WHEN @ordenar_por = N'periodo' THEN r.Periodo END ASC,
        CASE WHEN @ordenar_por = N'tipo_factura' THEN r.Periodo END ASC,
        CASE WHEN @ordenar_por = N'periodo' THEN r.TipoFactura END ASC,
        r.Moneda ASC,
        r.IdFactura ASC;

    DROP TABLE #reporte;
END;
