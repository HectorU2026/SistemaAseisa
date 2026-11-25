CREATE   PROCEDURE dbo.AprobarRegistroCliente
    @id_empresa INT,
    @id_usuario INT,
    @id_usuario_accion INT,
    @aprobar BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @aprobar IS NULL
        THROW 51100, 'Debe indicar si aprueba o rechaza la solicitud.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        IF dbo.EsAdministradorActivo(@id_usuario_accion,@id_empresa)=0 THROW 51102,'Operacion no autorizada.',1;

        DECLARE
            @estado_aprobacion VARCHAR(20),
            @id_activo INT,
            @id_inactivo INT,
            @anterior VARCHAR(MAX),
            @nuevo VARCHAR(MAX);

        SELECT @id_activo = id_estado
        FROM dbo.ESTADO
        WHERE LTRIM(RTRIM(nombre_estado)) = 'Activo';

        SELECT @id_inactivo = id_estado
        FROM dbo.ESTADO
        WHERE LTRIM(RTRIM(nombre_estado)) = 'Inactivo';

        IF @id_activo IS NULL OR @id_inactivo IS NULL
            THROW 51101, 'No se encontraron los estados requeridos.', 1;

        -- Verificar permisos del administrador.
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
            THROW 51102, 'No tiene permisos para gestionar solicitudes.', 1;

        -- Obtener solicitud.
        SELECT @estado_aprobacion = U.estado_aprobacion
        FROM dbo.USUARIO U WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN dbo.ROL_USUARIO RU
            ON RU.id_usuario = U.id_usuario
        INNER JOIN dbo.ROL R
            ON R.id_rol = RU.id_rol
        WHERE U.id_usuario = @id_usuario
          AND U.id_empresa = @id_empresa
          AND R.nombre = 'Cliente'
          AND EXISTS(SELECT 1 FROM dbo.CLIENTE C WHERE C.id_usuario=U.id_usuario AND C.id_empresa=U.id_empresa)
          AND R.id_estado = @id_activo
          AND RU.id_estado = @id_activo;

        IF @estado_aprobacion IS NULL
            THROW 51103, 'No se encontro una solicitud de cliente valida.', 1;

        IF @estado_aprobacion <> 'Pendiente'
            THROW 51104, 'La solicitud ya fue procesada.', 1;

        SET @anterior = (
            SELECT id_usuario, id_estado, estado_aprobacion
            FROM dbo.USUARIO
            WHERE id_usuario = @id_usuario
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        -- Aprobar o rechazar.
        UPDATE dbo.USUARIO
        SET
            estado_aprobacion =
                CASE WHEN @aprobar = 1 THEN 'Aprobado' ELSE 'Rechazado' END,
            id_estado =
                CASE WHEN @aprobar = 1 THEN @id_activo ELSE @id_inactivo END,
            fecha_modificacion = SYSDATETIME()
        WHERE id_usuario = @id_usuario
          AND id_empresa = @id_empresa;

        -- Mantener sincronizado el cliente comercial.
        UPDATE dbo.CLIENTE
        SET id_estado =
            CASE WHEN @aprobar = 1 THEN @id_activo ELSE @id_inactivo END
        WHERE id_usuario = @id_usuario
          AND id_empresa = @id_empresa;

        SET @nuevo = (
            SELECT id_usuario, id_estado, estado_aprobacion
            FROM dbo.USUARIO
            WHERE id_usuario = @id_usuario
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

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
            CASE WHEN @aprobar = 1
                 THEN 'APROBAR_CLIENTE'
                 ELSE 'RECHAZAR_CLIENTE'
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