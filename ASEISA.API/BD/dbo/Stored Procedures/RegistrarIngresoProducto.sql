CREATE PROCEDURE RegistrarIngresoProducto
    @id_empresa                 INT,
    @id_producto                INT,
    @id_bodega                  INT,
    @id_usuario                 INT,
    @id_contacto                INT,
    @fecha_entrega              DATETIME2(7),
    @tipo_operacion             VARCHAR(50),
    @cantidad                   DECIMAL(18, 4),
    @cotizacion_compra          VARCHAR(50) = NULL,
    @id_cotizacion              INT = NULL,
    @id_distribucion_analitica  INT = NULL,
    @estado                     VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- Códigos: 1 éxito; 0 incompleto; 2 cantidad negativa; 3 producto no existe;
    -- 4 referencias inválidas o distinta empresa; 5 entrega Listo sin existencia;
    -- 8 catálogo ESTADO inválido; 9 fallo inesperado al actualizar existencia en recepción;
    -- 10 existencias insuficientes; 11 tipo u estado no permitido;
    -- 12 cotización de venta inválida; 13 distribución analítica inválida.
    BEGIN TRY
        BEGIN TRANSACTION;

        SET @tipo_operacion = NULLIF(LTRIM(RTRIM(@tipo_operacion)), '');
        SET @estado = NULLIF(LTRIM(RTRIM(@estado)), '');
        SET @cotizacion_compra = NULLIF(LTRIM(RTRIM(@cotizacion_compra)), '');

        IF @id_empresa IS NULL OR @id_empresa <= 0
            OR @id_producto IS NULL OR @id_producto <= 0
            OR @id_bodega IS NULL OR @id_bodega <= 0
            OR @id_usuario IS NULL OR @id_usuario <= 0
            OR @id_contacto IS NULL OR @id_contacto <= 0
            OR @fecha_entrega IS NULL
            OR @tipo_operacion IS NULL
            OR @cantidad IS NULL
            OR @estado IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0;
            RETURN;
        END;

        IF @cantidad < 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 2;
            RETURN;
        END;

        IF @tipo_operacion NOT IN ('recepcion', 'entrega')
            OR @estado NOT IN ('Borrador', 'Listo')
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 11;
            RETURN;
        END;

        DECLARE @id_empresa_producto INT;
        DECLARE @costo_producto DECIMAL(18, 4);

        SELECT @id_empresa_producto = id_empresa,
               @costo_producto = costo_promedio
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

        IF NOT EXISTS
        (
            SELECT 1
            FROM CONTACTO WITH (HOLDLOCK)
            WHERE id_contacto = @id_contacto
              AND id_empresa = @id_empresa
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 4;
            RETURN;
        END;

        IF @id_cotizacion IS NOT NULL
           AND NOT EXISTS
        (
            SELECT 1
            FROM COTIZACION WITH (HOLDLOCK)
            WHERE id_cotizacion = @id_cotizacion
              AND id_empresa = @id_empresa
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 12;
            RETURN;
        END;

        IF @id_distribucion_analitica IS NOT NULL
           AND NOT EXISTS
        (
            SELECT 1
            FROM DISTRIBUCION_ANALITICA WITH (HOLDLOCK)
            WHERE id_distribucion_analitica = @id_distribucion_analitica
              AND id_empresa = @id_empresa
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 13;
            RETURN;
        END;

        DECLARE @id_estado_documento INT;
        DECLARE @id_activo INT;

        IF
        (
            SELECT COUNT(*)
            FROM ESTADO WITH (HOLDLOCK)
            WHERE LTRIM(RTRIM(nombre_estado)) = @estado
        ) <> 1
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 8;
            RETURN;
        END;

        SELECT @id_estado_documento = id_estado
        FROM ESTADO WITH (HOLDLOCK)
        WHERE LTRIM(RTRIM(nombre_estado)) = @estado;

        IF @estado = 'Listo' AND @cantidad > 0
        BEGIN
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
        END;

        INSERT INTO INGRESO_PRODUCTO
        (
            id_empresa,
            id_producto,
            id_bodega,
            id_usuario,
            id_contacto,
            fecha_entrega,
            tipo_operacion,
            cantidad,
            cotizacion_compra,
            id_cotizacion,
            id_distribucion_analitica,
            id_estado
        )
        VALUES
        (
            @id_empresa,
            @id_producto,
            @id_bodega,
            @id_usuario,
            @id_contacto,
            @fecha_entrega,
            @tipo_operacion,
            @cantidad,
            @cotizacion_compra,
            @id_cotizacion,
            @id_distribucion_analitica,
            @id_estado_documento
        );

        DECLARE @id_ingreso_producto INT = SCOPE_IDENTITY();

        IF @estado = 'Borrador'
        BEGIN
            COMMIT TRANSACTION;
            SELECT 1;
            RETURN;
        END;

        DECLARE @cantidad_disponible DECIMAL(18, 4);
        DECLARE @costo_promedio DECIMAL(18, 4);
        DECLARE @id_existencia INT;
        DECLARE @fecha DATETIME2(7) = SYSDATETIME();

        SELECT @id_existencia = id_existencia,
               @cantidad_disponible = cantidad_disponible,
               @costo_promedio = costo_promedio
        FROM EXISTENCIA WITH (UPDLOCK, HOLDLOCK)
        WHERE id_producto = @id_producto
          AND id_bodega = @id_bodega;

        IF @tipo_operacion = 'entrega'
        BEGIN
            IF @id_existencia IS NULL
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 5;
                RETURN;
            END;

            IF @cantidad_disponible < @cantidad
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 10;
                RETURN;
            END;

            UPDATE EXISTENCIA
            SET cantidad_disponible = cantidad_disponible - @cantidad,
                fecha_actualizacion = @fecha
            WHERE id_existencia = @id_existencia
              AND cantidad_disponible >= @cantidad;

            IF @@ROWCOUNT <> 1
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 10;
                RETURN;
            END;
        END;
        ELSE
        BEGIN
            IF @id_existencia IS NULL
            BEGIN
                INSERT INTO EXISTENCIA
                (
                    id_producto,
                    id_bodega,
                    cantidad_disponible,
                    cantidad_reservada,
                    costo_promedio
                )
                VALUES
                (
                    @id_producto,
                    @id_bodega,
                    @cantidad,
                    0,
                    @costo_producto
                );

                SET @costo_promedio = @costo_producto;
            END;
            ELSE IF @cantidad > 0
            BEGIN
                UPDATE EXISTENCIA
                SET cantidad_disponible = cantidad_disponible + @cantidad,
                    fecha_actualizacion = @fecha
                WHERE id_existencia = @id_existencia;

                IF @@ROWCOUNT <> 1
                BEGIN
                    ROLLBACK TRANSACTION;
                    SELECT 9;
                    RETURN;
                END;
            END;
        END;

        IF @cantidad > 0
        BEGIN
            DECLARE @cantidad_movimiento DECIMAL(18, 4) =
                CASE
                    WHEN @tipo_operacion = 'recepcion' THEN @cantidad
                    ELSE -@cantidad
                END;

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
                @tipo_operacion,
                @cantidad_movimiento,
                @costo_promedio,
                @fecha,
                CONVERT(VARCHAR(100), @id_ingreso_producto),
                NULL,
                @id_usuario,
                @id_activo
            );
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
