CREATE   PROCEDURE dbo.ObtenerCatalogosCompra
    @id_empresa INT,
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.USUARIO u
        INNER JOIN dbo.ESTADO e ON e.id_estado = u.id_estado
        WHERE u.id_usuario = @id_usuario AND u.id_empresa = @id_empresa
          AND LTRIM(RTRIM(e.nombre_estado)) = 'Activo')
        THROW 51201, 'El usuario no esta activo o no pertenece a la empresa.', 1;

    INSERT dbo.BITACORA(id_usuario,id_empresa,tabla_afectada,registro_id,accion,valor_nuevo)
    VALUES (@id_usuario,@id_empresa,'EMPRESA',CONVERT(VARCHAR(100),@id_empresa),
            'CONSULTAR','{"consulta":"Catalogos de compras"}');
    -- QueryMultipleAsync: proveedores, productos, terminos, estados (en ese orden).
    SELECT pr.id_proveedor AS IdProveedor,pr.nombre_razon_social AS Nombre,
           pr.id_contacto AS IdContacto,c.id_termino_pago_compras AS IdTerminoPagoPredeterminado
    FROM dbo.PROVEEDOR pr INNER JOIN dbo.ESTADO ep ON ep.id_estado = pr.id_estado
    LEFT JOIN dbo.CONTACTO c ON c.id_contacto = pr.id_contacto AND c.id_empresa = pr.id_empresa
    LEFT JOIN dbo.ESTADO ec ON ec.id_estado = c.id_estado
    WHERE pr.id_empresa = @id_empresa AND LTRIM(RTRIM(ep.nombre_estado)) = 'Activo'
      AND (pr.id_contacto IS NULL OR
           (c.rol_contacto IN ('Proveedor','Ambos') AND LTRIM(RTRIM(ec.nombre_estado)) = 'Activo'))
    ORDER BY pr.nombre_razon_social,pr.id_proveedor;
    SELECT p.id_producto AS IdProducto,p.codigo AS Codigo,p.nombre AS Nombre,
           p.costo_promedio AS CostoReferencia,p.porcentaje_impuesto_compra AS PorcentajeIVA
    FROM dbo.PRODUCTO p INNER JOIN dbo.ESTADO e ON e.id_estado = p.id_estado
    WHERE p.id_empresa = @id_empresa AND LTRIM(RTRIM(e.nombre_estado)) = 'Activo'
    ORDER BY p.nombre,p.id_producto;
    SELECT id_termino_pago AS IdTerminoPago,nombre AS Nombre,dias_credito AS DiasCredito
    FROM dbo.TERMINO_PAGO WHERE id_empresa = @id_empresa AND activo = 1
    ORDER BY dias_credito,nombre;
    SELECT id_estado AS IdEstado,nombre_estado AS Nombre
    FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado)) IN
        ('Solicitud de cotizacion','Orden de compra','Solicitud enviada','Cancelada')
    ORDER BY id_estado;
END;