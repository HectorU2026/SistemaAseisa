CREATE   PROCEDURE [dbo].[ObtenerCotizaciones]
    @id_empresa INT,
    @busqueda VARCHAR(150) = NULL,
    @id_estado_cotizacion INT = NULL,
    @pagina INT = 1,
    @tamano_pagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@pagina - 1) * @tamano_pagina;

    SELECT COUNT(*) 
    FROM dbo.COTIZACION c WITH (NOLOCK)
    INNER JOIN dbo.CLIENTE cli WITH (NOLOCK) ON c.id_cliente = cli.id_cliente
    INNER JOIN dbo.ESTADO_COTIZACION ec WITH (NOLOCK) ON c.id_estado_cotizacion = ec.id_estado_cotizacion
    WHERE c.id_empresa = @id_empresa
      AND ec.nombre_estado NOT IN ('Cancelada', 'Facturado')
      -- Filtro por búsqueda (número de cotización o nombre del cliente)
      AND (
          @busqueda IS NULL 
          OR c.numero_cotizacion LIKE '%' + @busqueda + '%'
          OR cli.nombre_razon_social LIKE '%' + @busqueda + '%'
      )

      AND (
          @id_estado_cotizacion IS NULL 
          OR c.id_estado_cotizacion = @id_estado_cotizacion
      );

    SELECT 
        c.id_cotizacion         AS IdCotizacion,
        c.id_empresa            AS IdEmpresa,
        c.id_cliente            AS IdCliente,
        cli.nombre_razon_social  AS NombreCliente, 
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
    FROM dbo.COTIZACION c WITH (NOLOCK)
    INNER JOIN dbo.CLIENTE cli WITH (NOLOCK) ON c.id_cliente = cli.id_cliente
    INNER JOIN dbo.ESTADO_COTIZACION ec WITH (NOLOCK) ON c.id_estado_cotizacion = ec.id_estado_cotizacion
    WHERE c.id_empresa = @id_empresa
      AND ec.nombre_estado NOT IN ('Cancelada', 'Facturado')
      AND (
          @busqueda IS NULL 
          OR c.numero_cotizacion LIKE '%' + @busqueda + '%'
          OR cli.nombre_razon_social LIKE '%' + @busqueda + '%'
      )
      AND (
          @id_estado_cotizacion IS NULL 
          OR c.id_estado_cotizacion = @id_estado_cotizacion
      )
    ORDER BY c.fecha_emision DESC
    OFFSET @Offset ROWS FETCH NEXT @tamano_pagina ROWS ONLY;
END;