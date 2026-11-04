CREATE PROCEDURE dbo.RegistrarPagoFactura
    @id_factura  INT,
    @id_empresa  INT,
    @monto       DECIMAL(18, 2),
    @metodo_pago VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @id_estado_factura_emitida INT;
    DECLARE @id_estado_factura_pagada INT;
    DECLARE @id_estado_activo INT;
    DECLARE @id_venta INT;
    DECLARE @id_cliente INT;
    DECLARE @id_estado_actual INT;
    DECLARE @id_cuenta_cobrar INT;
    DECLARE @id_pago_cliente INT;
    DECLARE @cantidad_facturas INT;
    DECLARE @cantidad_cxc INT;
    DECLARE @existe BIT;
    DECLARE @numero_factura VARCHAR(50);
    DECLARE @saldo_factura DECIMAL(18, 2);
    DECLARE @saldo_cxc DECIMAL(18, 2);
    DECLARE @total DECIMAL(18, 2);
    DECLARE @fecha_emision DATETIME2(7);
    DECLARE @fecha_vencimiento DATETIME2(7);
    DECLARE @saldo_nuevo DECIMAL(18, 2);
    DECLARE @metodo VARCHAR(50);
    DECLARE @id_estado_factura_destino INT;
    DECLARE @nombre_estado_destino VARCHAR(100);

    BEGIN TRY
        BEGIN TRANSACTION;

        SET @metodo = NULLIF(LTRIM(RTRIM(@metodo_pago)), '');

        IF @metodo IS NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                5 AS Codigo,
                'Por favor, rellene los campos obligatorios' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                CAST(NULL AS INT) AS IdPagoCliente,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        IF @monto IS NULL
           OR @monto <= 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                5 AS Codigo,
                'Por favor, rellene los campos obligatorios' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                CAST(NULL AS INT) AS IdPagoCliente,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SELECT @id_estado_factura_emitida = id_estado_factura
        FROM dbo.ESTADO_FACTURA
        WHERE nombre_estado = N'Emitida';

        SELECT @id_estado_factura_pagada = id_estado_factura
        FROM dbo.ESTADO_FACTURA
        WHERE nombre_estado = N'Pagada';

        SELECT @id_estado_activo = id_estado
        FROM dbo.ESTADO
        WHERE nombre_estado = N'Activo';

        IF @id_estado_factura_emitida IS NULL
           OR @id_estado_factura_pagada IS NULL
           OR @id_estado_activo IS NULL
           OR (SELECT COUNT(*) FROM dbo.ESTADO_FACTURA WHERE nombre_estado = N'Emitida') <> 1
           OR (SELECT COUNT(*) FROM dbo.ESTADO_FACTURA WHERE nombre_estado = N'Pagada') <> 1
           OR (SELECT COUNT(*) FROM dbo.ESTADO WHERE nombre_estado = N'Activo') <> 1
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                7 AS Codigo,
                'No se encontraron los estados de facturación' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                CAST(NULL AS INT) AS IdPagoCliente,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SET @existe = 0;

        SELECT
            @existe = 1,
            @id_venta = f.id_venta,
            @id_estado_actual = f.id_estado_factura,
            @numero_factura = f.numero_factura,
            @saldo_factura = f.saldo_pendiente,
            @total = f.total,
            @fecha_emision = f.fecha_emision,
            @fecha_vencimiento = f.fecha_vencimiento
        FROM dbo.FACTURA f WITH (UPDLOCK, HOLDLOCK)
        WHERE f.id_factura = @id_factura
          AND f.id_empresa = @id_empresa;

        IF @existe = 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS Codigo,
                'La factura no existe' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                CAST(NULL AS INT) AS IdPagoCliente,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        IF @saldo_factura IS NULL
           OR @saldo_factura <= 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                4 AS Codigo,
                'Esta factura no cuenta con saldo pendiente' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                CAST(NULL AS INT) AS IdPagoCliente,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        IF @id_estado_actual <> @id_estado_factura_emitida
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                9 AS Codigo,
                'Solo se pueden registrar pagos de facturas Emitida' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                CAST(NULL AS INT) AS IdPagoCliente,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        IF @monto > @saldo_factura
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                6 AS Codigo,
                'El monto no puede ser mayor al saldo pendiente' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                CAST(NULL AS INT) AS IdPagoCliente,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SELECT @cantidad_facturas = COUNT(*)
        FROM dbo.FACTURA WITH (HOLDLOCK)
        WHERE id_venta = @id_venta;

        IF @cantidad_facturas <> 1
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                8 AS Codigo,
                'La venta de la factura no es única' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                CAST(NULL AS INT) AS IdPagoCliente,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SELECT @id_cliente = v.id_cliente
        FROM dbo.VENTAS v WITH (HOLDLOCK)
        WHERE v.id_venta = @id_venta
          AND v.id_empresa = @id_empresa;

        IF @id_cliente IS NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS Codigo,
                'La factura no existe' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                CAST(NULL AS INT) AS IdPagoCliente,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SELECT @cantidad_cxc = COUNT(*)
        FROM dbo.CUENTA_POR_COBRAR WITH (UPDLOCK, HOLDLOCK)
        WHERE id_venta = @id_venta;

        IF @cantidad_cxc > 1
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                8 AS Codigo,
                'La cuenta por cobrar de la venta no es única' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                CAST(NULL AS INT) AS IdPagoCliente,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        IF @cantidad_cxc = 1
        BEGIN
            SELECT
                @id_cuenta_cobrar = cxc.id_cuenta_cobrar,
                @saldo_cxc = cxc.saldo_pendiente
            FROM dbo.CUENTA_POR_COBRAR cxc WITH (UPDLOCK, HOLDLOCK)
            WHERE cxc.id_venta = @id_venta;

            IF @saldo_cxc <> @saldo_factura
            BEGIN
                ROLLBACK TRANSACTION;

                SELECT
                    10 AS Codigo,
                    'El saldo de la cuenta por cobrar no coincide con el de la factura' AS Mensaje,
                    CAST(NULL AS INT) AS IdFactura,
                    CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                    CAST(NULL AS VARCHAR(100)) AS Estado,
                    CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                    CAST(NULL AS INT) AS IdPagoCliente,
                    CAST(NULL AS BINARY(8)) AS VersionFila;

                RETURN;
            END;
        END;
        ELSE
        BEGIN
            IF @fecha_emision IS NULL
               OR @fecha_vencimiento IS NULL
            BEGIN
                ROLLBACK TRANSACTION;

                SELECT
                    11 AS Codigo,
                    'La factura no tiene fechas de emisión y vencimiento para crear la cuenta por cobrar' AS Mensaje,
                    CAST(NULL AS INT) AS IdFactura,
                    CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                    CAST(NULL AS VARCHAR(100)) AS Estado,
                    CAST(NULL AS DECIMAL(18, 2)) AS SaldoPendiente,
                    CAST(NULL AS INT) AS IdPagoCliente,
                    CAST(NULL AS BINARY(8)) AS VersionFila;

                RETURN;
            END;

            INSERT dbo.CUENTA_POR_COBRAR
            (
                id_venta,
                id_cliente,
                fecha_emision,
                fecha_vencimiento,
                monto_original,
                saldo_pendiente,
                id_estado
            )
            VALUES
            (
                @id_venta,
                @id_cliente,
                @fecha_emision,
                @fecha_vencimiento,
                @total,
                @saldo_factura,
                @id_estado_activo
            );

            SET @id_cuenta_cobrar = CONVERT(INT, SCOPE_IDENTITY());
        END;

        SET @saldo_nuevo = @saldo_factura - @monto;

        IF @saldo_nuevo = 0
        BEGIN
            SET @id_estado_factura_destino = @id_estado_factura_pagada;
            SET @nombre_estado_destino = N'Pagada';
        END;
        ELSE
        BEGIN
            SET @id_estado_factura_destino = @id_estado_factura_emitida;
            SET @nombre_estado_destino = N'Emitida';
        END;

        INSERT dbo.PAGO_CLIENTE
        (
            id_cuenta_cobrar,
            id_cuenta_bancaria,
            fecha_pago,
            monto,
            metodo_pago,
            referencia,
            id_estado
        )
        VALUES
        (
            @id_cuenta_cobrar,
            NULL,
            SYSDATETIME(),
            @monto,
            @metodo,
            NULL,
            @id_estado_activo
        );

        SET @id_pago_cliente = CONVERT(INT, SCOPE_IDENTITY());

        UPDATE dbo.FACTURA
        SET saldo_pendiente = @saldo_nuevo,
            id_estado_factura = @id_estado_factura_destino,
            fecha_modificacion = SYSDATETIME()
        WHERE id_factura = @id_factura
          AND id_empresa = @id_empresa;

        UPDATE dbo.CUENTA_POR_COBRAR
        SET saldo_pendiente = @saldo_nuevo
        WHERE id_cuenta_cobrar = @id_cuenta_cobrar;

        COMMIT TRANSACTION;

        SELECT
            1 AS Codigo,
            'Pago registrado' AS Mensaje,
            f.id_factura AS IdFactura,
            f.numero_factura AS NumeroFactura,
            @nombre_estado_destino AS Estado,
            f.saldo_pendiente AS SaldoPendiente,
            @id_pago_cliente AS IdPagoCliente,
            f.version_fila AS VersionFila
        FROM dbo.FACTURA f
        WHERE f.id_factura = @id_factura
          AND f.id_empresa = @id_empresa;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
