CREATE PROCEDURE [dbo].ValidarCorreoActivo
	@correo VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT correo 
	FROM USUARIO
	WHERE correo = @correo 
	AND id_estado = 1
END;