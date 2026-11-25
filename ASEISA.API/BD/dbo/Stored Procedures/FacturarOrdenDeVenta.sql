
CREATE   PROCEDURE [dbo].[FacturarOrdenDeVenta]
    @id_cotizacion   INT,
    @id_empresa      INT,
    @id_cliente      INT,
    @id_usuario      INT,
    @numero_venta    VARCHAR(50),
    @numero_factura  VARCHAR(50),
    @tipo_factura    VARCHAR(50),
    @condicion_venta VARCHAR(50),
    @moneda          VARCHAR(10),
    @tipo_cambio     DECIMAL(18,6),
    @subtotal        DECIMAL(18,2),
    @impuesto        DECIMAL(18,2),
    @descuento       DECIMAL(18,2),
    @total           DECIMAL(18,2),
    @fecha_factura   DATETIME2,
    @id_termino_pago INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @id_estado_orden_venta INT;
    DECLARE @id_estado_facturado INT;
    DECLARE @id_estado_emitida INT;
    DECLARE @id_estado_borrador INT;
    DECLARE @id_estado_factura_borrador INT;
    DECLARE @id_estado_activo INT;
    DECLARE @id_venta INT;
    DECLARE @id_factura INT;
    DECLARE @id_asiento INT;
    DECLARE @applock INT;
    DECLARE @n INT;
    DECLARE @numero_generado VARCHAR(50);
    DECLARE @numero_asiento VARCHAR(50);
    DECLARE @recurso_applock NVARCHAR(255);
    DECLARE @id_cuenta_cxc INT;
    DECLARE @id_cuenta_impuesto_ventas INT;
    DECLARE @id_cuenta_ingreso_defecto INT;
    DECLARE @termino_pago_nombre NVARCHAR(100);
    DECLARE @dias_credito INT;
    DECLARE @fecha_vencimiento DATETIME2(7);
    DECLARE @es_contado BIT;
    DECLARE @es_credito BIT;
    DECLARE @condicion_limpia VARCHAR(50);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Limpiamos la condición para evitar problemas con espacios o mayúsculas
        SET @condicion_limpia = LOWER(LTRIM(RTRIM(ISNULL(@condicion_venta, '')))) COLLATE Latin1_General_CI_AI;

        -- Determinamos si es Contado o Crédito
        IF @condicion_limpia IN ('contado', '01')
            SET @es_contado = 1;
        ELSE IF @condicion_limpia IN ('credito', 'crédito', '02')
            SET @es_credito = 1;
        ELSE
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS IdFactura, NULL AS NumeroFactura, 'Condición de venta inválida. Debe ser "Contado" o "Crédito".' AS Mensaje;
            RETURN;
        END;

        /* ---------- Lógica del Término de Pago ---------- */
        IF @es_contado = 1
        BEGIN
            SET @fecha_vencimiento = @fecha_factura;
            
            -- Si es contado y mandan un id, validamos que sea de 0 días. Si no mandan, buscamos el de 0 días.
            IF @id_termino_pago IS NOT NULL AND @id_termino_pago > 0
            BEGIN
                SELECT @termino_pago_nombre = nombre FROM dbo.TERMINO_PAGO WHERE id_termino_pago = @id_termino_pago;
            END
            ELSE
            BEGIN
                SELECT TOP 1 @id_termino_pago = id_termino_pago, @termino_pago_nombre = nombre
                FROM dbo.TERMINO_PAGO 
                WHERE id_empresa = @id_empresa AND dias_credito = 0 AND activo = 1
                ORDER BY id_termino_pago;
            END
        END
        ELSE IF @es_credito = 1
        BEGIN
            -- Para crédito, el id del término es obligatorio o al menos buscamos uno con días > 0
            IF @id_termino_pago IS NOT NULL AND @id_termino_pago > 0
            BEGIN
                SELECT @termino_pago_nombre = nombre, @dias_credito = dias_credito
                FROM dbo.TERMINO_PAGO 
                WHERE id_termino_pago = @id_termino_pago AND id_empresa = @id_empresa;
            END
            ELSE
            BEGIN
                SELECT TOP 1 @id_termino_pago = id_termino_pago, @termino_pago_nombre = nombre, @dias_credito = dias_credito
                FROM dbo.TERMINO_PAGO 
                WHERE id_empresa = @id_empresa AND dias_credito > 0 AND activo = 1
                ORDER BY id_termino_pago;
            END

            SET @fecha_vencimiento = DATEADD(DAY, ISNULL(@dias_credito, 0), @fecha_factura);
        END;

        /* ---------- Estados por nombre ---------- */
        SELECT @id_estado_orden_venta = id_estado_cotizacion FROM ESTADO_COTIZACION WHERE nombre_estado = 'Orden de Venta';
        SELECT @id_estado_facturado     = id_estado_cotizacion FROM ESTADO_COTIZACION WHERE nombre_estado = 'Facturado';
        SELECT @id_estado_emitida       = id_estado FROM ESTADO WHERE nombre_estado = 'Emitida';
        SELECT @id_estado_borrador      = id_estado FROM ESTADO WHERE nombre_estado = 'Borrador';
        SELECT @id_estado_activo        = id_estado FROM ESTADO WHERE nombre_estado = 'Activo';
        SELECT @id_estado_factura_borrador = id_estado_factura FROM ESTADO_FACTURA WHERE nombre_estado = 'Borrador';

        /* ---------- Validaciones esenciales ---------- */
        
        -- 1. Cotización válida
        IF NOT EXISTS (SELECT 1 FROM COTIZACION WITH (UPDLOCK, HOLDLOCK) WHERE id_cotizacion = @id_cotizacion AND id_empresa = @id_empresa AND id_estado_cotizacion = @id_estado_orden_venta)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS IdFactura, NULL AS NumeroFactura, 'La cotización no está en estado Orden de Venta' AS Mensaje;
            RETURN;
        END;

        -- 2. Evitar doble facturación
        IF EXISTS (SELECT 1 FROM VENTAS WITH (HOLDLOCK) WHERE id_cotizacion = @id_cotizacion)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS IdFactura, NULL AS NumeroFactura, 'La orden de venta ya fue facturada' AS Mensaje;
            RETURN;
        END;

        -- 3. Obtener cuentas contables de la empresa (Si están en NULL, la factura se crea igual, sin asiento)
        SELECT @id_cuenta_cxc = id_cuenta_contable FROM dbo.CONFIGURACION_CONTABLE WHERE id_empresa = @id_empresa AND clave = 'CUENTA_CXC';
        SELECT @id_cuenta_impuesto_ventas = id_cuenta_contable FROM dbo.CONFIGURACION_CONTABLE WHERE id_empresa = @id_empresa AND clave = 'CUENTA_IVA_VENTAS';
        SELECT @id_cuenta_ingreso_defecto = id_cuenta_contable FROM dbo.CONFIGURACION_CONTABLE WHERE id_empresa = @id_empresa AND clave = 'CUENTA_INGRESO_DEFECTO';

        /* ---------- Consecutivo de factura (Lock) ---------- */
        SET @recurso_applock = N'FAC-NUMERO-' + CONVERT(NVARCHAR(11), @id_empresa);
        EXEC @applock = [sys].[sp_getapplock] @Resource = @recurso_applock, @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 8000;

        IF @applock < 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS IdFactura, NULL AS NumeroFactura, 'No se pudo obtener el consecutivo de factura' AS Mensaje;
            RETURN;
        END;

        SELECT @n = ISNULL(MAX(TRY_CONVERT(INT, SUBSTRING(numero_factura, 5, 6))), 0)
        FROM FACTURA WITH (HOLDLOCK)
        WHERE id_empresa = @id_empresa AND numero_factura LIKE 'FAC-[0-9][0-9][0-9][0-9][0-9][0-9]';

        SET @numero_generado = CONCAT('FAC-', RIGHT(CONCAT('000000', @n + 1), 6));

        /* ---------- Inserts Transaccionales ---------- */
        -- Venta
        INSERT INTO VENTAS (id_empresa, id_cliente, id_cotizacion, id_usuario, numero_comprobante, tipo_comprobante, fecha_emision, condicion_venta, moneda, tipo_cambio, subtotal, impuesto, descuento, total, id_estado)
        VALUES (@id_empresa, @id_cliente, @id_cotizacion, @id_usuario, @numero_venta, 'Factura', SYSDATETIME(), @condicion_venta, @moneda, @tipo_cambio, @subtotal, @impuesto, @descuento, @total, @id_estado_emitida);
        SET @id_venta = CAST(SCOPE_IDENTITY() AS INT);

        -- Detalle Venta
        INSERT INTO DETALLE_VENTA (id_venta, id_producto, cantidad, precio_unitario, porcentaje_descuento, porcentaje_impuesto, subtotal)
        SELECT @id_venta, id_producto, cantidad, precio_unitario, porcentaje_descuento, porcentaje_impuesto, subtotal FROM DETALLE_COTIZACION WHERE id_cotizacion = @id_cotizacion;

        -- Factura
        INSERT INTO FACTURA (id_venta, id_empresa, id_usuario, id_estado_factura, numero_factura, fecha_emision, tipo_factura, moneda, tipo_cambio, subtotal, impuesto, descuento, total, saldo_pendiente, fecha_vencimiento, id_termino_pago, termino_pago_nombre)
        VALUES (@id_venta, @id_empresa, @id_usuario, @id_estado_factura_borrador, @numero_generado, @fecha_factura, @tipo_factura, @moneda, @tipo_cambio, @subtotal, @impuesto, @descuento, @total, @total, @fecha_vencimiento, @id_termino_pago, @termino_pago_nombre);
        SET @id_factura = CAST(SCOPE_IDENTITY() AS INT);

        -- Detalle Factura
        INSERT INTO DETALLE_FACTURA (id_factura, id_producto, cantidad, precio_unitario, porcentaje_descuento, porcentaje_impuesto, subtotal)
        SELECT @id_factura, id_producto, cantidad, precio_unitario, porcentaje_descuento, porcentaje_impuesto, subtotal FROM DETALLE_COTIZACION WHERE id_cotizacion = @id_cotizacion;

        /* ---------- Asiento contable (Solo si la empresa tiene CxC configurada) ---------- */
        IF @id_cuenta_cxc IS NOT NULL
        BEGIN
            SET @numero_asiento = CONCAT('AST-FAC-', CONVERT(VARCHAR(11), @id_factura));
            
            INSERT INTO ASIENTO_CONTABLE (id_empresa, numero_asiento, fecha_asiento, descripcion, tipo_origen, total_debe, total_haber, id_estado, id_usuario)
            VALUES (@id_empresa, @numero_asiento, CAST(SYSDATETIME() AS DATE), CONCAT('Factura ', @numero_generado), 'Factura', @total, @total, @id_estado_borrador, @id_usuario);
            
            SET @id_asiento = CAST(SCOPE_IDENTITY() AS INT);

            -- Cargo a CxC
            INSERT INTO DETALLE_ASIENTO (id_asiento, id_cuenta_contable, id_distribucion_analitica, descripcion, debe, haber)
            VALUES (@id_asiento, @id_cuenta_cxc, NULL, CONCAT('CXC factura ', @numero_generado), @total, 0);

            -- Abono a Cuentas de Ingreso
            INSERT INTO DETALLE_ASIENTO (id_asiento, id_cuenta_contable, id_distribucion_analitica, descripcion, debe, haber)
            SELECT @id_asiento, COALESCE(p.id_cuenta_ingreso, @id_cuenta_ingreso_defecto), NULL, CONCAT('Ingreso factura ', @numero_generado), 0, SUM(df.subtotal)
            FROM DETALLE_FACTURA df
            INNER JOIN PRODUCTO p ON p.id_producto = df.id_producto
            WHERE df.id_factura = @id_factura AND COALESCE(p.id_cuenta_ingreso, @id_cuenta_ingreso_defecto) IS NOT NULL
            GROUP BY COALESCE(p.id_cuenta_ingreso, @id_cuenta_ingreso_defecto)
            HAVING SUM(df.subtotal) <> 0;

            -- Abono a IVA
            IF @impuesto > 0 AND @id_cuenta_impuesto_ventas IS NOT NULL
            BEGIN
                INSERT INTO DETALLE_ASIENTO (id_asiento, id_cuenta_contable, id_distribucion_analitica, descripcion, debe, haber)
                VALUES (@id_asiento, @id_cuenta_impuesto_ventas, NULL, CONCAT('Impuesto factura ', @numero_generado), 0, @impuesto);
            END;
        END;

        /* ---------- Actualizar estado de la cotización ---------- */
        UPDATE COTIZACION SET id_estado_cotizacion = @id_estado_facturado WHERE id_cotizacion = @id_cotizacion;

        COMMIT TRANSACTION;

        SELECT @id_factura AS IdFactura, @numero_generado AS NumeroFactura, 'Facturación exitosa' AS Mensaje;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;