CREATE PROCEDURE InformarionUsurio
    @nombre_usuario varchar(100),
    @hash varchar(500),
    @estado_activo int

AS 
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.id_usuario,
        u.nombre,
        u.primer_apellido,
        u.correo,
        u.nombre_usuario,
        u.id_empresa,
        r.id_rol,
        r.nombre AS nombre_rol
    FROM USUARIO u
    INNER JOIN ROL_USUARIO ru ON ru.id_usuario = u.id_usuario
    INNER JOIN ROL r ON r.id_rol = ru.id_rol
    WHERE u.nombre_usuario = @nombre_usuario
      AND u.contrasena_hash = @hash
      AND u.id_estado = @estado_activo
END;