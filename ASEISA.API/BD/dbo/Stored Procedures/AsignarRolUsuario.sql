CREATE PROCEDURE [dbo].[AsignarRolUsuario]
    @id_empresa INT,
    @id_usuario INT, -- ID del usuario al que se le asignará el rol
    @id_rol     INT  -- ID del rol a asignar
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    IF @@TRANCOUNT <> 0
        THROW 51000, 'AsignarRolUsuario debe ejecutarse sin una transaccion externa.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @anterior VARCHAR(MAX), @nuevo VARCHAR(MAX);

        -- Verificar que el usuario existe en la empresa
        IF NOT EXISTS (SELECT 1 FROM dbo.USUARIO WITH (HOLDLOCK)
                       WHERE id_usuario = @id_usuario AND id_empresa = @id_empresa)
            THROW 51015, 'El usuario no existe en la empresa indicada.', 1;

        -- Verificar que el rol existe
        IF NOT EXISTS (SELECT 1 FROM dbo.ROL WITH (HOLDLOCK) WHERE id_rol = @id_rol)
            THROW 51022, 'El rol especificado no existe.', 1;

        -- Capturar estado anterior en ROL_USUARIO si ya tuviera un registro previo
        SET @anterior = (SELECT * FROM dbo.ROL_USUARIO
            WHERE id_usuario = @id_usuario AND id_rol = @id_rol
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        -- Si ya existe la relación, se asegura que esté activo, sino se inserta
        IF EXISTS (SELECT 1 FROM dbo.ROL_USUARIO WHERE id_usuario = @id_usuario AND id_rol = @id_rol)
        BEGIN
            UPDATE dbo.ROL_USUARIO
            SET id_estado = 1, fecha_asignacion = SYSDATETIME()
            WHERE id_usuario = @id_usuario AND id_rol = @id_rol;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.ROL_USUARIO (id_usuario, id_rol, id_estado)
            VALUES (@id_usuario, @id_rol, 1);
        END;

        -- Capturar estado nuevo para la bitácora
        SET @nuevo = (SELECT * FROM dbo.ROL_USUARIO
            WHERE id_usuario = @id_usuario AND id_rol = @id_rol
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        -- Registrar en la bitácora
        INSERT INTO dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES (@id_usuario, @id_empresa, 'ROL_USUARIO', @id_usuario,
                'ASIGNAR_ROL', @anterior, @nuevo);

        COMMIT TRANSACTION;
        SELECT @id_usuario;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;