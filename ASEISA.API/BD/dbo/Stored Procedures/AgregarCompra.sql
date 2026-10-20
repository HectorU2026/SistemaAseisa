-- Ejecutar despues de 01_Compras_Estructura.sql. Requiere SQL Server 2016 SP1+
-- y compatibilidad 130+ por OPENJSON. Los lotes de instalacion son independientes.
CREATE   PROCEDURE dbo.AgregarCompra
    @id_empresa INT,
    @id_usuario INT,
    @id_proveedor INT,
    @productos NVARCHAR(MAX),
    @id_termino_pago INT = NULL,
    @porcentaje_descuento DECIMAL(5,2) = 0,
    @confirmacion_llegada BIT = 0,
    @id_estado INT = NULL,
    @fecha_requerida DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @@TRANCOUNT <> 0
        THROW 51200, 'Ejecute este SP sin una transaccion externa.', 1;
    IF @porcentaje_descuento IS NULL OR @porcentaje_descuento NOT BETWEEN 0 AND 100
        THROW 51202, 'El descuento debe ser un porcentaje entre 0 y 100.', 1;
    IF @confirmacion_llegada IS NULL
        THROW 51203, 'Indique confirmacion de llegada.', 1;
    -- JSON: [{"idProducto":1,"cantidad":2,"precioUnitario":1000.00}]
    IF @productos IS NULL OR ISJSON(@productos) <> 1
        THROW 51204, 'Ingrese una lista JSON valida de productos.', 1;
    IF LEFT(LTRIM(REPLACE(REPLACE(REPLACE(@productos,CHAR(9),' '),CHAR(10),' '),CHAR(13),' ')),1) <> '['
        THROW 51204, 'Productos debe ser un arreglo JSON.', 1;
    IF (SELECT COUNT(*) FROM OPENJSON(@productos)) NOT BETWEEN 1 AND 500
        THROW 51205, 'La compra requiere entre 1 y 500 productos.', 1;
    IF EXISTS (SELECT 1 FROM OPENJSON(@productos) WHERE type <> 5)
        THROW 51204, 'Cada producto debe ser un objeto JSON.', 1;
    -- Exigir numeros JSON, sin propiedades repetidas ni conversiones desde textos.
    IF EXISTS (
        SELECT 1 FROM OPENJSON(@productos) j
        WHERE (SELECT COUNT(*) FROM OPENJSON(j.value)
               WHERE [key] IN ('idProducto','cantidad','precioUnitario')) <> 3
           OR (SELECT COUNT(DISTINCT [key]) FROM OPENJSON(j.value)
               WHERE [key] IN ('idProducto','cantidad','precioUnitario') AND type = 2) <> 3)
        THROW 51206, 'Cada linea requiere idProducto, cantidad y precioUnitario numericos.', 1;

    DECLARE @entrada TABLE(id_producto INT, cantidad INT, precio_unitario DECIMAL(18,2));
    IF EXISTS (
        SELECT 1 FROM OPENJSON(@productos) j
        WHERE TRY_CONVERT(INT, JSON_VALUE(j.value,'$.idProducto')) IS NULL
           OR TRY_CONVERT(INT, JSON_VALUE(j.value,'$.idProducto')) <= 0
           OR TRY_CONVERT(INT, JSON_VALUE(j.value,'$.cantidad')) IS NULL
           OR TRY_CONVERT(INT, JSON_VALUE(j.value,'$.cantidad')) <= 0
           OR TRY_CONVERT(DECIMAL(18,2),JSON_VALUE(j.value,'$.precioUnitario')) IS NULL
           OR TRY_CONVERT(DECIMAL(28,10),JSON_VALUE(j.value,'$.precioUnitario')) IS NULL
           OR TRY_CONVERT(DECIMAL(28,10),JSON_VALUE(j.value,'$.precioUnitario')) < 0
           OR TRY_CONVERT(DECIMAL(28,10),JSON_VALUE(j.value,'$.precioUnitario')) <>
              TRY_CONVERT(DECIMAL(18,2),JSON_VALUE(j.value,'$.precioUnitario')))
        THROW 51207, 'Use IDs positivos, cantidades enteras positivas y precios no negativos con hasta dos decimales.', 1;
    INSERT @entrada
    SELECT CONVERT(INT,JSON_VALUE(value,'$.idProducto')),
           CONVERT(INT,JSON_VALUE(value,'$.cantidad')),
           CONVERT(DECIMAL(18,2),JSON_VALUE(value,'$.precioUnitario'))
    FROM OPENJSON(@productos);
    IF EXISTS (SELECT id_producto FROM @entrada GROUP BY id_producto HAVING COUNT(*) > 1)
        THROW 51208, 'No repita un producto; consolide su cantidad en una linea.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.USUARIO u
        INNER JOIN dbo.ESTADO e ON e.id_estado = u.id_estado
        WHERE u.id_usuario = @id_usuario AND u.id_empresa = @id_empresa
          AND LTRIM(RTRIM(e.nombre_estado)) = 'Activo')
        THROW 51201, 'El usuario no esta activo o no pertenece a la empresa.', 1;
        DECLARE @id_compra INT, @anterior VARCHAR(MAX) = NULL;
        IF @id_estado IS NULL
        BEGIN
            IF (SELECT COUNT(*) FROM dbo.ESTADO WITH (HOLDLOCK)
                WHERE LTRIM(RTRIM(nombre_estado)) = 'Solicitud de cotizacion') <> 1
                THROW 51212, 'Debe existir un unico estado Solicitud de cotizacion.', 1;
            SELECT @id_estado = id_estado FROM dbo.ESTADO
            WHERE LTRIM(RTRIM(nombre_estado)) = 'Solicitud de cotizacion';
        END;
        IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO WITH (HOLDLOCK)
                       WHERE id_estado = @id_estado AND LTRIM(RTRIM(nombre_estado)) IN
                       ('Solicitud de cotizacion','Orden de compra','Solicitud enviada'))
            THROW 51213, 'Seleccione un estado de compra valido. Para cancelar use AnularCompra.', 1;

        DECLARE @id_contacto INT;
        IF NOT EXISTS (SELECT 1 FROM dbo.PROVEEDOR pr WITH (HOLDLOCK)
                       INNER JOIN dbo.ESTADO e ON e.id_estado = pr.id_estado
                       WHERE pr.id_proveedor = @id_proveedor AND pr.id_empresa = @id_empresa
                         AND LTRIM(RTRIM(e.nombre_estado)) = 'Activo')
            THROW 51214, 'Seleccione un proveedor activo de la empresa.', 1;
        SELECT @id_contacto = id_contacto FROM dbo.PROVEEDOR
        WHERE id_proveedor = @id_proveedor AND id_empresa = @id_empresa;
        IF @id_contacto IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.CONTACTO c WITH (HOLDLOCK)
            INNER JOIN dbo.ESTADO e ON e.id_estado = c.id_estado
            WHERE c.id_contacto = @id_contacto AND c.id_empresa = @id_empresa
              AND c.rol_contacto IN ('Proveedor','Ambos')
              AND LTRIM(RTRIM(e.nombre_estado)) = 'Activo')
            THROW 51214, 'El contacto del proveedor no esta habilitado para compras.', 1;

        DECLARE @termino_nombre NVARCHAR(100), @dias INT;
        IF @id_termino_pago IS NULL
            SELECT @id_termino_pago = id_termino_pago_compras
            FROM dbo.CONTACTO WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa;
        SELECT @termino_nombre = nombre, @dias = dias_credito
        FROM dbo.TERMINO_PAGO WITH (HOLDLOCK)
        WHERE id_termino_pago = @id_termino_pago AND id_empresa = @id_empresa AND activo = 1;
        IF @termino_nombre IS NULL
            THROW 51215, 'Seleccione un termino de pago activo de la empresa o configure el predeterminado del proveedor.', 1;
        IF EXISTS (SELECT 1 FROM @entrada x WHERE NOT EXISTS (
            SELECT 1 FROM dbo.PRODUCTO p WITH (HOLDLOCK)
            INNER JOIN dbo.ESTADO e ON e.id_estado = p.id_estado
            WHERE p.id_producto = x.id_producto AND p.id_empresa = @id_empresa
              AND LTRIM(RTRIM(e.nombre_estado)) = 'Activo'))
            THROW 51216, 'Todos los productos deben estar activos y pertenecer a la empresa.', 1;

        DECLARE @lineas TABLE (
            id_producto INT PRIMARY KEY, cantidad INT, precio_unitario DECIMAL(18,2),
            subtotal DECIMAL(18,2), porcentaje_iva DECIMAL(8,4),
            descuento DECIMAL(18,2), impuesto DECIMAL(18,2), total DECIMAL(18,2),
            fraccion DECIMAL(10,4));
        INSERT @lineas(id_producto,cantidad,precio_unitario,subtotal,porcentaje_iva)
        SELECT x.id_producto,x.cantidad,x.precio_unitario,
               CONVERT(DECIMAL(18,2),CONVERT(DECIMAL(10,0),x.cantidad)*x.precio_unitario),
               p.porcentaje_impuesto_compra
        FROM @entrada x INNER JOIN dbo.PRODUCTO p WITH (HOLDLOCK)
            ON p.id_producto = x.id_producto AND p.id_empresa = @id_empresa
        ;
        IF EXISTS (SELECT 1 FROM @lineas WHERE porcentaje_iva IS NULL OR porcentaje_iva < 0)
            THROW 51217, 'El producto requiere un porcentaje de IVA de compra no negativo.', 1;

        DECLARE @subtotal DECIMAL(18,2), @descuento DECIMAL(18,2),
                @impuesto DECIMAL(18,2), @total DECIMAL(18,2), @centavos INT;
        SELECT @subtotal = TRY_CONVERT(DECIMAL(18,2),SUM(subtotal)) FROM @lineas;
        IF @subtotal IS NULL THROW 51218, 'El subtotal excede el limite permitido.', 1;
        SET @descuento = CONVERT(DECIMAL(18,2),ROUND(@subtotal*@porcentaje_descuento/100,2));
        -- En centavos, el descuento exacto de cada linea es subtotal * porcentaje.
        -- Distribucion por mayor resto, con desempate por ID de producto.
        UPDATE @lineas SET
            descuento = CONVERT(DECIMAL(18,2),CONVERT(DECIMAL(24,0),FLOOR(subtotal*@porcentaje_descuento))/100.0),
            fraccion = subtotal*@porcentaje_descuento-CONVERT(DECIMAL(24,0),FLOOR(subtotal*@porcentaje_descuento));
        SELECT @centavos = CONVERT(INT,(@descuento-CONVERT(DECIMAL(18,2),SUM(descuento)))*100) FROM @lineas;
        ;WITH reparto AS (
            SELECT id_producto,ROW_NUMBER() OVER(ORDER BY fraccion DESC,id_producto) AS posicion
            FROM @lineas)
        UPDATE l SET descuento = l.descuento+0.01
        FROM @lineas l INNER JOIN reparto r ON r.id_producto = l.id_producto
        WHERE r.posicion <= @centavos;
        UPDATE @lineas SET impuesto =
            CONVERT(DECIMAL(18,2),ROUND((subtotal-descuento)*porcentaje_iva/100,2));
        UPDATE @lineas SET total = subtotal-descuento+impuesto;
        SELECT @impuesto = TRY_CONVERT(DECIMAL(18,2),SUM(impuesto)),
               @total = TRY_CONVERT(DECIMAL(18,2),SUM(total)) FROM @lineas;
        IF @impuesto IS NULL OR @total IS NULL
            THROW 51218, 'Los importes exceden el limite permitido.', 1;
        INSERT dbo.ORDEN_COMPRA
            (id_empresa,id_proveedor,id_usuario_solicitante,numero_orden,fecha_emision,
             fecha_requerida,subtotal,impuesto,total,id_estado,id_termino_pago,
             termino_pago_nombre,dias_credito,porcentaje_descuento,monto_descuento,
             confirmacion_llegada,fecha_creacion)
        VALUES (@id_empresa,@id_proveedor,@id_usuario,'PENDIENTE',SYSDATETIME(),
                @fecha_requerida,@subtotal,@impuesto,@total,@id_estado,@id_termino_pago,
                @termino_nombre,@dias,@porcentaje_descuento,@descuento,
                @confirmacion_llegada,SYSDATETIME());
        SET @id_compra = CONVERT(INT,SCOPE_IDENTITY());
        UPDATE dbo.ORDEN_COMPRA
        SET numero_orden = CONCAT('OC-',RIGHT(CONCAT('0000000000',@id_compra),10))
        WHERE id_orden_compra = @id_compra;
        INSERT dbo.DETALLE_ORDEN_COMPRA
            (id_orden_compra,id_producto,cantidad,precio_unitario,subtotal,
             porcentaje_iva,monto_descuento,impuesto,total)
        SELECT @id_compra,id_producto,cantidad,precio_unitario,subtotal,
               porcentaje_iva,descuento,impuesto,total FROM @lineas;
        DECLARE @nuevo VARCHAR(MAX) = (SELECT o.*,
            JSON_QUERY((SELECT dc.* FROM dbo.DETALLE_ORDEN_COMPRA dc
                        WHERE dc.id_orden_compra = o.id_orden_compra
                        ORDER BY dc.id_producto FOR JSON PATH)) AS detalle
         FROM dbo.ORDEN_COMPRA o WHERE o.id_orden_compra = @id_compra
           AND o.id_empresa = @id_empresa FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        INSERT dbo.BITACORA(id_usuario,id_empresa,tabla_afectada,registro_id,
                            accion,valor_anterior,valor_nuevo)
        VALUES (@id_usuario,@id_empresa,'ORDEN_COMPRA',CONVERT(VARCHAR(100),@id_compra),
                'AGREGAR',@anterior,@nuevo);
        COMMIT TRANSACTION;
        SELECT id_orden_compra AS IdCompra,numero_orden AS NumeroOrden,
               subtotal AS Subtotal,porcentaje_descuento AS PorcentajeDescuento,
               monto_descuento AS MontoDescuento,impuesto AS IVA,total AS Total
        FROM dbo.ORDEN_COMPRA WHERE id_orden_compra = @id_compra AND id_empresa = @id_empresa;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;