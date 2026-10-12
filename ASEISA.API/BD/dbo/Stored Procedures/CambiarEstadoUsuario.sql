CREATE PROCEDURE [dbo].[CambiarEstadoUsuario]
    @id_empresa         INT,
    @id_usuario         INT, -- Usuario administrador que ejecuta la acción
    @id_usuario_objetivo INT, -- Usuario al que se le cambiará el estado
    @activo             BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    IF @@TRANCOUNT <> 0
        THROW 51000, 'CambiarEstadoUsuario debe ejecutarse sin una transaccion externa.', 1;
    IF @activo IS NULL
        THROW 51019, 'Indique si el usuario debe estar activo.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @estado_actual INT,
                @id_activo INT, @id_inactivo INT, @estado_nuevo INT,
                @anterior VARCHAR(MAX), @nuevo VARCHAR(MAX);

        -- Verificar que el usuario objetivo existe en la empresa
        SELECT @estado_actual = id_estado
        FROM dbo.USUARIO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_usuario = @id_usuario_objetivo AND id_empresa = @id_empresa;

        IF @estado_actual IS NULL
            THROW 51015, 'El usuario no existe en la empresa indicada.', 1;

        -- Verificar que el usuario que ejecuta pertenece a la empresa
        IF NOT EXISTS (SELECT 1 FROM dbo.USUARIO WITH (HOLDLOCK)
                       WHERE id_usuario = @id_usuario AND id_empresa = @id_empresa)
            THROW 51008, 'El usuario que ejecuta no pertenece a la empresa indicada.', 1;

        -- Validar catálogo de estados
        IF (SELECT COUNT(*) FROM dbo.ESTADO WITH (HOLDLOCK)
            WHERE LTRIM(RTRIM(nombre_estado)) = 'Activo') <> 1
            OR (SELECT COUNT(*) FROM dbo.ESTADO WITH (HOLDLOCK)
                WHERE LTRIM(RTRIM(nombre_estado)) = 'Inactivo') <> 1
            THROW 51020, 'El catalogo ESTADO debe tener un unico Activo y un unico Inactivo.', 1;

        SELECT @id_activo = id_estado FROM dbo.ESTADO
        WHERE LTRIM(RTRIM(nombre_estado)) = 'Activo';
        
        SELECT @id_inactivo = id_estado FROM dbo.ESTADO
        WHERE LTRIM(RTRIM(nombre_estado)) = 'Inactivo';

        IF @estado_actual NOT IN (@id_activo, @id_inactivo)
            THROW 51009, 'El usuario requiere un estado Activo o Inactivo.', 1;

        SET @estado_nuevo = CASE WHEN @activo = 1 THEN @id_activo ELSE @id_inactivo END;

        -- Si ya tiene el estado solicitado, no hace nada extra
        IF @estado_actual = @estado_nuevo
        BEGIN
            COMMIT TRANSACTION;
            SELECT @id_usuario_objetivo;
            RETURN;
        END;

        -- Capturar estado anterior para la bitácora
        SET @anterior = (SELECT * FROM dbo.USUARIO
            WHERE id_usuario = @id_usuario_objetivo AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        -- Actualizar estado en la tabla principal USUARIO
        UPDATE dbo.USUARIO 
        SET id_estado = @estado_nuevo, 
            fecha_modificacion = SYSDATETIME()
        WHERE id_usuario = @id_usuario_objetivo AND id_empresa = @id_empresa;

        -- Actualizar también el estado en ROL_USUARIO para mantener consistencia
        UPDATE dbo.ROL_USUARIO
        SET id_estado = @estado_nuevo
        WHERE id_usuario = @id_usuario_objetivo;

        -- Capturar estado nuevo para la bitácora
        SET @nuevo = (SELECT * FROM dbo.USUARIO
            WHERE id_usuario = @id_usuario_objetivo AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        -- Registrar en la bitácora del sistema
        INSERT INTO dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES (@id_usuario, @id_empresa, 'USUARIO', @id_usuario_objetivo,
                CASE WHEN @activo = 1 THEN 'ACTIVAR' ELSE 'DESACTIVAR' END, @anterior, @nuevo);

        COMMIT TRANSACTION;
        SELECT @id_usuario_objetivo;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;