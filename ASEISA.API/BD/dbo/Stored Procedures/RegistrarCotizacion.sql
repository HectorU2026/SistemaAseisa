

CREATE   PROCEDURE [dbo].[RegistrarCotizacion]
    @id_empresa         INT,
    @id_cliente         INT,
    @numero_cotizacion  VARCHAR(50),
    @fecha_emision      DATETIME2,
    @fecha_vencimiento  DATETIME2 = NULL,
    @moneda             VARCHAR(10) = NULL,
    @tipo_cambio        DECIMAL(18,6) = NULL,
    @subtotal           DECIMAL(18,2),
    @impuesto           DECIMAL(18,2),
    @descuento          DECIMAL(18,2),
    @total              DECIMAL(18,2),
    @id_estado          INT, 
    @id_usuario         INT, 
    @detalles_json      NVARCHAR(MAX) 
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    IF @@TRANCOUNT <> 0
        THROW 51000, 'RegistrarCotizacion debe ejecutarse sin una transaccion externa.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Insertar la cabecera de la cotización
        INSERT INTO dbo.COTIZACION
        (
            id_empresa, id_cliente, numero_cotizacion, fecha_emision, 
            fecha_vencimiento, moneda, tipo_cambio, subtotal, 
            impuesto, descuento, total, id_estado_cotizacion
        )
        VALUES
        (
            @id_empresa, @id_cliente, @numero_cotizacion, @fecha_emision, 
            @fecha_vencimiento, @moneda, @tipo_cambio, @subtotal, 
            @impuesto, @descuento, @total, @id_estado
        );

        DECLARE @nueva_cotizacion_id INT = CAST(SCOPE_IDENTITY() AS INT);

        -- 2. Insertar los detalles des serializando el JSON recibido
        INSERT INTO dbo.DETALLE_COTIZACION
        (
            id_cotizacion, id_producto, cantidad, precio_unitario, 
            porcentaje_descuento, porcentaje_impuesto, subtotal
        )
        SELECT 
            @nueva_cotizacion_id,
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

        -- 3. Registrar en la Bitácora el estado nuevo
        DECLARE @nuevo VARCHAR(MAX);
        SET @nuevo = (SELECT * FROM dbo.COTIZACION WHERE id_cotizacion = @nueva_cotizacion_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        INSERT INTO dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES 
            (@id_usuario, @id_empresa, 'COTIZACION', @nueva_cotizacion_id, 'CREAR', NULL, @nuevo);

        COMMIT TRANSACTION;

        -- Retornar el ID de la cotización creada
        SELECT @nueva_cotizacion_id;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;