CREATE PROCEDURE dbo.ObtenerFacturasPagadas
    @id_empresa        INT,
    @nombre_cliente    VARCHAR(200) = NULL,
    @fecha_pago_desde  DATE = NULL,
    @fecha_pago_hasta  DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @nombre_cliente = NULLIF(LTRIM(RTRIM(@nombre_cliente)), '');

    ;WITH facturas_por_venta AS
    (
        SELECT
            f.id_venta,
            COUNT(*) AS CantidadFacturas
        FROM dbo.FACTURA f
        GROUP BY f.id_venta
    ),
    cxc_por_venta AS
    (
        SELECT
            cxc.id_venta,
            COUNT(*) AS CantidadCuentas
        FROM dbo.CUENTA_POR_COBRAR cxc
        GROUP BY cxc.id_venta
    ),
    fecha_pago_venta AS
    (
        SELECT
            cxc.id_venta,
            MAX(p.fecha_pago) AS FechaPago
        FROM dbo.PAGO_CLIENTE p
        INNER JOIN dbo.ESTADO e
            ON e.id_estado = p.id_estado
           AND e.nombre_estado = N'Activo'
        INNER JOIN dbo.CUENTA_POR_COBRAR cxc
            ON cxc.id_cuenta_cobrar = p.id_cuenta_cobrar
        INNER JOIN facturas_por_venta fv
            ON fv.id_venta = cxc.id_venta
           AND fv.CantidadFacturas = 1
        INNER JOIN cxc_por_venta cv
            ON cv.id_venta = cxc.id_venta
           AND cv.CantidadCuentas = 1
        GROUP BY cxc.id_venta
    )
    SELECT
        f.id_factura           AS IdFactura,
        f.numero_factura       AS NumeroFactura,
        cl.nombre_razon_social AS NombreCliente,
        f.total                AS MontoPagado,
        fp.FechaPago           AS FechaPago
    FROM dbo.FACTURA f
    INNER JOIN dbo.ESTADO_FACTURA ef
        ON ef.id_estado_factura = f.id_estado_factura
    INNER JOIN dbo.VENTAS v
        ON v.id_venta = f.id_venta
       AND v.id_empresa = f.id_empresa
    INNER JOIN dbo.CLIENTE cl
        ON cl.id_cliente = v.id_cliente
       AND cl.id_empresa = f.id_empresa
    LEFT JOIN fecha_pago_venta fp
        ON fp.id_venta = f.id_venta
    WHERE f.id_empresa = @id_empresa
      AND ef.nombre_estado = N'Pagada'
      AND (@nombre_cliente IS NULL OR CHARINDEX(@nombre_cliente, cl.nombre_razon_social) > 0)
      AND (
            @fecha_pago_desde IS NULL
            OR (
                fp.FechaPago IS NOT NULL
                AND CAST(fp.FechaPago AS DATE) >= @fecha_pago_desde
            )
          )
      AND (
            @fecha_pago_hasta IS NULL
            OR (
                fp.FechaPago IS NOT NULL
                AND CAST(fp.FechaPago AS DATE) <= @fecha_pago_hasta
            )
          )
    ORDER BY
        fp.FechaPago DESC,
        f.id_factura ASC;
END;
