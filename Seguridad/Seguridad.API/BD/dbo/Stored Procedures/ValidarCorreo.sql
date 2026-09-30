CREATE   PROCEDURE ValidarCorreo 
@correo VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT correo, nombre_usuario AS Nombre
		FROM USUARIO
		WHERE correo = @correo
END;