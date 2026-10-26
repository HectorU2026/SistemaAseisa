CREATE PROCEDURE dbo.ObtenerFactura
    @id_factura INT,
    @id_empresa INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        f.id_factura           AS IdFactura,
        f.id_empresa           AS IdEmpresa,
        f.id_venta             AS IdVenta,
        f.id_usuario           AS IdUsuario,
        v.id_cliente           AS IdCliente,
        v.id_cotizacion        AS IdCotizacion,
        cl.nombre_razon_social AS NombreCliente,
        cl.identificacion      AS Identificacion,
        cl.correo              AS CorreoCliente,
        f.numero_factura       AS NumeroFactura,
        v.numero_comprobante   AS NumeroComprobante,
        v.condicion_venta      AS CondicionVenta,
        f.tipo_factura         AS TipoFactura,
        f.fecha_emision        AS FechaEmision,
        f.fecha_vencimiento    AS FechaVencimiento,
        f.id_termino_pago      AS IdTerminoPago,
        f.termino_pago_nombre  AS TerminoPagoNombre,
        f.moneda               AS Moneda,
        f.tipo_cambio          AS TipoCambio,
        f.subtotal             AS Subtotal,
        f.impuesto             AS Impuesto,
        f.descuento            AS Descuento,
        f.total                AS Total,
        f.saldo_pendiente      AS SaldoPendiente,
        f.motivo_anulacion     AS MotivoAnulacion,
        f.id_estado_factura    AS IdEstadoFactura,
        ef.nombre_estado       AS Estado,
        f.version_fila         AS VersionFila
    INTO #cabecera
    FROM dbo.FACTURA f
    INNER JOIN dbo.VENTAS v
        ON v.id_venta = f.id_venta
       AND v.id_empresa = f.id_empresa
    INNER JOIN dbo.CLIENTE cl
        ON cl.id_cliente = v.id_cliente
       AND cl.id_empresa = f.id_empresa
    INNER JOIN dbo.ESTADO_FACTURA ef
        ON ef.id_estado_factura = f.id_estado_factura
    WHERE f.id_factura = @id_factura
      AND f.id_empresa = @id_empresa;

    SELECT
        df.id_producto          AS IdProducto,
        p.codigo                AS Codigo,
        p.nombre                AS Nombre,
        df.cantidad             AS Cantidad,
        df.precio_unitario      AS PrecioUnitario,
        df.porcentaje_descuento AS PorcentajeDescuento,
        df.porcentaje_impuesto  AS PorcentajeImpuesto,
        df.subtotal             AS Subtotal
    INTO #productos
    FROM dbo.DETALLE_FACTURA df
    INNER JOIN dbo.PRODUCTO p
        ON p.id_producto = df.id_producto
       AND p.id_empresa = @id_empresa
    WHERE df.id_factura = @id_factura
      AND EXISTS (SELECT 1 FROM #cabecera);

    SELECT
        a.id_asiento     AS IdAsiento,
        a.numero_asiento AS NumeroAsiento,
        a.fecha_asiento  AS FechaAsiento,
        a.descripcion    AS Descripcion,
        a.tipo_origen    AS TipoOrigen,
        a.total_debe     AS TotalDebe,
        a.total_haber    AS TotalHaber,
        a.id_estado      AS IdEstado,
        e.nombre_estado  AS Estado
    INTO #asiento
    FROM dbo.ASIENTO_CONTABLE a
    INNER JOIN dbo.ESTADO e
        ON e.id_estado = a.id_estado
    INNER JOIN #cabecera c
        ON a.id_empresa = c.IdEmpresa
       AND a.tipo_origen = 'Factura'
       AND a.numero_asiento = CONCAT('AST-FAC-', CONVERT(VARCHAR(11), c.IdFactura));

    SELECT
        da.id_asiento         AS IdAsiento,
        da.id_cuenta_contable AS IdCuentaContable,
        cc.codigo             AS Codigo,
        cc.nombre             AS Nombre,
        da.descripcion        AS Descripcion,
        da.debe               AS Debe,
        da.haber              AS Haber
    INTO #movimientos
    FROM dbo.DETALLE_ASIENTO da
    INNER JOIN dbo.CUENTA_CONTABLE cc
        ON cc.id_cuenta_contable = da.id_cuenta_contable
       AND cc.id_empresa = @id_empresa
    INNER JOIN #asiento a
        ON a.IdAsiento = da.id_asiento;

    SELECT * FROM #cabecera;
    SELECT * FROM #productos ORDER BY IdProducto;
    SELECT * FROM #asiento;
    SELECT * FROM #movimientos ORDER BY IdCuentaContable;
END;
