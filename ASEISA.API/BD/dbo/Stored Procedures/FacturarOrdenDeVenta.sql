CREATE PROCEDURE dbo.FacturarOrdenDeVenta
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
    @fecha_factura   DATETIME2
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
    DECLARE @suma_detalles DECIMAL(18,2);
    DECLARE @total_haber DECIMAL(18,2);
    DECLARE @suma_debe DECIMAL(18,2);
    DECLARE @suma_haber DECIMAL(18,2);
    DECLARE @total_factura DECIMAL(18,2);
    DECLARE @total_debe_asiento DECIMAL(18,2);
    DECLARE @total_haber_asiento DECIMAL(18,2);
    DECLARE @id_estado_empresa INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @fecha_factura IS NULL
           OR CAST(@fecha_factura AS DATE) = CONVERT(DATE, '0001-01-01')
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'La fecha de factura es obligatoria' AS Mensaje;

            RETURN;
        END;

        SELECT @id_estado_orden_venta = id_estado
        FROM ESTADO
        WHERE nombre_estado = 'Orden de Venta';

        SELECT @id_estado_facturado = id_estado
        FROM ESTADO
        WHERE nombre_estado = 'Facturado';

        SELECT @id_estado_emitida = id_estado
        FROM ESTADO
        WHERE nombre_estado = 'Emitida';

        SELECT @id_estado_borrador = id_estado
        FROM ESTADO
        WHERE nombre_estado = 'Borrador';

        SELECT @id_estado_factura_borrador = id_estado_factura
        FROM ESTADO_FACTURA
        WHERE nombre_estado = 'Borrador';

        SELECT @id_estado_activo = id_estado
        FROM ESTADO
        WHERE nombre_estado = 'Activo';

        IF @id_estado_orden_venta IS NULL
           OR @id_estado_facturado IS NULL
           OR @id_estado_emitida IS NULL
           OR @id_estado_borrador IS NULL
           OR @id_estado_factura_borrador IS NULL
           OR @id_estado_activo IS NULL
           OR (SELECT COUNT(*) FROM ESTADO WHERE nombre_estado = 'Borrador') <> 1
           OR (SELECT COUNT(*) FROM ESTADO WHERE nombre_estado = 'Activo') <> 1
           OR (SELECT COUNT(*) FROM ESTADO_FACTURA WHERE nombre_estado = 'Borrador') <> 1
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'No se encontraron los estados de facturación' AS Mensaje;

            RETURN;
        END;

        IF @subtotal < 0
           OR @impuesto < 0
           OR @descuento < 0
           OR @total < 0
           OR @total <> (@subtotal - @descuento + @impuesto)
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'Los importes de la factura no son coherentes' AS Mensaje;

            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM DETALLE_COTIZACION
            WHERE id_cotizacion = @id_cotizacion
              AND (subtotal < 0 OR cantidad < 0 OR precio_unitario < 0)
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'No se permiten importes negativos' AS Mensaje;

            RETURN;
        END;

        SELECT @suma_detalles = ISNULL(SUM(subtotal), 0)
        FROM DETALLE_COTIZACION
        WHERE id_cotizacion = @id_cotizacion;

        IF @suma_detalles <> (@subtotal - @descuento)
           OR (@suma_detalles + @impuesto) <> @total
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'Los importes de la factura no son coherentes' AS Mensaje;

            RETURN;
        END;

        -- FAC-001: Validar usuario activo y pertenencia a la empresa
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.USUARIO
            WHERE id_usuario = @id_usuario
              AND id_empresa = @id_empresa
              AND id_estado = @id_estado_activo
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'Acceso denegado: usuario inactivo o de otra empresa' AS Mensaje;

            RETURN;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM COTIZACION WITH (UPDLOCK, HOLDLOCK)
            WHERE id_cotizacion = @id_cotizacion
              AND id_empresa = @id_empresa
              AND id_estado = @id_estado_orden_venta
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'La cotización no está en estado Orden de Venta' AS Mensaje;

            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM VENTAS WITH (HOLDLOCK)
            WHERE id_cotizacion = @id_cotizacion
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'La orden de venta ya fue facturada' AS Mensaje;

            RETURN;
        END;

        IF EXISTS
        (
            SELECT d.id_producto
            FROM DETALLE_COTIZACION d
            WHERE d.id_cotizacion = @id_cotizacion
            GROUP BY d.id_producto
            HAVING COUNT(*) > 1
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'La cotización tiene productos duplicados' AS Mensaje;

            RETURN;
        END;

        SELECT
            @id_cuenta_cxc = id_cuenta_cxc,
            @id_cuenta_impuesto_ventas = id_cuenta_impuesto_ventas,
            @id_estado_empresa = id_estado
        FROM EMPRESA WITH (UPDLOCK, HOLDLOCK)
        WHERE id_empresa = @id_empresa;

        IF @id_estado_empresa IS NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'La empresa no existe' AS Mensaje;

            RETURN;
        END;

        IF @id_estado_empresa <> @id_estado_activo
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'La empresa no está activa' AS Mensaje;

            RETURN;
        END;

        IF @id_cuenta_cxc IS NULL
           OR (@impuesto > 0 AND @id_cuenta_impuesto_ventas IS NULL)
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'La empresa no tiene configuradas las cuentas contables de facturación' AS Mensaje;

            RETURN;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM CUENTA_CONTABLE WITH (HOLDLOCK)
            WHERE id_cuenta_contable = @id_cuenta_cxc
              AND id_empresa = @id_empresa
              AND permite_movimientos = 1
              AND id_estado = @id_estado_activo
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'La cuenta de CXC no es válida para la empresa' AS Mensaje;

            RETURN;
        END;

        IF @impuesto > 0
           AND NOT EXISTS
        (
            SELECT 1
            FROM CUENTA_CONTABLE WITH (HOLDLOCK)
            WHERE id_cuenta_contable = @id_cuenta_impuesto_ventas
              AND id_empresa = @id_empresa
              AND permite_movimientos = 1
              AND id_estado = @id_estado_activo
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'La cuenta de impuestos sobre ventas no es válida para la empresa' AS Mensaje;

            RETURN;
        END;

        IF @impuesto > 0
           AND @id_cuenta_cxc = @id_cuenta_impuesto_ventas
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'Las cuentas contables de la factura no pueden repetirse' AS Mensaje;

            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM DETALLE_COTIZACION d
            LEFT JOIN PRODUCTO p
                ON p.id_producto = d.id_producto
            LEFT JOIN CUENTA_CONTABLE c
                ON c.id_cuenta_contable = p.id_cuenta_ingreso
            WHERE d.id_cotizacion = @id_cotizacion
              AND
              (
                  p.id_producto IS NULL
                  OR p.id_empresa <> @id_empresa
                  OR c.id_cuenta_contable IS NULL
                  OR c.id_empresa <> @id_empresa
                  OR c.permite_movimientos <> 1
                  OR c.id_estado <> @id_estado_activo
              )
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'Las cuentas de ingreso no son válidas' AS Mensaje;

            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM DETALLE_COTIZACION d
            INNER JOIN PRODUCTO p
                ON p.id_producto = d.id_producto
            WHERE d.id_cotizacion = @id_cotizacion
              AND
              (
                  p.id_cuenta_ingreso = @id_cuenta_cxc
                  OR
                  (
                      @impuesto > 0
                      AND p.id_cuenta_ingreso = @id_cuenta_impuesto_ventas
                  )
              )
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'Las cuentas contables de la factura no pueden repetirse' AS Mensaje;

            RETURN;
        END;

        SET @recurso_applock =
            N'FAC-NUMERO-' + CONVERT(NVARCHAR(11), @id_empresa);

        EXEC @applock = [sys].[sp_getapplock]
            @Resource = @recurso_applock,
            @LockMode = N'Exclusive',
            @LockOwner = N'Transaction',
            @LockTimeout = 8000;

        IF @applock < 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'No se pudo obtener el consecutivo de factura' AS Mensaje;

            RETURN;
        END;

        SELECT
            @n = ISNULL(
                MAX(
                    TRY_CONVERT(
                        INT,
                        SUBSTRING(numero_factura, 5, 6)
                    )
                ),
                0
            )
        FROM FACTURA WITH (HOLDLOCK)
        WHERE id_empresa = @id_empresa
          AND numero_factura LIKE
              'FAC-[0-9][0-9][0-9][0-9][0-9][0-9]';

        IF @n >= 999999
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'Se alcanzó el límite de consecutivos de factura' AS Mensaje;

            RETURN;
        END;

        SET @numero_generado =
            CONCAT(
                'FAC-',
                RIGHT(CONCAT('000000', @n + 1), 6)
            );

        INSERT INTO VENTAS
        (
            id_empresa,
            id_cliente,
            id_cotizacion,
            id_usuario,
            numero_comprobante,
            tipo_comprobante,
            fecha_emision,
            condicion_venta,
            moneda,
            tipo_cambio,
            subtotal,
            impuesto,
            descuento,
            total,
            id_estado
        )
        VALUES
        (
            @id_empresa,
            @id_cliente,
            @id_cotizacion,
            @id_usuario,
            @numero_venta,
            'Factura',
            SYSDATETIME(),
            @condicion_venta,
            @moneda,
            @tipo_cambio,
            @subtotal,
            @impuesto,
            @descuento,
            @total,
            @id_estado_emitida
        );

        SET @id_venta = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO DETALLE_VENTA
        (
            id_venta,
            id_producto,
            cantidad,
            precio_unitario,
            porcentaje_descuento,
            porcentaje_impuesto,
            subtotal
        )
        SELECT
            @id_venta,
            id_producto,
            cantidad,
            precio_unitario,
            porcentaje_descuento,
            porcentaje_impuesto,
            subtotal
        FROM DETALLE_COTIZACION
        WHERE id_cotizacion = @id_cotizacion;

        INSERT INTO FACTURA
        (
            id_venta,
            id_empresa,
            id_usuario,
            id_estado_factura,
            numero_factura,
            fecha_emision,
            tipo_factura,
            moneda,
            tipo_cambio,
            subtotal,
            impuesto,
            descuento,
            total,
            saldo_pendiente
        )
        VALUES
        (
            @id_venta,
            @id_empresa,
            @id_usuario,
            @id_estado_factura_borrador,
            @numero_generado,
            @fecha_factura,
            @tipo_factura,
            @moneda,
            @tipo_cambio,
            @subtotal,
            @impuesto,
            @descuento,
            @total,
            @total
        );

        SET @id_factura = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO DETALLE_FACTURA
        (
            id_factura,
            id_producto,
            cantidad,
            precio_unitario,
            porcentaje_descuento,
            porcentaje_impuesto,
            subtotal
        )
        SELECT
            @id_factura,
            id_producto,
            cantidad,
            precio_unitario,
            porcentaje_descuento,
            porcentaje_impuesto,
            subtotal
        FROM DETALLE_COTIZACION
        WHERE id_cotizacion = @id_cotizacion;

        SET @numero_asiento =
            CONCAT('AST-FAC-', CONVERT(VARCHAR(11), @id_factura));

        IF EXISTS
        (
            SELECT 1
            FROM ASIENTO_CONTABLE WITH (HOLDLOCK)
            WHERE id_empresa = @id_empresa
              AND tipo_origen = 'Factura'
              AND numero_asiento = @numero_asiento
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'Ya existe un asiento para esta factura' AS Mensaje;

            RETURN;
        END;

        SET @total_haber = @suma_detalles + @impuesto;

        IF @total_haber <> @total
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'Los importes de la factura no son coherentes' AS Mensaje;

            RETURN;
        END;

        INSERT INTO ASIENTO_CONTABLE
        (
            id_empresa,
            numero_asiento,
            fecha_asiento,
            descripcion,
            tipo_origen,
            total_debe,
            total_haber,
            id_estado,
            id_usuario
        )
        VALUES
        (
            @id_empresa,
            @numero_asiento,
            CAST(SYSDATETIME() AS DATE),
            CONCAT('Factura ', @numero_generado),
            'Factura',
            @total,
            @total,
            @id_estado_borrador,
            @id_usuario
        );

        SET @id_asiento = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO DETALLE_ASIENTO
        (
            id_asiento,
            id_cuenta_contable,
            id_distribucion_analitica,
            descripcion,
            debe,
            haber
        )
        VALUES
        (
            @id_asiento,
            @id_cuenta_cxc,
            NULL,
            CONCAT('CXC factura ', @numero_generado),
            @total,
            0
        );

        INSERT INTO DETALLE_ASIENTO
        (
            id_asiento,
            id_cuenta_contable,
            id_distribucion_analitica,
            descripcion,
            debe,
            haber
        )
        SELECT
            @id_asiento,
            p.id_cuenta_ingreso,
            NULL,
            CONCAT('Ingreso factura ', @numero_generado),
            0,
            SUM(df.subtotal)
        FROM DETALLE_FACTURA df
        INNER JOIN PRODUCTO p
            ON p.id_producto = df.id_producto
        WHERE df.id_factura = @id_factura
        GROUP BY p.id_cuenta_ingreso
        HAVING SUM(df.subtotal) <> 0;

        IF @impuesto > 0
        BEGIN
            INSERT INTO DETALLE_ASIENTO
            (
                id_asiento,
                id_cuenta_contable,
                id_distribucion_analitica,
                descripcion,
                debe,
                haber
            )
            VALUES
            (
                @id_asiento,
                @id_cuenta_impuesto_ventas,
                NULL,
                CONCAT('Impuesto factura ', @numero_generado),
                0,
                @impuesto
            );
        END;

        SELECT
            @suma_debe = ISNULL(SUM(debe), 0),
            @suma_haber = ISNULL(SUM(haber), 0)
        FROM DETALLE_ASIENTO
        WHERE id_asiento = @id_asiento;

        SELECT @total_factura = total
        FROM FACTURA
        WHERE id_factura = @id_factura;

        SELECT
            @total_debe_asiento = total_debe,
            @total_haber_asiento = total_haber
        FROM ASIENTO_CONTABLE
        WHERE id_asiento = @id_asiento;

        IF @suma_debe <> @suma_haber
           OR @suma_debe <> @total
           OR @suma_haber <> @total
           OR @total_factura IS NULL
           OR @total_factura <> @total
           OR @total_debe_asiento IS NULL
           OR @total_haber_asiento IS NULL
           OR @total_debe_asiento <> @total
           OR @total_haber_asiento <> @total
           OR @total_debe_asiento <> @total_haber_asiento
           OR @suma_debe <> @total_debe_asiento
           OR @suma_haber <> @total_haber_asiento
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                'El asiento contable no está cuadrado' AS Mensaje;

            RETURN;
        END;

        UPDATE COTIZACION
        SET id_estado = @id_estado_facturado
        WHERE id_cotizacion = @id_cotizacion;

        COMMIT TRANSACTION;

        SELECT
            @id_factura AS IdFactura,
            @numero_generado AS NumeroFactura,
            'Facturación exitosa' AS Mensaje;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;