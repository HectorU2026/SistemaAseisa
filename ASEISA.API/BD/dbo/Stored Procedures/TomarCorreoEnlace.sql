CREATE PROCEDURE dbo.TomarCorreoEnlace
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @id BIGINT,@solicitud BIGINT,@usuario INT,@ahora DATETIME2(7)=SYSUTCDATETIME();
    SELECT TOP(1) @id=C.id_correo,@solicitud=C.id_solicitud,@usuario=S.id_usuario
    FROM dbo.CORREO_ENLACE C INNER JOIN dbo.SOLICITUD_CONTRASENA S ON S.id_solicitud=C.id_solicitud
    WHERE (C.estado='Pendiente' AND C.proximo_intento<=@ahora)
       OR (C.estado='Procesando' AND C.reserva_hasta<=@ahora)
    ORDER BY C.proximo_intento,C.id_correo;
    IF @id IS NULL RETURN;
    BEGIN TRY
        BEGIN TRANSACTION;
        -- Orden consistente con emision y consumo: usuario -> solicitud -> correo.
        DECLARE @empresa INT,@nombre VARCHAR(100),@correo VARCHAR(150),@vigente BIT=0;
        SELECT @empresa=U.id_empresa,@nombre=U.nombre,@correo=U.correo FROM dbo.USUARIO U WITH(UPDLOCK,HOLDLOCK)
        WHERE U.id_usuario=@usuario;
        SELECT @vigente=1 FROM dbo.SOLICITUD_CONTRASENA S WITH(UPDLOCK,HOLDLOCK)
        INNER JOIN dbo.USUARIO U ON U.id_usuario=S.id_usuario
        INNER JOIN dbo.ESTADO EU ON EU.id_estado=U.id_estado
        INNER JOIN dbo.EMPRESA E ON E.id_empresa=U.id_empresa
        INNER JOIN dbo.ESTADO EE ON EE.id_estado=E.id_estado
        WHERE S.id_solicitud=@solicitud AND S.fecha_consumo IS NULL AND S.fecha_expiracion>SYSUTCDATETIME()
          AND S.id_empresa=@empresa AND S.correo=@correo AND S.version_credencial=U.version_credencial AND U.estado_aprobacion='Aprobado'
          AND LTRIM(RTRIM(EU.nombre_estado))='Activo' AND LTRIM(RTRIM(EE.nombre_estado))='Activo';
        DECLARE @reserva UNIQUEIDENTIFIER=NEWID();
        DECLARE @tomado TABLE(IdCorreo BIGINT,Reserva UNIQUEIDENTIFIER,TokenCifrado VARCHAR(500));
        UPDATE dbo.CORREO_ENLACE WITH(UPDLOCK,ROWLOCK)
        SET estado=CASE WHEN @vigente=0 THEN 'Anulado' WHEN intentos>=5 THEN 'Fallido' ELSE 'Procesando' END,
            token_cifrado=CASE WHEN @vigente=0 OR intentos>=5 THEN NULL ELSE token_cifrado END,
            reserva=CASE WHEN @vigente=1 AND intentos<5 THEN @reserva ELSE NULL END,
            reserva_hasta=CASE WHEN @vigente=1 AND intentos<5 THEN DATEADD(MINUTE,5,SYSUTCDATETIME()) ELSE NULL END,
            intentos=CASE WHEN @vigente=1 AND intentos<5 THEN intentos+1 ELSE intentos END
        OUTPUT inserted.id_correo,inserted.reserva,inserted.token_cifrado INTO @tomado
        WHERE id_correo=@id AND ((estado='Pendiente' AND proximo_intento<=SYSUTCDATETIME())
            OR (estado='Procesando' AND reserva_hasta<=SYSUTCDATETIME()));
        COMMIT;
        SELECT T.IdCorreo,T.Reserva,T.TokenCifrado,S.hash_token HashToken,@usuario IdUsuario,@nombre Nombre,@correo Correo,
            CAST(CASE WHEN T.Reserva=@reserva AND T.TokenCifrado IS NOT NULL THEN 1 ELSE 0 END AS BIT) Procesable
        FROM @tomado T INNER JOIN dbo.SOLICITUD_CONTRASENA S ON S.id_solicitud=@solicitud
        ;
    END TRY
    BEGIN CATCH IF XACT_STATE()<>0 ROLLBACK; THROW; END CATCH;
END;