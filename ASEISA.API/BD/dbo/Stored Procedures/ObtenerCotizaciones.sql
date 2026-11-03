
CREATE   PROCEDURE [dbo].[ObtenerCotizaciones]
    @id_empresa INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.id_cotizacion         AS IdCotizacion,
        c.id_empresa            AS IdEmpresa,
        c.id_cliente            AS IdCliente,
        cl.nombre_razon_social  AS NombreCliente, 
        c.numero_cotizacion     AS NumeroCotizacion,
        c.fecha_emision         AS FechaEmision,
        c.fecha_vencimiento     AS FechaVencimiento,
        c.moneda                AS Moneda,
        c.tipo_cambio           AS TipoCambio,
        c.subtotal              AS Subtotal,
        c.impuesto              AS Impuesto,
        c.descuento             AS Descuento,
        c.total                 AS Total,
        c.id_estado_cotizacion  AS IdEstado,
        ec.nombre_estado        AS Estado
    FROM dbo.COTIZACION c
    INNER JOIN dbo.CLIENTE cl ON c.id_cliente = cl.id_cliente
    INNER JOIN dbo.ESTADO_COTIZACION ec ON c.id_estado_cotizacion = ec.id_estado_cotizacion
    WHERE c.id_empresa = @id_empresa
    ORDER BY c.fecha_emision DESC;
END;