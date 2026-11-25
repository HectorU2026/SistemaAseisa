

CREATE PROCEDURE [dbo].[ObtenerCotizaciones]
    @id_empresa INT,
    @pagina INT = 1,
    @tamano_pagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@pagina - 1) * @tamano_pagina;

    -- 1. Total de registros para el paginador del frontend
    SELECT COUNT(*) 
    FROM dbo.COTIZACION c WITH (NOLOCK)
    WHERE c.id_empresa = @id_empresa;

    -- 2. Datos exactos de la página solicitada
    SELECT 
        c.id_cotizacion,
        c.id_empresa,
        c.id_cliente,
        cli.nombre_razon_social AS nombre_cliente,
        c.numero_cotizacion,
        c.fecha_emision,
        c.fecha_vencimiento,
        c.moneda,
        c.tipo_cambio,
        c.subtotal,
        c.impuesto,
        c.descuento,
        c.total,
        c.id_estado_cotizacion,
        e.nombre_estado AS estado
    FROM dbo.COTIZACION c WITH (NOLOCK)
    INNER JOIN dbo.CLIENTE cli WITH (NOLOCK) ON c.id_cliente = cli.id_cliente
    INNER JOIN dbo.ESTADO e WITH (NOLOCK) ON c.id_estado_cotizacion = e.id_estado
    WHERE c.id_empresa = @id_empresa
    ORDER BY c.fecha_emision DESC
    OFFSET @Offset ROWS FETCH NEXT @tamano_pagina ROWS ONLY;
END;