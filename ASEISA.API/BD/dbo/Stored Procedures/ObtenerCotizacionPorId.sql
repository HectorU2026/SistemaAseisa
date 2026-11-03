
CREATE   PROCEDURE [dbo].[ObtenerCotizacionPorId]
    @id_cotizacion INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_estado_cotizacion INT;
    SELECT @id_estado_cotizacion = id_estado_cotizacion 
    FROM ESTADO_COTIZACION 
    WHERE nombre_estado = 'Cotización';

    SELECT
        c.id_cotizacion         AS IdCotizacion,
        c.id_empresa            AS IdEmpresa,
        c.id_cliente            AS IdCliente,
        c.numero_cotizacion     AS NumeroCotizacion,
        c.fecha_emision         AS FechaEmision,
        c.fecha_vencimiento     AS FechaVencimiento,
        c.moneda                AS Moneda,
        c.tipo_cambio           AS TipoCambio,
        c.subtotal              AS Subtotal,
        c.impuesto              AS Impuesto,
        c.descuento             AS Descuento,
        c.total                 AS Total,
        c.id_estado_cotizacion  AS IdEstado, -- Mantenemos el alias
        ec.nombre_estado        AS NombreEstado,
        -- El estado es válido si está en "Cotización"
        CAST(CASE WHEN c.id_estado_cotizacion = @id_estado_cotizacion THEN 1 ELSE 0 END AS BIT) AS EstadoValido
    FROM COTIZACION c
    INNER JOIN ESTADO_COTIZACION ec ON ec.id_estado_cotizacion = c.id_estado_cotizacion
    WHERE c.id_cotizacion = @id_cotizacion;

    SELECT
        emp.nombre_comercial AS NombreComercial,
        emp.razon_social     AS RazonSocial,
        emp.cedula_juridica  AS CedulaJuridica,
        emp.correo           AS Correo,
        emp.telefono         AS Telefono,
        emp.direccion        AS Direccion
    FROM COTIZACION c
    INNER JOIN EMPRESA emp ON emp.id_empresa = c.id_empresa
    WHERE c.id_cotizacion = @id_cotizacion;

    -- 3. Cliente
    SELECT
        cli.nombre_razon_social AS NombreRazonSocial,
        cli.identificacion      AS Identificacion,
        cli.correo              AS Correo,
        cli.telefono            AS Telefono,
        cli.direccion           AS Direccion
    FROM COTIZACION c
    INNER JOIN CLIENTE cli ON cli.id_cliente = c.id_cliente
    WHERE c.id_cotizacion = @id_cotizacion;

    SELECT
        d.id_detalle_cotizacion AS IdDetalleCotizacion,
        d.id_cotizacion         AS IdCotizacion,
        d.id_producto           AS IdProducto,
        p.codigo                AS CodigoProducto,
        p.nombre                AS NombreProducto,
        p.descripcion           AS DescripcionProducto,
        p.unidad_medida         AS UnidadMedida,
        d.cantidad              AS Cantidad,
        d.precio_unitario       AS PrecioUnitario,
        d.porcentaje_descuento  AS PorcentajeDescuento,
        d.porcentaje_impuesto   AS PorcentajeImpuesto,
        d.subtotal              AS Subtotal
    FROM DETALLE_COTIZACION d
    INNER JOIN PRODUCTO p ON p.id_producto = d.id_producto
    WHERE d.id_cotizacion = @id_cotizacion;
END;