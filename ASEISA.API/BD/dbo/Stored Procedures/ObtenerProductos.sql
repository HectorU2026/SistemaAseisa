CREATE PROCEDURE ObtenerProductos
    @id_empresa    INT,
    @nombre        VARCHAR(150) = NULL,
    @pagina        INT = 1,
    @tamano_pagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SET @nombre = NULLIF(LTRIM(RTRIM(@nombre)), '');

    IF @pagina IS NULL OR @pagina < 1
        SET @pagina = 1;

    IF @tamano_pagina IS NULL OR @tamano_pagina < 1
        SET @tamano_pagina = 10;

    IF @tamano_pagina > 100
        SET @tamano_pagina = 100;

    SELECT COUNT(*) AS Total
    FROM PRODUCTO p
    WHERE p.id_empresa = @id_empresa
      AND (@nombre IS NULL OR CHARINDEX(@nombre, p.nombre) > 0);

    SELECT
        p.id_producto AS IdProducto,
        p.nombre AS Nombre,
        p.tipo_producto AS TipoProducto,
        p.politica_facturacion AS PoliticaFacturacion,
        p.precio_venta AS PrecioVenta,
        p.porcentaje_impuesto_venta AS PorcentajeImpuestoVenta,
        p.costo_promedio AS CostoPromedio,
        p.porcentaje_impuesto_compra AS PorcentajeImpuestoCompra,
        p.descripcion AS Descripcion,
        p.peso AS Peso,
        p.volumen AS Volumen,
        p.tiempo_entrega AS TiempoEntrega,
        p.id_cuenta_ingreso AS IdCuentaIngreso,
        (ci.codigo + ' - ' + ci.nombre) AS CuentaIngreso,
        p.id_cuenta_gasto AS IdCuentaGasto,
        (cg.codigo + ' - ' + cg.nombre) AS CuentaGasto,
        p.id_estado AS IdEstado,
        e.nombre_estado AS Estado
    FROM PRODUCTO p
    INNER JOIN ESTADO e
        ON e.id_estado = p.id_estado
    INNER JOIN CUENTA_CONTABLE ci
        ON ci.id_cuenta_contable = p.id_cuenta_ingreso
    INNER JOIN CUENTA_CONTABLE cg
        ON cg.id_cuenta_contable = p.id_cuenta_gasto
    WHERE p.id_empresa = @id_empresa
      AND (@nombre IS NULL OR CHARINDEX(@nombre, p.nombre) > 0)
    ORDER BY p.nombre, p.id_producto
    OFFSET (@pagina - 1) * @tamano_pagina ROWS
    FETCH NEXT @tamano_pagina ROWS ONLY;
END;
