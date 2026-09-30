CREATE PROCEDURE CambiarContrasena
@correo VARCHAR(50),
@contrasena VARCHAR(50)
AS
BEGIN 
	SET NOCOUNT ON

	UPDATE USUARIO
	SET contrasena_hash = @contrasena
	WHERE correo = @correo;

END;