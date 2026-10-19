CREATE PROCEDURE [dbo].[CambiarContrasena]
@correo VARCHAR(50),
@contrasena VARCHAR(500)
AS
BEGIN 
	SET NOCOUNT ON

	UPDATE USUARIO
	SET contrasena_hash = @contrasena, fecha_modificacion = SYSDATETIME()
	WHERE correo = @correo;

END;