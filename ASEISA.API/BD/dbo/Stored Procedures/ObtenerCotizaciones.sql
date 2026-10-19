CREATE PROCEDURE [dbo].[ObtenerCotizaciones]
    @id_empresa INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.id_cotizacion,
        c.id_empresa,
        c.id_cliente,
        cl.nombre_razon_social AS nombre_cliente, 
        c.numero_cotizacion,
        c.fecha_emision,
        c.fecha_vencimiento,
        c.moneda,
        c.tipo_cambio,
        c.subtotal,
        c.impuesto,
        c.descuento,
        c.total,
        c.id_estado,
        e.nombre_estado AS estado
    FROM dbo.COTIZACION c
    INNER JOIN dbo.CLIENTE cl ON c.id_cliente = cl.id_cliente
    INNER JOIN dbo.ESTADO e ON c.id_estado = e.id_estado
    WHERE c.id_empresa = @id_empresa
    ORDER BY c.fecha_emision DESC;
END;