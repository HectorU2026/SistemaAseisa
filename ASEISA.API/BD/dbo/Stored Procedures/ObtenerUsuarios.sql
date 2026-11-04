CREATE   PROCEDURE dbo.ObtenerUsuarios
    @id_empresa INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.id_usuario AS IdUsuario,
        U.id_empresa AS IdEmpresa,
        U.tipo_cliente AS TipoCliente,
        U.nombre AS Nombre,
        U.primer_apellido AS PrimerApellido,
        U.segundo_apellido AS SegundoApellido,
        U.correo AS Correo,
        U.nombre_usuario AS NombreUsuario,
        U.ultimo_acceso AS UltimoAcceso,
        U.id_estado AS IdEstado,
        U.fecha_creacion AS FechaCreacion,
        U.fecha_modificacion AS FechaModificacion,
        R.id_rol AS IdRol,
        R.nombre AS NombreRol
    FROM dbo.USUARIO U
    LEFT JOIN dbo.ROL_USUARIO RU
        ON RU.id_usuario = U.id_usuario
        AND RU.id_estado = 1
    LEFT JOIN dbo.ROL R
        ON R.id_rol = RU.id_rol
    WHERE U.id_empresa = @id_empresa
    ORDER BY U.nombre, U.primer_apellido;
END;