CREATE PROCEDURE dbo.ConsumirSolicitudContrasena @hash_token BINARY(32),@contrasena_hash VARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @hash_token IS NULL OR NULLIF(LTRIM(RTRIM(@contrasena_hash)),'') IS NULL
        THROW 51200,'Solicitud no valida.',1;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @version BIGINT,@usuario INT,@empresa INT,@correo VARCHAR(150);
        -- Mismo orden de bloqueo que la emision: usuario y luego sus solicitudes.
        SELECT @usuario=id_usuario FROM dbo.SOLICITUD_CONTRASENA WHERE hash_token=@hash_token;
        IF @usuario IS NULL BEGIN COMMIT; SELECT CAST(0 AS BIT) Exito; RETURN; END;
        SELECT @version=U.version_credencial,@empresa=U.id_empresa,@correo=U.correo FROM dbo.USUARIO U WITH(UPDLOCK,HOLDLOCK)
        INNER JOIN dbo.ESTADO EU ON EU.id_estado=U.id_estado
        LEFT JOIN dbo.EMPRESA E ON E.id_empresa=U.id_empresa
        LEFT JOIN dbo.ESTADO EE ON EE.id_estado=E.id_estado
        WHERE U.id_usuario=@usuario AND U.estado_aprobacion='Aprobado'
            AND LTRIM(RTRIM(EU.nombre_estado))='Activo' AND dbo.UsuarioVigente(U.id_usuario)=1;
        IF @version IS NULL OR NOT EXISTS(SELECT 1 FROM dbo.SOLICITUD_CONTRASENA WITH(UPDLOCK,HOLDLOCK)
            WHERE hash_token=@hash_token AND id_usuario=@usuario AND (id_empresa=@empresa OR (id_empresa IS NULL AND @empresa IS NULL)) AND correo=@correo
                AND version_credencial=@version AND fecha_consumo IS NULL AND fecha_expiracion>SYSUTCDATETIME())
        BEGIN COMMIT; SELECT CAST(0 AS BIT) Exito; RETURN; END;
        UPDATE dbo.USUARIO SET contrasena_hash=@contrasena_hash,version_credencial=version_credencial+1,fecha_modificacion=SYSDATETIME() WHERE id_usuario=@usuario;
        UPDATE dbo.SOLICITUD_CONTRASENA SET fecha_consumo=SYSUTCDATETIME() WHERE id_usuario=@usuario AND fecha_consumo IS NULL;
        UPDATE C SET estado='Anulado',token_cifrado=NULL,reserva=NULL,reserva_hasta=NULL
            FROM dbo.CORREO_ENLACE C INNER JOIN dbo.SOLICITUD_CONTRASENA S ON S.id_solicitud=C.id_solicitud
            WHERE S.id_usuario=@usuario AND C.estado IN ('Pendiente','Procesando');
        INSERT dbo.BITACORA(id_usuario,id_empresa,tabla_afectada,registro_id,accion)
            VALUES(@usuario,@empresa,'USUARIO',CONVERT(VARCHAR(100),@usuario),'CAMBIO_CONTRASENA');
        COMMIT; SELECT CAST(1 AS BIT) Exito;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK; THROW;
    END CATCH;
END;