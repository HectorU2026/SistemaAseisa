CREATE PROCEDURE [dbo].[ObtenerUsuario]
    @correo VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    -- Contrato interno de autenticacion: no devolver este resultado directamente por HTTP.
    SELECT
        u.version_credencial AS VersionCredencial,
        u.id_usuario AS IdUsuario,
        u.id_empresa AS IdEmpresa,
        u.tipo_cliente AS TipoCliente,
        u.nombre AS Nombre,
        u.primer_apellido AS PrimerApellido,
        u.segundo_apellido AS SegundoApellido,
        u.correo AS Correo,
        u.nombre_usuario AS NombreUsuario,
        u.contrasena_hash AS ContrasenaHash,
        u.ultimo_acceso AS UltimoAcceso,
        u.id_estado AS IdEstado,
        r.nombre AS Rol,
        u.fecha_creacion AS FechaCreacion,
        u.fecha_modificacion AS FechaModificacion
    FROM dbo.USUARIO u
    INNER JOIN dbo.ESTADO estadoUsuario
        ON estadoUsuario.id_estado = u.id_estado
       AND LTRIM(RTRIM(estadoUsuario.nombre_estado)) = 'Activo'
    INNER JOIN dbo.ROL_USUARIO ru
        ON ru.id_usuario = u.id_usuario
    INNER JOIN dbo.ESTADO estadoAsignacion
        ON estadoAsignacion.id_estado = ru.id_estado
       AND LTRIM(RTRIM(estadoAsignacion.nombre_estado)) = 'Activo'
    INNER JOIN dbo.ROL r
        ON r.id_rol = ru.id_rol
    INNER JOIN dbo.ESTADO estadoRol
        ON estadoRol.id_estado = r.id_estado
       AND LTRIM(RTRIM(estadoRol.nombre_estado)) = 'Activo'
    WHERE u.correo = @correo
      AND u.estado_aprobacion = 'Aprobado'
      AND EXISTS(SELECT 1 FROM dbo.EMPRESA E INNER JOIN dbo.ESTADO EE ON EE.id_estado=E.id_estado
          WHERE E.id_empresa=u.id_empresa AND LTRIM(RTRIM(EE.nombre_estado))='Activo')
    ORDER BY r.id_rol;
END;
