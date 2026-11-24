CREATE PROCEDURE dbo.ObtenerFlujoFacturas
    @id_empresa     INT,
    @id_cotizacion  INT = NULL,
    @numero_factura VARCHAR(50) = NULL,
    @ordenar_por    VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @numero_factura = NULLIF(LTRIM(RTRIM(@numero_factura)), '');
    SET @ordenar_por = NULLIF(LTRIM(RTRIM(@ordenar_por)), '');

    IF @ordenar_por IS NULL OR @ordenar_por <> N'numeroFactura'
        SET @ordenar_por = N'id_cotizacion';

    ;WITH pagos_venta AS
    (
        SELECT
            cxc.id_venta,
            SUM(p.monto) AS cobrado_venta
        FROM dbo.PAGO_CLIENTE p
        INNER JOIN dbo.CUENTA_POR_COBRAR cxc
            ON cxc.id_cuenta_cobrar = p.id_cuenta_cobrar
        INNER JOIN dbo.VENTAS v
            ON v.id_venta = cxc.id_venta
           AND v.id_empresa = @id_empresa
        GROUP BY cxc.id_venta
    ),
    cobrado_orden AS
    (
        SELECT
            v.id_cotizacion,
            SUM(ISNULL(pv.cobrado_venta, 0)) AS total_cobrado_orden
        FROM dbo.VENTAS v
        LEFT JOIN pagos_venta pv
            ON pv.id_venta = v.id_venta
        WHERE v.id_empresa = @id_empresa
          AND v.id_cotizacion IS NOT NULL
        GROUP BY v.id_cotizacion
    ),
    facturado_orden AS
    (
        SELECT
            v.id_cotizacion,
            SUM(
                CASE
                    WHEN ef.nombre_estado IN (N'Emitida', N'Pagada')
                    THEN f.total
                    ELSE 0
                END
            ) AS total_facturado
        FROM dbo.FACTURA f
        INNER JOIN dbo.VENTAS v
            ON v.id_venta = f.id_venta
           AND v.id_empresa = f.id_empresa
        INNER JOIN dbo.ESTADO_FACTURA ef
            ON ef.id_estado_factura = f.id_estado_factura
        WHERE f.id_empresa = @id_empresa
          AND v.id_cotizacion IS NOT NULL
        GROUP BY v.id_cotizacion
    ),
    facturas_venta AS
    (
        SELECT
            f.id_venta,
            COUNT(*) AS cantidad_facturas_venta
        FROM dbo.FACTURA f
        WHERE f.id_empresa = @id_empresa
        GROUP BY f.id_venta
    )
    SELECT
        c.id_cotizacion            AS IdCotizacion,
        c.numero_cotizacion        AS NumeroCotizacion,
        ec.nombre_estado           AS EstadoCotizacion,
        c.total                    AS TotalOrden,
        ISNULL(fo.total_facturado, 0) AS TotalFacturado,
        ISNULL(co.total_cobrado_orden, 0) AS TotalCobradoOrden,
        ISNULL(fo.total_facturado, 0) - ISNULL(co.total_cobrado_orden, 0) AS SaldoPendienteOrden,
        v.id_venta                 AS IdVenta,
        f.id_factura               AS IdFactura,
        f.numero_factura           AS NumeroFactura,
        ef.nombre_estado           AS EstadoFactura,
        cl.nombre_razon_social     AS NombreCliente,
        f.fecha_emision            AS FechaEmision,
        f.total                    AS TotalFactura,
        f.saldo_pendiente          AS SaldoPendienteFactura,
        fv.cantidad_facturas_venta AS CantidadFacturasVenta,
        ISNULL(pv.cobrado_venta, 0) AS MontoCobradoVenta,
        CASE
            WHEN fv.cantidad_facturas_venta = 1
            THEN ISNULL(pv.cobrado_venta, 0)
            ELSE CAST(NULL AS DECIMAL(18, 2))
        END                        AS MontoCobradoFactura
    FROM dbo.FACTURA f
    INNER JOIN dbo.VENTAS v
        ON v.id_venta = f.id_venta
       AND v.id_empresa = f.id_empresa
    INNER JOIN dbo.COTIZACION c
        ON c.id_cotizacion = v.id_cotizacion
       AND c.id_empresa = f.id_empresa
    INNER JOIN dbo.ESTADO_FACTURA ef
        ON ef.id_estado_factura = f.id_estado_factura
    INNER JOIN dbo.ESTADO_COTIZACION ec
        ON ec.id_estado_cotizacion = c.id_estado_cotizacion
    INNER JOIN dbo.CLIENTE cl
        ON cl.id_cliente = v.id_cliente
       AND cl.id_empresa = f.id_empresa
    INNER JOIN facturas_venta fv
        ON fv.id_venta = v.id_venta
    LEFT JOIN pagos_venta pv
        ON pv.id_venta = v.id_venta
    LEFT JOIN cobrado_orden co
        ON co.id_cotizacion = c.id_cotizacion
    LEFT JOIN facturado_orden fo
        ON fo.id_cotizacion = c.id_cotizacion
    WHERE f.id_empresa = @id_empresa
      AND (@id_cotizacion IS NULL OR c.id_cotizacion = @id_cotizacion)
      AND (@numero_factura IS NULL OR f.numero_factura = @numero_factura)
    ORDER BY
        CASE WHEN @ordenar_por = N'numeroFactura' THEN f.numero_factura END ASC,
        CASE WHEN @ordenar_por = N'id_cotizacion' THEN c.id_cotizacion END ASC,
        f.id_factura ASC;
END;
