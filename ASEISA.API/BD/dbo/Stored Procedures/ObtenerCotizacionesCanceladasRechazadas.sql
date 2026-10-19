
CREATE   PROCEDURE dbo.ObtenerCotizacionesCanceladasRechazadas
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_cancelada INT;
    DECLARE @id_rechazada INT;

    SELECT @id_cancelada = id_estado FROM ESTADO WHERE nombre_estado = 'Cancelada';
    SELECT @id_rechazada = id_estado FROM ESTADO WHERE nombre_estado = 'Rechazada';

    -- 1. Resultado: Cabeceras de las cotizaciones
    SELECT
        c.id_cotizacion        AS IdCotizacion,
        c.id_empresa           AS IdEmpresa,
        c.id_cliente           AS IdCliente,
        c.numero_cotizacion    AS NumeroCotizacion,
        c.fecha_emision        AS FechaEmision, -- Usada como "Fecha cancelación" en el HTML
        c.fecha_vencimiento    AS FechaVencimiento,
        c.moneda               AS Moneda,
        c.subtotal             AS Subtotal,
        c.impuesto             AS Impuesto,
        c.descuento            AS Descuento,
        c.total                AS Total,
        c.id_estado            AS IdEstado,
        e.nombre_estado        AS NombreEstado,
        cli.nombre_razon_social AS [Cliente.NombreRazonSocial] -- Alias para Dapper
    FROM COTIZACION c
    INNER JOIN ESTADO e ON e.id_estado = c.id_estado
    INNER JOIN CLIENTE cli ON cli.id_cliente = c.id_cliente
    WHERE c.id_estado IN (@id_cancelada, @id_rechazada);

    -- 2. Resultado: Detalles de las cotizaciones (solo de las canceladas/rechazadas)
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
    WHERE c.id_estado IN (@id_cancelada, @id_rechazada);
END;