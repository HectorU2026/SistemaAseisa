CREATE   PROCEDURE dbo.RegistroUsuario
    @id_empresa           INT,
    @tipo_cliente         VARCHAR(50)  = NULL,
    @nombre               VARCHAR(100),
    @primer_apellido      VARCHAR(100),
    @segundo_apellido     VARCHAR(100) = NULL,
    @correo               VARCHAR(150),
    @nombre_usuario       VARCHAR(100),
    @contrasena_hash      VARCHAR(500),
    @id_estado            INT,
    @telefono             VARCHAR(30)  = NULL,
    @direccion            VARCHAR(300) = NULL,
    @identificacion       VARCHAR(50)  = NULL,
    @nombre_razon_social  VARCHAR(200) = NULL,
    @id_rol               INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.USUARIO
        ( 
            id_empresa, tipo_cliente, nombre, primer_apellido, segundo_apellido,
            correo, nombre_usuario, contrasena_hash, id_estado
        )
        VALUES
        (
            @id_empresa,
            ISNULL(@tipo_cliente, 'Persona física'),
            @nombre, @primer_apellido, @segundo_apellido,
            @correo, @nombre_usuario, @contrasena_hash, @id_estado
        );

        DECLARE @nuevo_id INT = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.ROL_USUARIO (id_usuario, id_rol, id_estado)
        VALUES (@nuevo_id, @id_rol, 1);

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

        COMMIT TRANSACTION;

        SELECT @nuevo_id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;