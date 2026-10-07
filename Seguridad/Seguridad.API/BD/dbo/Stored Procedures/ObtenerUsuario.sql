CREATE PROCEDURE [dbo].[ObtenerUsuario]
    @correo VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.id_usuario       AS IdUsuario,
        u.id_empresa       AS IdEmpresa,
        u.tipo_cliente     AS TipoCliente,
        u.nombre           AS Nombre,
        u.primer_apellido  AS PrimerApellido,
        u.segundo_apellido AS SegundoApellido,
        u.correo           AS Correo,
        u.nombre_usuario   AS NombreUsuario,
        u.contrasena_hash  AS ContrasenaHash,
        u.ultimo_acceso    AS UltimoAcceso,
        u.id_estado        AS IdEstado,
        r.nombre           AS Rol,
        u.fecha_creacion   AS FechaCreacion,
        u.fecha_modificacion AS FechaModificacion
    FROM USUARIO u
    INNER JOIN ROL_USUARIO ru ON ru.id_usuario = u.id_usuario AND ru.id_estado = 1
    INNER JOIN ROL r          ON r.id_rol = ru.id_rol       AND r.id_estado = 1
    WHERE u.correo = @correo;
END;