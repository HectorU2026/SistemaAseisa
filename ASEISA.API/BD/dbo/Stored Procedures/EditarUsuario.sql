CREATE PROCEDURE [dbo].[EditarUsuario]
    @id_usuario            INT,
    @id_empresa            INT,
    @tipo_cliente          VARCHAR(50)  = NULL,
    @nombre                VARCHAR(100),
    @primer_apellido       VARCHAR(100),
    @segundo_apellido      VARCHAR(100) = NULL,
    @correo                VARCHAR(150),
    @nombre_usuario        VARCHAR(100),
    @contrasena_hash       VARCHAR(500) = NULL, -- Si viene NULL o vacío, se conserva la contraseña actual
    @id_estado             INT,
    @telefono              VARCHAR(30)  = NULL,
    @direccion             VARCHAR(300) = NULL,
    @identificacion        VARCHAR(50)  = NULL,
    @nombre_razon_social   VARCHAR(200) = NULL,
    @id_rol                INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Actualizar la tabla principal USUARIO
        UPDATE dbo.USUARIO
        SET 
            tipo_cliente       = ISNULL(@tipo_cliente, tipo_cliente),
            nombre             = @nombre,
            primer_apellido    = @primer_apellido,
            segundo_apellido   = @segundo_apellido,
            correo             = @correo,
            nombre_usuario     = @nombre_usuario,
            contrasena_hash    = ISNULL(NULLIF(@contrasena_hash, ''), contrasena_hash),
            id_estado          = @id_estado,
			fecha_modificacion = SYSDATETIME()
        WHERE id_usuario = @id_usuario AND id_empresa = @id_empresa;

        -- 2. Actualizar o asegurar el rol en ROL_USUARIO
        IF EXISTS (SELECT 1 FROM dbo.ROL_USUARIO WHERE id_usuario = @id_usuario)
        BEGIN
            UPDATE dbo.ROL_USUARIO
            SET id_rol = @id_rol,
                id_estado = @id_estado
            WHERE id_usuario = @id_usuario;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.ROL_USUARIO (id_usuario, id_rol, id_estado)
            VALUES (@id_usuario, @id_rol, @id_estado);
        END;

        -- 3. Actualizar o insertar el registro en CLIENTE 
        IF EXISTS (SELECT 1 FROM dbo.CLIENTE WHERE id_empresa = @id_empresa AND (correo = @correo OR identificacion = ISNULL(@identificacion, @correo)))
        BEGIN
            UPDATE dbo.CLIENTE
            SET 
                tipo_cliente          = ISNULL(@tipo_cliente, tipo_cliente),
                nombre_razon_social   = ISNULL(@nombre_razon_social, @nombre + ' ' + @primer_apellido),
                identificacion        = ISNULL(@identificacion, identificacion),
                correo                = @correo,
                telefono              = @telefono,
                direccion             = @direccion,
                id_estado             = @id_estado
            WHERE id_empresa = @id_empresa AND (correo = @correo OR identificacion = ISNULL(@identificacion, @correo));
        END
        ELSE
        BEGIN
            INSERT INTO dbo.CLIENTE
            (
                id_empresa, tipo_cliente, nombre_razon_social, identificacion,
                correo, telefono, direccion, id_estado
            )
            VALUES
            (
                @id_empresa,
                ISNULL(@tipo_cliente, 'Persona física'),
                ISNULL(@nombre_razon_social, @nombre + ' ' + @primer_apellido),
                ISNULL(@identificacion, @correo),
                @correo,
                @telefono,
                @direccion,
                @id_estado
            );
        END;

        COMMIT TRANSACTION;

        -- Retornar el ID del usuario editado
        SELECT @id_usuario;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;