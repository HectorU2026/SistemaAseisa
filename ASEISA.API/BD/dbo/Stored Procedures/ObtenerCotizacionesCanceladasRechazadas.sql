
CREATE   PROCEDURE [dbo].[ObtenerCotizacionesCanceladasRechazadas]
    @id_empresa INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_cancelada INT;
    DECLARE @id_rechazada INT;

    SELECT @id_cancelada = id_estado_cotizacion FROM ESTADO_COTIZACION WHERE nombre_estado = 'Cancelada';
    SELECT @id_rechazada = id_estado_cotizacion FROM ESTADO_COTIZACION WHERE nombre_estado = 'Rechazada';

    SELECT
        c.id_cotizacion         AS IdCotizacion,
        c.id_empresa            AS IdEmpresa,
        c.id_cliente            AS IdCliente,
        c.numero_cotizacion     AS NumeroCotizacion,
        c.fecha_emision         AS FechaEmision,
        c.fecha_vencimiento     AS FechaVencimiento,
        c.moneda                AS Moneda,
        c.subtotal              AS Subtotal,
        c.impuesto              AS Impuesto,
        c.descuento             AS Descuento,
        c.total                 AS Total,
        c.id_estado_cotizacion  AS IdEstado, 
        ec.nombre_estado        AS NombreEstado,
        cli.nombre_razon_social AS NombreRazonSocial
    FROM COTIZACION c
    INNER JOIN ESTADO_COTIZACION ec ON ec.id_estado_cotizacion = c.id_estado_cotizacion
    INNER JOIN CLIENTE cli ON cli.id_cliente = c.id_cliente
    WHERE c.id_estado_cotizacion IN (@id_cancelada, @id_rechazada)
      AND c.id_empresa = @id_empresa;

    SELECT
        d.id_cotizacion         AS IdCotizacion,
        d.id_detalle_cotizacion AS IdDetalleCotizacion,
        d.id_producto           AS IdProducto,
        p.nombre                AS NombreProducto,
        d.cantidad              AS Cantidad,
        d.precio_unitario       AS PrecioUnitario,
        d.subtotal              AS Subtotal
    FROM DETALLE_COTIZACION d
    INNER JOIN COTIZACION c ON c.id_cotizacion = d.id_cotizacion
    INNER JOIN PRODUCTO p ON p.id_producto = d.id_producto
    WHERE c.id_estado_cotizacion IN (@id_cancelada, @id_rechazada)
      AND c.id_empresa = @id_empresa;
END;