CREATE   PROCEDURE dbo.ObtenerCompras
    @id_empresa INT,
    @id_usuario INT,
    @busqueda NVARCHAR(200) = NULL,
    @id_estado INT = NULL,
    @pagina INT = 1,
    @tamano_pagina INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.USUARIO u
        INNER JOIN dbo.ESTADO e ON e.id_estado = u.id_estado
        WHERE u.id_usuario = @id_usuario AND u.id_empresa = @id_empresa
          AND LTRIM(RTRIM(e.nombre_estado)) = 'Activo')
        THROW 51201, 'El usuario no esta activo o no pertenece a la empresa.', 1;

    IF @pagina IS NULL OR @pagina < 1 OR @tamano_pagina IS NULL OR @tamano_pagina NOT BETWEEN 1 AND 100
        THROW 51220, 'Pagina debe ser positiva y tamano de pagina entre 1 y 100.', 1;
    SET @busqueda = NULLIF(LTRIM(RTRIM(@busqueda)),N'');
    DECLARE @offset BIGINT = (CONVERT(BIGINT,@pagina)-1)*@tamano_pagina;
    SELECT o.id_orden_compra AS IdCompra,o.id_empresa AS IdEmpresa,
        em.nombre_comercial AS NombreComercial,em.razon_social AS NombreEmpresa,
        o.numero_orden AS NumeroOrden,o.id_proveedor AS IdProveedor,
        pr.nombre_razon_social AS Proveedor,o.id_usuario_solicitante AS IdUsuarioSolicitante,
        o.id_termino_pago AS IdTerminoPago,o.termino_pago_nombre AS TerminoPago,
        o.dias_credito AS DiasCredito,o.fecha_emision AS FechaRegistro,
        o.fecha_requerida AS FechaRequerida,o.fecha_creacion AS FechaCreacion,
        o.fecha_modificacion AS FechaModificacion,o.subtotal AS Subtotal,
        o.porcentaje_descuento AS PorcentajeDescuento,o.monto_descuento AS MontoDescuento,
        o.impuesto AS IVA,o.total AS Total,o.confirmacion_llegada AS ConfirmacionLlegada,
        o.id_estado AS IdEstado,e.nombre_estado AS Estado INTO #filtradas
    FROM dbo.ORDEN_COMPRA o
    INNER JOIN dbo.EMPRESA em ON em.id_empresa = o.id_empresa
    INNER JOIN dbo.PROVEEDOR pr ON pr.id_proveedor = o.id_proveedor AND pr.id_empresa = o.id_empresa
    INNER JOIN dbo.ESTADO e ON e.id_estado = o.id_estado
    WHERE o.id_empresa = @id_empresa
      AND (@busqueda IS NULL OR CHARINDEX(@busqueda,pr.nombre_razon_social) > 0)
      AND (@id_estado IS NULL OR o.id_estado = @id_estado);
    INSERT dbo.BITACORA(id_usuario,id_empresa,tabla_afectada,registro_id,accion,valor_nuevo)
    VALUES (@id_usuario,@id_empresa,'ORDEN_COMPRA',NULL,'CONSULTAR',
            (SELECT @busqueda AS busqueda,@id_estado AS idEstado,
                    @pagina AS pagina,@tamano_pagina AS tamanoPagina FOR JSON PATH,WITHOUT_ARRAY_WRAPPER));
    -- QueryMultipleAsync: pagina con productos JSON y luego metadatos (siempre una fila).
    SELECT f.*,JSON_QUERY((SELECT dc.id_producto AS IdProducto,p.codigo AS Codigo,p.nombre AS Producto,
        dc.cantidad AS Cantidad,dc.precio_unitario AS PrecioUnitario,dc.subtotal AS Subtotal,
        dc.porcentaje_iva AS PorcentajeIVA,dc.monto_descuento AS MontoDescuento,
        dc.impuesto AS IVA,dc.total AS Total
        FROM dbo.DETALLE_ORDEN_COMPRA dc
        INNER JOIN dbo.PRODUCTO p ON p.id_producto = dc.id_producto AND p.id_empresa = @id_empresa
        WHERE dc.id_orden_compra = f.IdCompra
        ORDER BY dc.id_producto FOR JSON PATH)) AS ProductosJson
    FROM #filtradas f
    ORDER BY f.FechaRegistro DESC,f.IdCompra DESC
    OFFSET @offset ROWS FETCH NEXT @tamano_pagina ROWS ONLY;
    SELECT COUNT_BIG(*) AS TotalRegistros,@pagina AS Pagina,@tamano_pagina AS TamanoPagina,
           (COUNT_BIG(*)+@tamano_pagina-1)/@tamano_pagina AS TotalPaginas
    FROM #filtradas;
END;