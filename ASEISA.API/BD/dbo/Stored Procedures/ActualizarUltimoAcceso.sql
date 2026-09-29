CREATE PROCEDURE ActualizarUltimoAcceso
	@correo VARCHAR(50)
AS
BEGIN 
	SET NOCOUNT ON

	UPDATE USUARIO
	SET ultimo_acceso = SYSDATETIME()
	WHERE correo = @correo;

END;