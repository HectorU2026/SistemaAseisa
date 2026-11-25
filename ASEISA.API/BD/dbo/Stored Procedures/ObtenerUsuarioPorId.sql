CREATE PROCEDURE dbo.ObtenerUsuarioPorId
    @id_empresa INT,
    @id_usuario INT
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
        U.estado_aprobacion AS EstadoAprobacion,
        TRY_CONVERT(INT, JSON_VALUE(Roles.RolesJson, '$[0].IdRol')) AS IdRol,
        JSON_VALUE(Roles.RolesJson, '$[0].NombreRol') AS NombreRol,
        Roles.RolesJson
    FROM dbo.USUARIO U
    OUTER APPLY (
        SELECT (
            SELECT R.id_rol AS IdRol, R.nombre AS NombreRol
            FROM dbo.ROL_USUARIO RU
            INNER JOIN dbo.ROL R ON R.id_rol = RU.id_rol AND R.id_estado IN (SELECT id_estado FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado))='Activo')
            WHERE RU.id_usuario = U.id_usuario
              AND RU.id_estado IN (SELECT id_estado FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado))='Activo')
            -- El primer rol mantiene los campos escalares por compatibilidad.
            ORDER BY RU.fecha_asignacion DESC, R.id_rol ASC
            FOR JSON PATH
        ) AS RolesJson
    ) Roles
    WHERE U.id_empresa = @id_empresa
      AND U.id_usuario = @id_usuario;
END;
