CREATE PROCEDURE dbo.RestablecerContrasenaUsuario
    @id_empresa INT,@id_usuario INT,@contrasena_hash VARCHAR(500),@id_usuario_accion INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Se conserva la firma SQL para diagnosticar consumidores antiguos, sin cambiar credenciales.
    THROW 51201,'El restablecimiento administrativo requiere un enlace de un solo uso.',1;
END;