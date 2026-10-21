
CREATE   PROCEDURE dbo.ValidarCorreo
    @correo VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) FROM dbo.USUARIO WHERE correo = @correo;
END;