CREATE PROCEDURE EditarProducto
    @id_producto                INT,
    @nombre                     VARCHAR(150),
    @descripcion                VARCHAR(300) = NULL,
    @tipo_producto              VARCHAR(50),
    @costo_promedio             DECIMAL(18, 4),
    @precio_venta               DECIMAL(18, 2),
    @id_estado                  INT,
    @politica_facturacion       VARCHAR(50),
    @porcentaje_impuesto_venta  DECIMAL(8, 4),
    @porcentaje_impuesto_compra DECIMAL(8, 4),
    @peso                       DECIMAL(18, 4) = NULL,
    @volumen                    DECIMAL(18, 4) = NULL,
    @tiempo_entrega             INT,
    @id_cuenta_ingreso          INT,
    @id_cuenta_gasto            INT,
    @id_bodega                  INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Códigos: 1 éxito; 3 no existe; 4 referencias inválidas;
    -- 5 sin existencia; 6 varias existencias; 7 conflicto en destino.
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @id_empresa INT;

        SELECT @id_empresa = id_empresa
        FROM PRODUCTO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_producto = @id_producto;

        IF @id_empresa IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 3;
            RETURN;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM ESTADO WITH (HOLDLOCK)
            WHERE id_estado = @id_estado
        )
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
            FROM CUENTA_CONTABLE WITH (HOLDLOCK)
            WHERE id_cuenta_contable = @id_cuenta_ingreso
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
            FROM CUENTA_CONTABLE WITH (HOLDLOCK)
            WHERE id_cuenta_contable = @id_cuenta_gasto
              AND id_empresa = @id_empresa
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 4;
            RETURN;
        END;

        DECLARE @cantidad_existencias INT;
        DECLARE @id_bodega_actual INT;

        SELECT @cantidad_existencias = COUNT(*)
        FROM EXISTENCIA WITH (UPDLOCK, HOLDLOCK)
        WHERE id_producto = @id_producto;

        IF @cantidad_existencias = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 5;
            RETURN;
        END;

        IF @cantidad_existencias > 1
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 6;
            RETURN;
        END;

        SELECT @id_bodega_actual = id_bodega
        FROM EXISTENCIA WITH (UPDLOCK, HOLDLOCK)
        WHERE id_producto = @id_producto;

        IF @id_bodega_actual <> @id_bodega
        BEGIN
            IF EXISTS
            (
                SELECT 1
                FROM EXISTENCIA WITH (UPDLOCK, HOLDLOCK)
                WHERE id_producto = @id_producto
                  AND id_bodega = @id_bodega
            )
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 7;
                RETURN;
            END;

            UPDATE EXISTENCIA
            SET id_bodega = @id_bodega,
                fecha_actualizacion = SYSDATETIME()
            WHERE id_producto = @id_producto
              AND id_bodega = @id_bodega_actual;
        END;

        UPDATE PRODUCTO
        SET nombre = @nombre,
            descripcion = @descripcion,
            tipo_producto = @tipo_producto,
            costo_promedio = @costo_promedio,
            precio_venta = @precio_venta,
            id_estado = @id_estado,
            politica_facturacion = @politica_facturacion,
            porcentaje_impuesto_venta = @porcentaje_impuesto_venta,
            porcentaje_impuesto_compra = @porcentaje_impuesto_compra,
            peso = @peso,
            volumen = @volumen,
            tiempo_entrega = @tiempo_entrega,
            id_cuenta_ingreso = @id_cuenta_ingreso,
            id_cuenta_gasto = @id_cuenta_gasto,
            fecha_modificacion = SYSDATETIME()
        WHERE id_producto = @id_producto;

        COMMIT TRANSACTION;
        SELECT 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
