CREATE PROCEDURE dbo.AsignarRolUsuario @id_empresa INT,@id_usuario INT,@id_rol INT,@id_usuario_accion INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Compatibilidad: asignar ahora agrega sin retirar las demas asignaciones.
    EXEC dbo.GestionarRolUsuario @id_empresa,@id_usuario,@id_rol,@id_usuario_accion,1;
END;