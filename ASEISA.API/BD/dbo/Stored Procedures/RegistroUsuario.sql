CREATE   PROCEDURE dbo.RegistroUsuario
    @id_empresa           INT,
    @tipo_cliente         VARCHAR(50) = NULL,
    @nombre               VARCHAR(100),
    @primer_apellido      VARCHAR(100),
    @segundo_apellido     VARCHAR(100) = NULL,
    @correo               VARCHAR(150),
    @nombre_usuario       VARCHAR(100),
    @contrasena_hash      VARCHAR(500),
    @id_estado            INT,
    @telefono             VARCHAR(30) = NULL,
    @direccion            VARCHAR(300) = NULL,
    @identificacion       VARCHAR(50) = NULL,
    @nombre_razon_social  VARCHAR(200) = NULL,
    @id_rol               INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @nombre_rol VARCHAR(100);

        SELECT @nombre_rol = nombre
        FROM dbo.ROL
        WHERE id_rol = @id_rol
          AND id_estado = 1;

        IF @nombre_rol IS NULL
            THROW 50001,
                'El rol seleccionado no existe o esta inactivo.', 1;

        IF @id_estado NOT IN (1, 2)
            THROW 50002,
                'El estado del usuario no es valido.', 1;

        INSERT INTO dbo.USUARIO
        (
            id_empresa,
            tipo_cliente,
            nombre,
            primer_apellido,
            segundo_apellido,
            correo,
            nombre_usuario,
            contrasena_hash,
            id_estado
        )
        VALUES
        (
            @id_empresa,
            @tipo_cliente,
            @nombre,
            @primer_apellido,
            @segundo_apellido,
            @correo,
            @nombre_usuario,
            @contrasena_hash,
            @id_estado
        );

        DECLARE @nuevo_id INT =
            CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.ROL_USUARIO
        (
            id_usuario,
            id_rol,
            id_estado
        )
        VALUES
        (
            @nuevo_id,
            @id_rol,
            1
        );

        IF @nombre_rol = 'Cliente'
        BEGIN
            INSERT INTO dbo.CLIENTE
            (
                id_empresa,
                id_usuario,
                tipo_cliente,
                nombre_razon_social,
                identificacion,
                correo,
                telefono,
                direccion,
                id_estado
            )
            VALUES
            (
                @id_empresa,
                @nuevo_id,
                ISNULL(@tipo_cliente, 'Persona física'),
                ISNULL(
                    @nombre_razon_social,
                    @nombre + ' ' + @primer_apellido
                ),
                ISNULL(@identificacion, @correo),
                @correo,
                @telefono,
                @direccion,
                @id_estado
            );
        END;

        COMMIT TRANSACTION;

        SELECT @nuevo_id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO

