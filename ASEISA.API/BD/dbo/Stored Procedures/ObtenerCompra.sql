CREATE   PROCEDURE dbo.ObtenerCompra
    @id_empresa INT,
    @id_usuario INT,
    @id_compra INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.USUARIO u
        INNER JOIN dbo.ESTADO e ON e.id_estado = u.id_estado
        WHERE u.id_usuario = @id_usuario AND u.id_empresa = @id_empresa
          AND LTRIM(RTRIM(e.nombre_estado)) = 'Activo')
        THROW 51201, 'El usuario no esta activo o no pertenece a la empresa.', 1;

    -- Materializar ambos resultados antes de devolverlos.
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
        o.id_estado AS IdEstado,e.nombre_estado AS Estado INTO #cabecera
    FROM dbo.ORDEN_COMPRA o
    INNER JOIN dbo.EMPRESA em ON em.id_empresa = o.id_empresa
    INNER JOIN dbo.PROVEEDOR pr ON pr.id_proveedor = o.id_proveedor AND pr.id_empresa = o.id_empresa
    INNER JOIN dbo.ESTADO e ON e.id_estado = o.id_estado
    WHERE o.id_empresa = @id_empresa AND o.id_orden_compra = @id_compra;
    SELECT dc.id_producto AS IdProducto,p.codigo AS Codigo,p.nombre AS Producto,
        dc.cantidad AS Cantidad,dc.precio_unitario AS PrecioUnitario,dc.subtotal AS Subtotal,
        dc.porcentaje_iva AS PorcentajeIVA,dc.monto_descuento AS MontoDescuento,
        dc.impuesto AS IVA,dc.total AS Total INTO #detalle
    FROM dbo.DETALLE_ORDEN_COMPRA dc
    INNER JOIN dbo.PRODUCTO p ON p.id_producto = dc.id_producto AND p.id_empresa = @id_empresa
    WHERE dc.id_orden_compra = @id_compra AND EXISTS (SELECT 1 FROM #cabecera);
    IF EXISTS (SELECT 1 FROM #cabecera)
        INSERT dbo.BITACORA(id_usuario,id_empresa,tabla_afectada,registro_id,accion)
        VALUES (@id_usuario,@id_empresa,'ORDEN_COMPRA',CONVERT(VARCHAR(100),@id_compra),'CONSULTAR');
    -- QueryMultipleAsync: cabecera (0 o 1), luego detalle.
    SELECT * FROM #cabecera;
    SELECT * FROM #detalle ORDER BY IdProducto;
END;