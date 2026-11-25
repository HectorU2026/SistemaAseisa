CREATE PROCEDURE [dbo].[CambiarContrasena]
@correo VARCHAR(150),
@contrasena VARCHAR(500)
AS
BEGIN 
	SET NOCOUNT ON

	UPDATE USUARIO
	SET contrasena_hash = @contrasena, version_credencial=version_credencial+1, fecha_modificacion = SYSDATETIME()
	WHERE correo = @correo;

END;