CREATE PROCEDURE [dbo].[AvanzarCotizacionEnviada]
    @id_empresa     INT,
    @id_usuario     INT, 
    @id_cotizacion  INT  
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    IF @@TRANCOUNT <> 0
        THROW 51000, 'AvanzarCotizacionEnviada debe ejecutarse sin una transaccion externa.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @estado_actual INT, @id_estado_cotizacion INT, @id_estado_enviada INT,
                @cliente INT, @subtotal DECIMAL(18,2), @total DECIMAL(18,2),
                @anterior VARCHAR(MAX), @nuevo VARCHAR(MAX);

        -- 1. Verificar que la cotización existe en la empresa
        SELECT 
            @estado_actual = id_estado,
            @cliente = id_cliente,
            @subtotal = subtotal,
            @total = total
        FROM dbo.COTIZACION WITH (UPDLOCK, HOLDLOCK)
        WHERE id_cotizacion = @id_cotizacion AND id_empresa = @id_empresa;

        IF @estado_actual IS NULL
            THROW 51015, 'La cotizacion no existe en la empresa indicada.', 1;

        -- 2. Validar el catálogo de estados para obtener los IDs correctos de 'Cotización' y 'Cotización Enviada'
        IF (SELECT COUNT(*) FROM dbo.ESTADO WITH (HOLDLOCK) WHERE LTRIM(RTRIM(nombre_estado)) = 'Cotización') <> 1
            OR (SELECT COUNT(*) FROM dbo.ESTADO WITH (HOLDLOCK) WHERE LTRIM(RTRIM(nombre_estado)) = 'Cotización Enviada') <> 1
            THROW 51020, 'El catalogo ESTADO debe tener definidos los estados requeridos.', 1;

        SELECT @id_estado_cotizacion = id_estado FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado)) = 'Cotización';
        SELECT @id_estado_enviada = id_estado FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado)) = 'Cotización Enviada';

        -- 3. Validar que la cotización se encuentre obligatoriamente en estado "Cotización"
        IF @estado_actual <> @id_estado_cotizacion
            THROW 51009, 'La cotizacion debe estar en estado "Cotización" para poder avanzar a enviada.', 1;

        -- 4. VALIDAR QUE LA INFORMACIÓN ESTÉ COMPLETA 
        IF @cliente IS NULL OR @cliente <= 0
            THROW 51021, 'La cotizacion debe tener un cliente asignado.', 1;

        IF @total IS NULL OR @total <= 0
            THROW 51022, 'La cotizacion debe tener un monto total válido.', 1;

        -- Validar que tenga al menos un producto en el detalle
        IF NOT EXISTS (SELECT 1 FROM dbo.DETALLE_COTIZACION WHERE id_cotizacion = @id_cotizacion)
            THROW 51023, 'La cotizacion debe contener al menos un producto en su detalle para ser enviada.', 1;

        -- 5. Capturar estado anterior para la bitácora
        SET @anterior = (SELECT * FROM dbo.COTIZACION
            WHERE id_cotizacion = @id_cotizacion AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        -- 6. Actualizar el estado de la cotización a "Cotización Envidada" y la fecha de modificación si aplica
        UPDATE dbo.COTIZACION 
        SET id_estado = @id_estado_enviada
        WHERE id_cotizacion = @id_cotizacion AND id_empresa = @id_empresa;

        -- 7. Capturar estado nuevo para la bitácora
        SET @nuevo = (SELECT * FROM dbo.COTIZACION
            WHERE id_cotizacion = @id_cotizacion AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        -- 8. Registrar en la BITACORA
        INSERT INTO dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES (@id_usuario, @id_empresa, 'COTIZACION', @id_cotizacion,
                'AVANZAR_COTIZACION_ENVIADA', @anterior, @nuevo);

        COMMIT TRANSACTION;
        SELECT @id_cotizacion;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;