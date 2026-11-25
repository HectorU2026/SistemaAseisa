CREATE   PROCEDURE [dbo].[EditarUsuario]
    @id_usuario        INT,
    @id_empresa        INT,
    @nombre            VARCHAR(100),
    @primer_apellido   VARCHAR(100),
    @segundo_apellido  VARCHAR(100) = NULL,
    @correo            VARCHAR(150),
    @nombre_usuario    VARCHAR(100),
    @id_usuario_accion INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;
        IF dbo.EsAdministradorActivo(@id_usuario_accion,@id_empresa)=0
            THROW 51025,'Operacion no autorizada.',1;
        IF EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU INNER JOIN dbo.ROL R ON R.id_rol=RU.id_rol
            WHERE RU.id_usuario=@id_usuario AND R.nombre='Superadministrador') THROW 51025,'Operacion no autorizada.',1;
        DECLARE @anterior VARCHAR(MAX),@nuevo VARCHAR(MAX);
        SELECT @anterior=(SELECT nombre,primer_apellido,segundo_apellido,correo,nombre_usuario
            FROM dbo.USUARIO WHERE id_usuario=@id_usuario AND id_empresa=@id_empresa FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);

        -- 1. Verificar que el usuario pertenece a la empresa.
        IF NOT EXISTS (
            SELECT 1
            FROM dbo.USUARIO WITH (UPDLOCK, HOLDLOCK)
            WHERE id_usuario = @id_usuario
              AND id_empresa = @id_empresa
        )
        BEGIN
            THROW 51015,
                'El usuario no existe en la empresa indicada.', 1;
        END;

        -- 2. Verificar que el correo no esté registrado
        -- por otro usuario.
        IF EXISTS (
            SELECT 1
            FROM dbo.USUARIO
            WHERE correo = @correo
              AND id_usuario <> @id_usuario
        )
        BEGIN
            THROW 51016,
                'El correo ya esta registrado por otro usuario.', 1;
        END;

        -- Identidad inmutable, incluso ante solicitudes directas.
        IF EXISTS(SELECT 1 FROM dbo.USUARIO WHERE id_usuario=@id_usuario
            AND (nombre_usuario COLLATE Latin1_General_100_BIN2 <> @nombre_usuario COLLATE Latin1_General_100_BIN2
                 OR DATALENGTH(nombre_usuario)<>DATALENGTH(@nombre_usuario)))
            THROW 51210,'El nombre de usuario no puede modificarse.',1;
        -- 4. Actualizar únicamente los datos personales.
        UPDATE dbo.USUARIO
        SET
            nombre = @nombre,
            primer_apellido = @primer_apellido,
            segundo_apellido = @segundo_apellido,
            correo = @correo,
            fecha_modificacion = SYSDATETIME()
        WHERE id_usuario = @id_usuario
          AND id_empresa = @id_empresa;

        SELECT @nuevo=(SELECT nombre,primer_apellido,segundo_apellido,correo,nombre_usuario
            FROM dbo.USUARIO WHERE id_usuario=@id_usuario AND id_empresa=@id_empresa FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);
        INSERT dbo.BITACORA(id_usuario,id_empresa,tabla_afectada,registro_id,accion,valor_anterior,valor_nuevo)
            VALUES(@id_usuario_accion,@id_empresa,'USUARIO',CONVERT(VARCHAR(100),@id_usuario),'EDITAR_USUARIO',@anterior,@nuevo);
        COMMIT TRANSACTION;

        -- 5. Retornar el usuario editado.
        SELECT @id_usuario AS id_usuario;

    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;