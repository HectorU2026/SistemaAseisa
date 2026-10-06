CREATE PROCEDURE RegistrarProducto
    @id_empresa                 INT,
    @codigo                     VARCHAR(50),
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
    @id_bodega                  INT,
    @cantidad                   DECIMAL(18, 4)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS
        (
            SELECT 1
            FROM BODEGA
            WHERE id_bodega = @id_bodega
              AND id_empresa = @id_empresa
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0;
            RETURN;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM CUENTA_CONTABLE
            WHERE id_cuenta_contable = @id_cuenta_ingreso
              AND id_empresa = @id_empresa
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0;
            RETURN;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM CUENTA_CONTABLE
            WHERE id_cuenta_contable = @id_cuenta_gasto
              AND id_empresa = @id_empresa
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0;
            RETURN;
        END;

        INSERT INTO PRODUCTO
        (
            id_empresa,
            codigo,
            nombre,
            descripcion,
            tipo_producto,
            costo_promedio,
            precio_venta,
            id_estado,
            politica_facturacion,
            porcentaje_impuesto_venta,
            porcentaje_impuesto_compra,
            peso,
            volumen,
            tiempo_entrega,
            id_cuenta_ingreso,
            id_cuenta_gasto
        )
        VALUES
        (
            @id_empresa,
            @codigo,
            @nombre,
            @descripcion,
            @tipo_producto,
            @costo_promedio,
            @precio_venta,
            @id_estado,
            @politica_facturacion,
            @porcentaje_impuesto_venta,
            @porcentaje_impuesto_compra,
            @peso,
            @volumen,
            @tiempo_entrega,
            @id_cuenta_ingreso,
            @id_cuenta_gasto
        );

        DECLARE @id_producto INT = SCOPE_IDENTITY();

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
            @costo_promedio
        );

        COMMIT TRANSACTION;
        SELECT 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
