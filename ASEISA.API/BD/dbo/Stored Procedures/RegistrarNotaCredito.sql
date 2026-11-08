CREATE PROCEDURE dbo.RegistrarNotaCredito
    @id_factura             INT,
    @id_empresa             INT,
    @id_usuario             INT,
    @motivo                 VARCHAR(500),
    @monto                  DECIMAL(18, 2),
    @descripcion_adicional  VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @id_estado_factura_emitida INT;
    DECLARE @id_estado_factura_pagada INT;
    DECLARE @id_estado_activo INT;
    DECLARE @id_estado_actual INT;
    DECLARE @id_nota_credito INT;
    DECLARE @existe BIT;
    DECLARE @numero_factura VARCHAR(50);
    DECLARE @total_factura DECIMAL(18, 2);
    DECLARE @suma_notas DECIMAL(18, 2);
    DECLARE @motivo_normalizado VARCHAR(500);
    DECLARE @descripcion VARCHAR(500);
    DECLARE @numero_generado VARCHAR(50);
    DECLARE @n INT;
    DECLARE @applock INT;
    DECLARE @recurso_applock NVARCHAR(100);

    BEGIN TRY
        BEGIN TRANSACTION;

        SET @motivo_normalizado = NULLIF(LTRIM(RTRIM(@motivo)), '');
        SET @descripcion = NULLIF(LTRIM(RTRIM(@descripcion_adicional)), '');

        IF @motivo_normalizado IS NULL
           OR @monto IS NULL
           OR @monto <= 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                5 AS Codigo,
                'Por favor, rellene los campos obligatorios' AS Mensaje,
                CAST(NULL AS INT) AS IdNotaCredito,
                CAST(NULL AS VARCHAR(50)) AS NumeroNotaCredito,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS DECIMAL(18, 2)) AS Monto;

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
                CAST(NULL AS INT) AS IdNotaCredito,
                CAST(NULL AS VARCHAR(50)) AS NumeroNotaCredito,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS DECIMAL(18, 2)) AS Monto;

            RETURN;
        END;

        SET @existe = 0;

        SELECT
            @existe = 1,
            @id_estado_actual = f.id_estado_factura,
            @numero_factura = f.numero_factura,
            @total_factura = f.total
        FROM dbo.FACTURA f WITH (UPDLOCK, HOLDLOCK)
        WHERE f.id_factura = @id_factura
          AND f.id_empresa = @id_empresa;

        IF @existe = 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                0 AS Codigo,
                'La factura no existe' AS Mensaje,
                CAST(NULL AS INT) AS IdNotaCredito,
                CAST(NULL AS VARCHAR(50)) AS NumeroNotaCredito,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS DECIMAL(18, 2)) AS Monto;

            RETURN;
        END;

        IF @id_estado_actual NOT IN (@id_estado_factura_emitida, @id_estado_factura_pagada)
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                9 AS Codigo,
                'Solo se pueden registrar notas de crédito de facturas Emitida o Pagada' AS Mensaje,
                CAST(NULL AS INT) AS IdNotaCredito,
                CAST(NULL AS VARCHAR(50)) AS NumeroNotaCredito,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS DECIMAL(18, 2)) AS Monto;

            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.NOTA_CREDITO nc WITH (UPDLOCK, HOLDLOCK)
            WHERE nc.id_factura = @id_factura
              AND nc.id_estado = @id_estado_activo
              AND LTRIM(RTRIM(nc.motivo)) = @motivo_normalizado
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                10 AS Codigo,
                'Ya existe un registro con este numeroFactura con el mismo motivo' AS Mensaje,
                CAST(NULL AS INT) AS IdNotaCredito,
                CAST(NULL AS VARCHAR(50)) AS NumeroNotaCredito,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS DECIMAL(18, 2)) AS Monto;

            RETURN;
        END;

        SELECT @suma_notas = ISNULL(SUM(nc.monto), 0)
        FROM dbo.NOTA_CREDITO nc WITH (UPDLOCK, HOLDLOCK)
        WHERE nc.id_factura = @id_factura
          AND nc.id_estado = @id_estado_activo;

        IF @suma_notas + @monto > @total_factura
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                11 AS Codigo,
                'La suma de notas de crédito no puede superar el total de la factura' AS Mensaje,
                CAST(NULL AS INT) AS IdNotaCredito,
                CAST(NULL AS VARCHAR(50)) AS NumeroNotaCredito,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS DECIMAL(18, 2)) AS Monto;

            RETURN;
        END;

        SET @recurso_applock =
            N'NC-NUMERO-' + CONVERT(NVARCHAR(11), @id_empresa);

        EXEC @applock = [sys].[sp_getapplock]
            @Resource = @recurso_applock,
            @LockMode = N'Exclusive',
            @LockOwner = N'Transaction',
            @LockTimeout = 8000;

        IF @applock < 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                12 AS Codigo,
                'No se pudo obtener el consecutivo de nota de crédito' AS Mensaje,
                CAST(NULL AS INT) AS IdNotaCredito,
                CAST(NULL AS VARCHAR(50)) AS NumeroNotaCredito,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS DECIMAL(18, 2)) AS Monto;

            RETURN;
        END;

        SELECT
            @n = ISNULL(
                MAX(
                    TRY_CONVERT(
                        INT,
                        SUBSTRING(nc.numero_nota_credito, 4, 6)
                    )
                ),
                0
            )
        FROM dbo.NOTA_CREDITO nc WITH (HOLDLOCK)
        INNER JOIN dbo.FACTURA f
            ON f.id_factura = nc.id_factura
        WHERE f.id_empresa = @id_empresa
          AND nc.numero_nota_credito LIKE
              'NC-[0-9][0-9][0-9][0-9][0-9][0-9]';

        IF @n >= 999999
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                13 AS Codigo,
                'Se alcanzó el límite de consecutivos de nota de crédito' AS Mensaje,
                CAST(NULL AS INT) AS IdNotaCredito,
                CAST(NULL AS VARCHAR(50)) AS NumeroNotaCredito,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS DECIMAL(18, 2)) AS Monto;

            RETURN;
        END;

        SET @numero_generado =
            CONCAT(
                'NC-',
                RIGHT(CONCAT('000000', @n + 1), 6)
            );

        INSERT dbo.NOTA_CREDITO
        (
            id_factura,
            id_usuario,
            numero_nota_credito,
            fecha_emision,
            motivo,
            monto,
            id_estado,
            descripcion_adicional
        )
        VALUES
        (
            @id_factura,
            @id_usuario,
            @numero_generado,
            SYSDATETIME(),
            @motivo_normalizado,
            @monto,
            @id_estado_activo,
            @descripcion
        );

        SET @id_nota_credito = CONVERT(INT, SCOPE_IDENTITY());

        COMMIT TRANSACTION;

        SELECT
            1 AS Codigo,
            'Nota de crédito registrada' AS Mensaje,
            @id_nota_credito AS IdNotaCredito,
            @numero_generado AS NumeroNotaCredito,
            @id_factura AS IdFactura,
            @numero_factura AS NumeroFactura,
            @monto AS Monto;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
