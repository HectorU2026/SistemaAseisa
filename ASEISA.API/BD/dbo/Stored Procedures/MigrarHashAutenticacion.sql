CREATE PROCEDURE dbo.MigrarHashAutenticacion
    @id_usuario INT,@version_credencial BIGINT,@hash_anterior VARCHAR(500),@hash_nuevo VARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    IF NULLIF(LTRIM(RTRIM(@hash_nuevo)),'') IS NULL THROW 51200,'Solicitud no valida.',1;
    UPDATE dbo.USUARIO SET contrasena_hash=@hash_nuevo,fecha_modificacion=SYSDATETIME()
    WHERE id_usuario=@id_usuario AND version_credencial=@version_credencial
      AND CONVERT(VARBINARY(500),contrasena_hash)=CONVERT(VARBINARY(500),@hash_anterior);
    SELECT CAST(CASE WHEN @@ROWCOUNT=1 THEN 1 ELSE 0 END AS BIT);
END;