CREATE   PROCEDURE [dbo].[CancelarCotizacion]
    @id_empresa     INT,
    @id_usuario     INT, 
    @id_cotizacion  INT  
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    IF @@TRANCOUNT <> 0
        THROW 51000, 'CancelarCotizacion debe ejecutarse sin una transaccion externa.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @estado_actual INT;
        DECLARE @id_estado_cotizacion INT, @id_estado_enviada INT, @id_estado_cancelada INT;
        DECLARE @anterior VARCHAR(MAX), @nuevo VARCHAR(MAX);

        -- 1. Verificar que la cotización existe en la empresa
        SELECT @estado_actual = id_estado_cotizacion
        FROM dbo.COTIZACION WITH (UPDLOCK, HOLDLOCK)
        WHERE id_cotizacion = @id_cotizacion AND id_empresa = @id_empresa;

        IF @estado_actual IS NULL
            THROW 51015, 'La cotizacion no existe en la empresa indicada.', 1;

        -- 2. Validar el catálogo de estados necesarios en la nueva tabla
        IF (SELECT COUNT(*) FROM dbo.ESTADO_COTIZACION WITH (HOLDLOCK) WHERE LTRIM(RTRIM(nombre_estado)) = 'Cotización') <> 1
            OR (SELECT COUNT(*) FROM dbo.ESTADO_COTIZACION WITH (HOLDLOCK) WHERE LTRIM(RTRIM(nombre_estado)) = 'Cotización Enviada') <> 1
            OR (SELECT COUNT(*) FROM dbo.ESTADO_COTIZACION WITH (HOLDLOCK) WHERE LTRIM(RTRIM(nombre_estado)) = 'Cancelada') <> 1
            THROW 51020, 'El catalogo ESTADO_COTIZACION debe tener definidos los estados requeridos (Cotización, Cotización Enviada, Cancelada).', 1;

        SELECT @id_estado_cotizacion = id_estado_cotizacion FROM dbo.ESTADO_COTIZACION WHERE LTRIM(RTRIM(nombre_estado)) = 'Cotización';
        SELECT @id_estado_enviada = id_estado_cotizacion FROM dbo.ESTADO_COTIZACION WHERE LTRIM(RTRIM(nombre_estado)) = 'Cotización Enviada';
        SELECT @id_estado_cancelada = id_estado_cotizacion FROM dbo.ESTADO_COTIZACION WHERE LTRIM(RTRIM(nombre_estado)) = 'Cancelada';

        -- 3. Validar que la cotización esté obligatoriamente en "Cotización" o "Cotización Enviada"
        IF @estado_actual NOT IN (@id_estado_cotizacion, @id_estado_enviada)
            THROW 51009, 'Solo se pueden cancelar cotizaciones que se encuentren en estado "Cotización" o "Cotización Enviada".', 1;

        -- 4. Capturar estado anterior para la bitácora
        SET @anterior = (SELECT * FROM dbo.COTIZACION
            WHERE id_cotizacion = @id_cotizacion AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        -- 5. Actualizar el estado a "Cancelada"
        UPDATE dbo.COTIZACION 
        SET id_estado_cotizacion = @id_estado_cancelada
        WHERE id_cotizacion = @id_cotizacion AND id_empresa = @id_empresa;

        -- 6. Capturar estado nuevo para la bitácora
        SET @nuevo = (SELECT * FROM dbo.COTIZACION
            WHERE id_cotizacion = @id_cotizacion AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        -- 7. Registrar en la BITACORA
        INSERT INTO dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES (@id_usuario, @id_empresa, 'COTIZACION', @id_cotizacion,
                'CANCELAR_COTIZACION', @anterior, @nuevo);

        COMMIT TRANSACTION;
        SELECT @id_cotizacion;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;