
CREATE   PROCEDURE [dbo].[EditarCotizacion]
    @id_cotizacion      INT,
    @id_empresa         INT,
    @id_cliente         INT,
    @fecha_vencimiento  DATETIME2 = NULL,
    @moneda             VARCHAR(10) = NULL,
    @tipo_cambio        DECIMAL(18,6) = NULL,
    @subtotal           DECIMAL(18,2),
    @impuesto           DECIMAL(18,2),
    @descuento          DECIMAL(18,2),
    @total              DECIMAL(18,2),
    @id_usuario         INT, 
    @detalles_json      NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    IF @@TRANCOUNT <> 0
        THROW 51000, 'EditarCotizacion debe ejecutarse sin una transaccion externa.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @estado_actual INT, @id_estado_cotizacion INT;
        DECLARE @anterior VARCHAR(MAX), @nuevo VARCHAR(MAX);

        -- 1. Verificar que la cotización existe en la empresa
        SELECT @estado_actual = id_estado_cotizacion
        FROM dbo.COTIZACION WITH (UPDLOCK, HOLDLOCK)
        WHERE id_cotizacion = @id_cotizacion AND id_empresa = @id_empresa;

        IF @estado_actual IS NULL
            THROW 51015, 'La cotizacion no existe en la empresa indicada.', 1;

        -- 2. Validar estado 'Cotización'
        SELECT @id_estado_cotizacion = id_estado_cotizacion FROM dbo.ESTADO_COTIZACION WHERE LTRIM(RTRIM(nombre_estado)) = 'Cotización';
        
        IF @estado_actual <> @id_estado_cotizacion
            THROW 51009, 'Solo se pueden editar cotizaciones que se encuentren en estado "Cotización".', 1;

        -- 3. Capturar el estado anterior completo para la bitácora
        SET @anterior = (SELECT * FROM dbo.COTIZACION WHERE id_cotizacion = @id_cotizacion FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        -- 4. Actualizar la cabecera de la cotización
        UPDATE dbo.COTIZACION
        SET id_cliente = @id_cliente,
            fecha_vencimiento = @fecha_vencimiento,
            moneda = @moneda,
            tipo_cambio = @tipo_cambio,
            subtotal = @subtotal,
            impuesto = @impuesto,
            descuento = @descuento,
            total = @total
        WHERE id_cotizacion = @id_cotizacion AND id_empresa = @id_empresa;

        -- 5. Sincronizar los detalles: Eliminar los detalles anteriores e insertar los nuevos actualizados
        DELETE FROM dbo.DETALLE_COTIZACION WHERE id_cotizacion = @id_cotizacion;

        INSERT INTO dbo.DETALLE_COTIZACION
        (
            id_cotizacion, id_producto, cantidad, precio_unitario, 
            porcentaje_descuento, porcentaje_impuesto, subtotal
        )
        SELECT 
            @id_cotizacion,
            j.id_producto,
            j.cantidad,
            j.precio_unitario,
            j.porcentaje_descuento,
            j.porcentaje_impuesto,
            j.subtotal
        FROM OPENJSON(@detalles_json) 
        WITH (
            id_producto             INT             '$.IdProducto',
            cantidad                DECIMAL(18,4)   '$.Cantidad',
            precio_unitario         DECIMAL(18,2)   '$.PrecioUnitario',
            porcentaje_descuento    DECIMAL(8,4)    '$.PorcentajeDescuento',
            porcentaje_impuesto     DECIMAL(8,4)    '$.PorcentajeImpuesto',
            subtotal                DECIMAL(18,2)   '$.Subtotal'
        ) AS j;

        -- 6. Capturar el estado nuevo para la bitácora
        SET @nuevo = (SELECT * FROM dbo.COTIZACION WHERE id_cotizacion = @id_cotizacion FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        -- 7. Registrar en la BITACORA
        INSERT INTO dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES 
            (@id_usuario, @id_empresa, 'COTIZACION', @id_cotizacion, 'EDITAR', @anterior, @nuevo);

        COMMIT TRANSACTION;
        SELECT @id_cotizacion;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;