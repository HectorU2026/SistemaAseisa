CREATE PROCEDURE dbo.RetirarRolUsuario @id_empresa INT,@id_usuario INT,@id_rol INT,@id_usuario_accion INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.GestionarRolUsuario @id_empresa,@id_usuario,@id_rol,@id_usuario_accion,0;
END;