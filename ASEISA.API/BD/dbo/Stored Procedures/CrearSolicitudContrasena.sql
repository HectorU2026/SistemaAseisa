CREATE PROCEDURE dbo.CrearSolicitudContrasena
    @hash_token BINARY(32), @token_cifrado VARCHAR(500), @proposito VARCHAR(20),
    @correo VARCHAR(150)=NULL, @id_usuario INT=NULL, @id_empresa INT=NULL, @id_usuario_accion INT=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NULLIF(@token_cifrado,'') IS NULL OR @hash_token IS NULL OR @proposito IS NULL OR @proposito NOT IN ('Recuperacion','Invitacion','Administrativo')
            THROW 51200,'Solicitud no valida.',1;
        IF @id_usuario_accion IS NOT NULL AND dbo.EsAdministradorActivo(@id_usuario_accion,@id_empresa)=0 AND dbo.EsSuperadministradorActivo(@id_usuario_accion)=0
            THROW 51025,'Operacion no autorizada.',1;
        IF @id_usuario_accion IS NULL AND (@proposito<>'Recuperacion' OR @id_usuario IS NOT NULL OR @id_empresa IS NOT NULL)
            THROW 51025,'Operacion no autorizada.',1;
        DECLARE @version BIGINT,@usuario INT,@empresa INT,@nombre VARCHAR(100),@destino VARCHAR(150);
        SELECT @version=U.version_credencial,@usuario=U.id_usuario,@empresa=U.id_empresa,@nombre=U.nombre,@destino=U.correo
        FROM dbo.USUARIO U WITH(UPDLOCK,HOLDLOCK)
        INNER JOIN dbo.ESTADO EU ON EU.id_estado=U.id_estado
        LEFT JOIN dbo.EMPRESA E ON E.id_empresa=U.id_empresa
        LEFT JOIN dbo.ESTADO EE ON EE.id_estado=E.id_estado
        WHERE ((@id_usuario_accion IS NULL AND U.correo=@correo)
               OR (@id_usuario_accion IS NOT NULL AND U.id_usuario=@id_usuario AND (U.id_empresa=@id_empresa OR (U.id_empresa IS NULL AND @id_empresa IS NULL AND U.id_usuario=@id_usuario_accion))))
          AND U.estado_aprobacion='Aprobado' AND LTRIM(RTRIM(EU.nombre_estado))='Activo'
          AND dbo.UsuarioVigente(U.id_usuario)=1;
        IF @usuario IS NULL
        BEGIN
            IF @id_usuario_accion IS NOT NULL THROW 51015,'Usuario no disponible para esta operacion.',1;
            COMMIT; RETURN;
        END;
        IF @id_usuario_accion IS NOT NULL AND @id_usuario_accion<>@usuario AND EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU
            INNER JOIN dbo.ROL R ON R.id_rol=RU.id_rol WHERE RU.id_usuario=@usuario AND R.nombre='Superadministrador')
            THROW 51025,'Operacion no autorizada.',1;
        IF dbo.EsSuperadministradorActivo(@id_usuario_accion)=1 AND @id_usuario_accion<>@usuario
           AND NOT EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU JOIN dbo.ROL R ON R.id_rol=RU.id_rol
               JOIN dbo.ESTADO EA ON EA.id_estado=RU.id_estado JOIN dbo.ESTADO ER ON ER.id_estado=R.id_estado
               WHERE RU.id_usuario=@usuario AND R.nombre='Administrador' AND LTRIM(RTRIM(EA.nombre_estado))='Activo' AND LTRIM(RTRIM(ER.nombre_estado))='Activo')
            THROW 51025,'La plataforma solo puede restablecer administradores de empresas.',1;
        -- Serializar por usuario. No invalidar un enlace por peticiones publicas repetidas en un minuto.
        IF @id_usuario_accion IS NULL AND EXISTS(SELECT 1 FROM dbo.SOLICITUD_CONTRASENA
            WHERE id_usuario=@usuario AND fecha_creacion>DATEADD(SECOND,-60,SYSUTCDATETIME()))
        BEGIN COMMIT; RETURN; END;
        UPDATE dbo.SOLICITUD_CONTRASENA SET fecha_consumo=SYSUTCDATETIME()
            WHERE id_usuario=@usuario AND fecha_consumo IS NULL;
        UPDATE C SET estado='Anulado',token_cifrado=NULL,reserva=NULL,reserva_hasta=NULL
            FROM dbo.CORREO_ENLACE C INNER JOIN dbo.SOLICITUD_CONTRASENA S ON S.id_solicitud=C.id_solicitud
            WHERE S.id_usuario=@usuario AND C.estado IN ('Pendiente','Procesando');
        INSERT dbo.SOLICITUD_CONTRASENA(id_usuario,id_empresa,correo,hash_token,version_credencial,proposito,fecha_expiracion)
            VALUES(@usuario,@empresa,@destino,@hash_token,@version,@proposito,DATEADD(MINUTE,30,SYSUTCDATETIME()));
        DECLARE @solicitud BIGINT=CONVERT(BIGINT,SCOPE_IDENTITY());
        INSERT dbo.CORREO_ENLACE(id_solicitud,token_cifrado) VALUES(@solicitud,@token_cifrado);
        IF @id_usuario_accion IS NOT NULL
            INSERT dbo.BITACORA(id_usuario,id_empresa,id_empresa_objetivo,tabla_afectada,registro_id,accion)
            VALUES(@id_usuario_accion,CASE WHEN dbo.EsSuperadministradorActivo(@id_usuario_accion)=1 THEN NULL ELSE @empresa END,CASE WHEN dbo.EsSuperadministradorActivo(@id_usuario_accion)=1 THEN @empresa ELSE NULL END,'USUARIO',CONVERT(VARCHAR(100),@usuario),
                CASE WHEN @proposito='Invitacion' THEN 'INVITAR_USUARIO' ELSE 'SOLICITAR_RESTABLECIMIENTO' END);
        COMMIT;
        SELECT @usuario IdUsuario,@nombre Nombre,@destino Correo;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK; THROW;
    END CATCH;
END;