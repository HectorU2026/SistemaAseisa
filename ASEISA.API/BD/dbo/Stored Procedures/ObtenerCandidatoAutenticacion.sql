CREATE PROCEDURE dbo.ObtenerCandidatoAutenticacion @correo VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    -- Exclusivo para verificar credenciales; nunca devolver este contrato por HTTP.
    SELECT U.id_usuario IdUsuario,U.id_empresa IdEmpresa,U.correo Correo,U.nombre_usuario NombreUsuario,
        U.contrasena_hash ContrasenaHash,U.version_credencial VersionCredencial,U.estado_aprobacion EstadoAprobacion,
        CONVERT(BIT,CASE WHEN LTRIM(RTRIM(E.nombre_estado))='Activo' THEN 1 ELSE 0 END) UsuarioActivo,
        CONVERT(BIT,CASE WHEN EXISTS(SELECT 1 FROM dbo.EMPRESA EM INNER JOIN dbo.ESTADO EE ON EE.id_estado=EM.id_estado
            WHERE EM.id_empresa=U.id_empresa AND LTRIM(RTRIM(EE.nombre_estado))='Activo') THEN 1 ELSE 0 END) EmpresaActiva,
        Roles.Nombre Rol
    FROM dbo.USUARIO U
    LEFT JOIN dbo.ESTADO E ON E.id_estado=U.id_estado
    OUTER APPLY(SELECT DISTINCT R.nombre Nombre FROM dbo.ROL_USUARIO RU
        INNER JOIN dbo.ESTADO EA ON EA.id_estado=RU.id_estado
        INNER JOIN dbo.ROL R ON R.id_rol=RU.id_rol
        INNER JOIN dbo.ESTADO ER ON ER.id_estado=R.id_estado
        WHERE RU.id_usuario=U.id_usuario AND LTRIM(RTRIM(EA.nombre_estado))='Activo'
            AND LTRIM(RTRIM(ER.nombre_estado))='Activo') Roles
    WHERE U.correo=@correo;
END;