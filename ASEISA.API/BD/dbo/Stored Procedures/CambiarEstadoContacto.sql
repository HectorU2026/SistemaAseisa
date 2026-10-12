CREATE   PROCEDURE dbo.CambiarEstadoContacto
    @id_empresa INT,
    @id_usuario INT,
    @id_contacto INT,
    @activo BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @@TRANCOUNT <> 0
        THROW 51000, 'CambiarEstadoContacto debe ejecutarse sin una transaccion externa.', 1;
    IF @activo IS NULL
        THROW 51019, 'Indique si el contacto debe estar activo.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @estado_actual INT, @rol VARCHAR(10),
                @id_activo INT, @id_inactivo INT, @estado_nuevo INT,
                @anterior VARCHAR(MAX), @nuevo VARCHAR(MAX);

        SELECT @estado_actual = id_estado, @rol = rol_contacto
        FROM dbo.CONTACTO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa;
        IF @estado_actual IS NULL
            THROW 51015, 'El contacto no existe en la empresa indicada.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.USUARIO WITH (HOLDLOCK)
                       WHERE id_usuario = @id_usuario AND id_empresa = @id_empresa)
            THROW 51008, 'El usuario no pertenece a la empresa indicada.', 1;

        -- No se asumen los IDs del catalogo. Debe existir un estado de cada nombre.
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
            THROW 51009, 'El contacto requiere un estado Activo o Inactivo.', 1;
        SET @estado_nuevo = CASE WHEN @activo = 1 THEN @id_activo ELSE @id_inactivo END;

        -- Repetir una solicitud no cambia la fecha ni genera otra bitacora.
        IF @estado_actual = @estado_nuevo
        BEGIN
            COMMIT TRANSACTION;
            SELECT @id_contacto;
            RETURN;
        END;

        SET @anterior = (SELECT * FROM dbo.CONTACTO
            WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        UPDATE dbo.CONTACTO SET id_estado = @estado_nuevo, fecha_modificacion = SYSDATETIME()
        WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa;

        -- Los registros de roles retirados se conservan inactivos.
        UPDATE dbo.CLIENTE
        SET id_estado = CASE WHEN @activo = 1 AND @rol IN ('Cliente','Ambos')
                             THEN @id_activo ELSE @id_inactivo END,
            fecha_modificacion = SYSDATETIME()
        WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa;
        UPDATE dbo.PROVEEDOR
        SET id_estado = CASE WHEN @activo = 1 AND @rol IN ('Proveedor','Ambos')
                             THEN @id_activo ELSE @id_inactivo END
        WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa;

        SET @nuevo = (SELECT * FROM dbo.CONTACTO
            WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        INSERT dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES (@id_usuario, @id_empresa, 'CONTACTO', @id_contacto,
                CASE WHEN @activo = 1 THEN 'ACTIVAR' ELSE 'DESACTIVAR' END, @anterior, @nuevo);

        COMMIT TRANSACTION;
        SELECT @id_contacto;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;