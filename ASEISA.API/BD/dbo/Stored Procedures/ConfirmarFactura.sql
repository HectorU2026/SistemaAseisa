CREATE PROCEDURE dbo.ConfirmarFactura
    @id_factura   INT,
    @id_empresa   INT,
    @id_usuario   INT,
    @version_fila BINARY(8)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @id_estado_factura_borrador INT;
    DECLARE @id_estado_factura_emitida INT;
    DECLARE @id_estado_asiento_borrador INT;
    DECLARE @id_estado_asiento_emitida INT;
    DECLARE @id_estado_facturado INT;
    DECLARE @id_venta INT;
    DECLARE @id_asiento INT;
    DECLARE @id_estado_actual INT;
    DECLARE @id_cotizacion INT;
    DECLARE @id_estado_cotizacion INT;
    DECLARE @numero_factura VARCHAR(50);
    DECLARE @total DECIMAL(18, 2);
    DECLARE @suma_debe DECIMAL(18, 2);
    DECLARE @suma_haber DECIMAL(18, 2);
    DECLARE @total_debe DECIMAL(18, 2);
    DECLARE @total_haber DECIMAL(18, 2);
    DECLARE @existe BIT;
    DECLARE @version_actual BINARY(8);
    DECLARE @estado_factura_anterior VARCHAR(100);
    DECLARE @estado_asiento_anterior VARCHAR(100);
    DECLARE @valor_anterior VARCHAR(MAX);
    DECLARE @valor_nuevo VARCHAR(MAX);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @version_fila IS NULL
           OR DATALENGTH(@version_fila) <> 8
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                8 AS Codigo,
                'La versión de la factura es obligatoria' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SELECT @id_estado_factura_borrador = id_estado_factura
        FROM ESTADO_FACTURA
        WHERE nombre_estado = 'Borrador';

        SELECT @id_estado_factura_emitida = id_estado_factura
        FROM ESTADO_FACTURA
        WHERE nombre_estado = 'Emitida';

        SELECT @id_estado_asiento_borrador = id_estado
        FROM ESTADO
        WHERE nombre_estado = 'Borrador';

        SELECT @id_estado_asiento_emitida = id_estado
        FROM ESTADO
        WHERE nombre_estado = 'Emitida';

        SELECT @id_estado_facturado = id_estado
        FROM ESTADO
        WHERE nombre_estado = 'Facturado';

        IF @id_estado_factura_borrador IS NULL
           OR @id_estado_factura_emitida IS NULL
           OR @id_estado_asiento_borrador IS NULL
           OR @id_estado_asiento_emitida IS NULL
           OR @id_estado_facturado IS NULL
           OR (SELECT COUNT(*) FROM ESTADO_FACTURA WHERE nombre_estado = 'Borrador') <> 1
           OR (SELECT COUNT(*) FROM ESTADO_FACTURA WHERE nombre_estado = 'Emitida') <> 1
           OR (SELECT COUNT(*) FROM ESTADO WHERE nombre_estado = 'Borrador') <> 1
           OR (SELECT COUNT(*) FROM ESTADO WHERE nombre_estado = 'Emitida') <> 1
           OR (SELECT COUNT(*) FROM ESTADO WHERE nombre_estado = 'Facturado') <> 1
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                7 AS Codigo,
                'No se encontraron los estados de facturación' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SET @existe = 0;

        SELECT
            @existe = 1,
            @id_venta = f.id_venta,
            @id_estado_actual = f.id_estado_factura,
            @numero_factura = f.numero_factura,
            @total = f.total,
            @version_actual = f.version_fila,
            @estado_factura_anterior = ef.nombre_estado
        FROM dbo.FACTURA f WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN dbo.ESTADO_FACTURA ef
            ON ef.id_estado_factura = f.id_estado_factura
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
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        IF @version_actual <> @version_fila
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                2 AS Codigo,
                'La factura fue modificada por otro usuario' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        IF @id_estado_actual <> @id_estado_factura_borrador
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                3 AS Codigo,
                'La factura no está en Borrador' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SELECT @id_cotizacion = v.id_cotizacion
        FROM dbo.VENTAS v WITH (HOLDLOCK)
        WHERE v.id_venta = @id_venta
          AND v.id_empresa = @id_empresa;

        IF @id_cotizacion IS NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                4 AS Codigo,
                'La cotización de origen no existe o no está facturada' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SELECT @id_estado_cotizacion = c.id_estado
        FROM dbo.COTIZACION c WITH (HOLDLOCK)
        WHERE c.id_cotizacion = @id_cotizacion
          AND c.id_empresa = @id_empresa;

        IF @id_estado_cotizacion IS NULL
           OR @id_estado_cotizacion <> @id_estado_facturado
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                4 AS Codigo,
                'La cotización de origen no existe o no está facturada' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SELECT
            @id_asiento = a.id_asiento,
            @total_debe = a.total_debe,
            @total_haber = a.total_haber,
            @estado_asiento_anterior = e.nombre_estado
        FROM dbo.ASIENTO_CONTABLE a WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN dbo.ESTADO e
            ON e.id_estado = a.id_estado
        WHERE a.id_empresa = @id_empresa
          AND a.tipo_origen = 'Factura'
          AND a.numero_asiento = CONCAT('AST-FAC-', CONVERT(VARCHAR(11), @id_factura));

        IF @id_asiento IS NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                5 AS Codigo,
                'El asiento contable no existe' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SELECT
            @suma_debe = ISNULL(SUM(debe), 0),
            @suma_haber = ISNULL(SUM(haber), 0)
        FROM dbo.DETALLE_ASIENTO
        WHERE id_asiento = @id_asiento;

        IF @suma_debe <> @suma_haber
           OR @suma_debe <> @total
           OR @suma_haber <> @total
           OR @total_debe IS NULL
           OR @total_haber IS NULL
           OR @total_debe <> @total
           OR @total_haber <> @total
           OR @total_debe <> @total_haber
           OR @suma_debe <> @total_debe
           OR @suma_haber <> @total_haber
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                5 AS Codigo,
                'El asiento contable no está cuadrado' AS Mensaje,
                CAST(NULL AS INT) AS IdFactura,
                CAST(NULL AS VARCHAR(50)) AS NumeroFactura,
                CAST(NULL AS VARCHAR(100)) AS Estado,
                CAST(NULL AS BINARY(8)) AS VersionFila;

            RETURN;
        END;

        SET @valor_anterior = CONCAT(
            '{"estadoFactura":"',
            @estado_factura_anterior,
            '","estadoAsiento":"',
            @estado_asiento_anterior,
            '"}');

        UPDATE dbo.FACTURA
        SET id_estado_factura = @id_estado_factura_emitida,
            fecha_modificacion = SYSDATETIME()
        WHERE id_factura = @id_factura
          AND id_empresa = @id_empresa;

        UPDATE dbo.ASIENTO_CONTABLE
        SET id_estado = @id_estado_asiento_emitida
        WHERE id_asiento = @id_asiento;

        SET @valor_nuevo = '{"estadoFactura":"Emitida","estadoAsiento":"Emitida"}';

        INSERT dbo.BITACORA
        (
            id_usuario,
            id_empresa,
            tabla_afectada,
            registro_id,
            accion,
            valor_anterior,
            valor_nuevo
        )
        VALUES
        (
            @id_usuario,
            @id_empresa,
            'FACTURA',
            CONVERT(VARCHAR(100), @id_factura),
            'CONFIRMAR',
            @valor_anterior,
            @valor_nuevo
        );

        COMMIT TRANSACTION;

        SELECT
            1 AS Codigo,
            'Factura confirmada' AS Mensaje,
            f.id_factura AS IdFactura,
            f.numero_factura AS NumeroFactura,
            ef.nombre_estado AS Estado,
            f.version_fila AS VersionFila
        FROM dbo.FACTURA f
        INNER JOIN dbo.ESTADO_FACTURA ef
            ON ef.id_estado_factura = f.id_estado_factura
        WHERE f.id_factura = @id_factura
          AND f.id_empresa = @id_empresa;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
