CREATE PROCEDURE [dbo].[CambiarEstadoUsuario]
    @id_empresa INT,
    @id_usuario INT,
    @activo BIT,
    @id_usuario_accion INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @@TRANCOUNT <> 0
        THROW 51000,
            'CambiarEstadoUsuario debe ejecutarse sin una transaccion externa.', 1;

    IF @activo IS NULL
        THROW 51019,
            'Indique si el usuario debe estar activo.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        IF dbo.EsAdministradorActivo(@id_usuario_accion,@id_empresa)=0 THROW 51025,'Operacion no autorizada.',1;
        IF EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU INNER JOIN dbo.ROL R ON R.id_rol=RU.id_rol
            WHERE RU.id_usuario=@id_usuario AND R.nombre='Superadministrador') THROW 51025,'Operacion no autorizada.',1;

        DECLARE
            @estado_actual INT,
            @id_activo INT,
            @id_inactivo INT,
            @estado_nuevo INT,
            @anterior VARCHAR(MAX),
            @nuevo VARCHAR(MAX);

        -- 1. Obtener los estados del catálogo.
        SELECT @id_activo = id_estado
        FROM dbo.ESTADO
        WHERE LTRIM(RTRIM(nombre_estado)) = 'Activo';

        SELECT @id_inactivo = id_estado
        FROM dbo.ESTADO
        WHERE LTRIM(RTRIM(nombre_estado)) = 'Inactivo';

        IF @id_activo IS NULL OR @id_inactivo IS NULL
            THROW 51020,
                'No se encontraron los estados Activo e Inactivo.', 1;

        -- Determinar el nuevo estado solicitado.
        SET @estado_nuevo =
            CASE
                WHEN @activo = 1 THEN @id_activo
                ELSE @id_inactivo
            END;

        -- 2. Verificar que quien realiza la acción
        -- sea administrador activo y aprobado de la empresa.
        IF NOT EXISTS (
            SELECT 1
            FROM dbo.USUARIO U
            INNER JOIN dbo.ROL_USUARIO RU
                ON RU.id_usuario = U.id_usuario
            INNER JOIN dbo.ROL R
                ON R.id_rol = RU.id_rol
            WHERE U.id_usuario = @id_usuario_accion
              AND U.id_empresa = @id_empresa
              AND U.id_estado = @id_activo
              AND U.estado_aprobacion = 'Aprobado'
              AND RU.id_estado = @id_activo
              AND R.id_estado = @id_activo
              AND R.nombre = 'Administrador'
        )
        BEGIN
            THROW 51021,
                'No tiene permisos para cambiar el estado de usuarios.', 1;
        END;

        -- 3. Verificar que el usuario existe en la empresa.
        SELECT @estado_actual = id_estado
        FROM dbo.USUARIO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_usuario = @id_usuario
          AND id_empresa = @id_empresa;

        IF @estado_actual IS NULL
            THROW 51015,
                'El usuario no existe en la empresa indicada.', 1;

        -- Impedir activar usuarios pendientes o rechazados.
        IF @activo = 1
           AND EXISTS (
               SELECT 1
               FROM dbo.USUARIO
               WHERE id_usuario = @id_usuario
                 AND id_empresa = @id_empresa
                 AND estado_aprobacion <> 'Aprobado'
           )
        BEGIN
            THROW 51024,
                'No se puede activar una cuenta pendiente o rechazada.', 1;
        END;

        -- 4. Evitar que un administrador
        -- desactive su propia cuenta.
        IF @id_usuario = @id_usuario_accion
           AND @activo = 0
            THROW 51023,
                'No puede desactivar su propia cuenta.', 1;

        -- 5. Si ya tiene el estado solicitado,
        -- no realizar cambios adicionales.
        IF @estado_actual = @estado_nuevo
        BEGIN
            COMMIT TRANSACTION;

            SELECT @id_usuario AS id_usuario;
            RETURN;
        END;

        -- 6. Guardar información anterior
        -- sin incluir la contraseña.
        SET @anterior = (
            SELECT
                id_usuario,
                id_empresa,
                nombre,
                primer_apellido,
                correo,
                id_estado
            FROM dbo.USUARIO
            WHERE id_usuario = @id_usuario
              AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        -- 7. Cambiar únicamente el estado de USUARIO.
        UPDATE dbo.USUARIO
        SET
            id_estado = @estado_nuevo,
            fecha_modificacion = SYSDATETIME()
        WHERE id_usuario = @id_usuario
          AND id_empresa = @id_empresa;

        -- 8. Guardar información nueva.
        SET @nuevo = (
            SELECT
                id_usuario,
                id_empresa,
                nombre,
                primer_apellido,
                correo,
                id_estado
            FROM dbo.USUARIO
            WHERE id_usuario = @id_usuario
              AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        -- 9. Registrar en bitácora.
        INSERT INTO dbo.BITACORA
        (
            id_usuario,
            id_empresa,
            tabla_afectada,
            registro_id,
            accion,
            valor_anterior,
            valor_nuevo
        )
        VALUES
        (
            @id_usuario_accion,
            @id_empresa,
            'USUARIO',
            @id_usuario,
            CASE
                WHEN @activo = 1 THEN 'ACTIVAR'
                ELSE 'DESACTIVAR'
            END,
            @anterior,
            @nuevo
        );

        COMMIT TRANSACTION;

        SELECT @id_usuario AS id_usuario;

    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
