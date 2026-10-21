CREATE PROCEDURE RegistrarInventarioFisico
    @id_empresa            INT,
    @id_producto           INT,
    @id_bodega             INT,
    @id_usuario            INT,
    @cantidad_inventario   DECIMAL(18, 4),
    @contado_fisicamente   DECIMAL(18, 4)
AS
BEGIN
    SET NOCOUNT ON;

    -- Códigos: 1 éxito; 2 conteo negativo; 3 producto no existe;
    -- 4 referencias inválidas o distinta empresa; 5 sin existencia;
    -- 8 catálogo ESTADO inválido; 9 conflicto de concurrencia.
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @contado_fisicamente < 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 2;
            RETURN;
        END;

        DECLARE @id_empresa_producto INT;

        SELECT @id_empresa_producto = id_empresa
        FROM PRODUCTO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_producto = @id_producto;

        IF @id_empresa_producto IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 3;
            RETURN;
        END;

        IF @id_empresa_producto <> @id_empresa
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 4;
            RETURN;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM BODEGA WITH (HOLDLOCK)
            WHERE id_bodega = @id_bodega
              AND id_empresa = @id_empresa
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 4;
            RETURN;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM USUARIO WITH (HOLDLOCK)
            WHERE id_usuario = @id_usuario
              AND id_empresa = @id_empresa
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 4;
            RETURN;
        END;

        DECLARE @id_activo INT;

        IF
        (
            SELECT COUNT(*)
            FROM ESTADO WITH (HOLDLOCK)
            WHERE LTRIM(RTRIM(nombre_estado)) = 'Activo'
        ) <> 1
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 8;
            RETURN;
        END;

        SELECT @id_activo = id_estado
        FROM ESTADO WITH (HOLDLOCK)
        WHERE LTRIM(RTRIM(nombre_estado)) = 'Activo';

        DECLARE @cantidad_disponible DECIMAL(18, 4);
        DECLARE @costo_promedio DECIMAL(18, 4);

        SELECT @cantidad_disponible = cantidad_disponible,
               @costo_promedio = costo_promedio
        FROM EXISTENCIA WITH (UPDLOCK, HOLDLOCK)
        WHERE id_producto = @id_producto
          AND id_bodega = @id_bodega;

        IF @cantidad_disponible IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 5;
            RETURN;
        END;

        IF @cantidad_disponible <> @cantidad_inventario
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 9;
            RETURN;
        END;

        DECLARE @fecha DATETIME2(7) = SYSDATETIME();
        DECLARE @diferencia DECIMAL(18, 4) = @cantidad_inventario - @contado_fisicamente;
        DECLARE @cantidad_movimiento DECIMAL(18, 4) = @contado_fisicamente - @cantidad_inventario;

        INSERT INTO INVENTARIO_FISICO
        (
            id_producto,
            id_bodega,
            id_usuario,
            cantidad_inventario,
            contado_fisicamente,
            diferencia,
            fecha_ajuste
        )
        VALUES
        (
            @id_producto,
            @id_bodega,
            @id_usuario,
            @cantidad_inventario,
            @contado_fisicamente,
            @diferencia,
            @fecha
        );

        DECLARE @id_inventario_fisico INT = SCOPE_IDENTITY();

        INSERT INTO MOVIMIENTO_INVENTARIO
        (
            id_producto,
            id_bodega,
            tipo_movimiento,
            cantidad,
            costo_unitario,
            fecha_movimiento,
            documento_origen,
            observacion,
            id_usuario,
            id_estado
        )
        VALUES
        (
            @id_producto,
            @id_bodega,
            'ajuste fisico',
            @cantidad_movimiento,
            @costo_promedio,
            @fecha,
            CONVERT(VARCHAR(100), @id_inventario_fisico),
            CONCAT(
                'libros=',
                @cantidad_inventario,
                '; contado=',
                @contado_fisicamente,
                '; diferencia=',
                @diferencia
            ),
            @id_usuario,
            @id_activo
        );

        UPDATE EXISTENCIA
        SET cantidad_disponible = @contado_fisicamente,
            fecha_actualizacion = @fecha
        WHERE id_producto = @id_producto
          AND id_bodega = @id_bodega
          AND cantidad_disponible = @cantidad_inventario;

        IF @@ROWCOUNT <> 1
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 9;
            RETURN;
        END;

        COMMIT TRANSACTION;
        SELECT 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
