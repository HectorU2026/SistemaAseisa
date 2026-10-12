CREATE PROCEDURE [dbo].[RestablecerContrasenaUsuario]
    @id_empresa INT,
    @id_usuario INT, -- ID del usuario al que se le restablecerá la contraseña
    @contrasena_hash VARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    IF @@TRANCOUNT <> 0
        THROW 51000, 'RestablecerContrasenaUsuario debe ejecutarse sin una transaccion externa.', 1;
    IF @contrasena_hash IS NULL OR LTRIM(RTRIM(@contrasena_hash)) = ''
        THROW 51021, 'Indique la nueva contrasena.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @anterior VARCHAR(MAX), @nuevo VARCHAR(MAX);

        IF NOT EXISTS (SELECT 1 FROM dbo.USUARIO WITH (UPDLOCK, HOLDLOCK)
                       WHERE id_usuario = @id_usuario AND id_empresa = @id_empresa)
            THROW 51015, 'El usuario no existe en la empresa indicada.', 1;

        SET @anterior = (SELECT * FROM dbo.USUARIO
            WHERE id_usuario = @id_usuario AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
            
        UPDATE dbo.USUARIO 
        SET contrasena_hash = @contrasena_hash, 
            fecha_modificacion = SYSDATETIME()
        WHERE id_usuario = @id_usuario AND id_empresa = @id_empresa;

        SET @nuevo = (SELECT * FROM dbo.USUARIO
            WHERE id_usuario = @id_usuario AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
            
        INSERT dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES (@id_usuario, @id_empresa, 'USUARIO', @id_usuario,
                'RESTABLECER_CONTRASENA', @anterior, @nuevo);

        COMMIT TRANSACTION;
        SELECT @id_usuario;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;