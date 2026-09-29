
CREATE PROCEDURE [dbo].[ObtenerUsuario]
@correo varchar(50)

AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.id_usuario as IdUsuario,
        u.nombre_usuario AS NombreUsuario,
        u.correo AS Correo, 
        u.contrasena_hash AS ContrasenaHash,
        r.nombre AS Rol
    FROM USUARIO u
    INNER JOIN ROL_USUARIO ru ON ru.id_usuario = u.id_usuario AND ru.id_estado = 1
    INNER JOIN ROL r ON r.id_rol = ru.id_rol AND r.id_estado = 1
    WHERE u.correo = @correo;
END;