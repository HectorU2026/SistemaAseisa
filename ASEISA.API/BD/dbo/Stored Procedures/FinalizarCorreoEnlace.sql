CREATE PROCEDURE dbo.FinalizarCorreoEnlace @id_correo BIGINT,@reserva UNIQUEIDENTIFIER,@enviado BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.CORREO_ENLACE
    SET estado=CASE WHEN @enviado=1 THEN 'Enviado' WHEN intentos>=5 THEN 'Fallido' ELSE 'Pendiente' END,
        fecha_envio=CASE WHEN @enviado=1 THEN SYSUTCDATETIME() ELSE NULL END,
        token_cifrado=CASE WHEN @enviado=1 OR intentos>=5 THEN NULL ELSE token_cifrado END,
        proximo_intento=DATEADD(SECOND,CASE WHEN intentos>=4 THEN 300 ELSE 30*CONVERT(INT,POWER(2,intentos-1)) END,SYSUTCDATETIME()),
        reserva=NULL,reserva_hasta=NULL
    WHERE id_correo=@id_correo AND reserva=@reserva AND estado='Procesando';
END;