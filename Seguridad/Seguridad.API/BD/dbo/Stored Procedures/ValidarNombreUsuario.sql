
CREATE   PROCEDURE dbo.ValidarNombreUsuario
    @nombre_usuario VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) FROM dbo.USUARIO WHERE nombre_usuario = @nombre_usuario;
END;