CREATE FUNCTION dbo.EsSuperadministradorActivo(@usuario INT) RETURNS BIT AS
BEGIN
    IF EXISTS(SELECT 1 FROM dbo.USUARIO U INNER JOIN dbo.ESTADO E ON E.id_estado=U.id_estado
        WHERE U.id_usuario=@usuario AND U.es_plataforma=1 AND U.id_empresa IS NULL
        AND U.estado_aprobacion='Aprobado' AND LTRIM(RTRIM(E.nombre_estado))='Activo'
        AND EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU INNER JOIN dbo.ROL R ON R.id_rol=RU.id_rol
            INNER JOIN dbo.ESTADO EA ON EA.id_estado=RU.id_estado INNER JOIN dbo.ESTADO ER ON ER.id_estado=R.id_estado
            WHERE RU.id_usuario=U.id_usuario AND R.nombre='Superadministrador'
            AND LTRIM(RTRIM(EA.nombre_estado))='Activo' AND LTRIM(RTRIM(ER.nombre_estado))='Activo')
        AND NOT EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU INNER JOIN dbo.ROL R ON R.id_rol=RU.id_rol
            WHERE RU.id_usuario=U.id_usuario AND R.nombre<>'Superadministrador')) RETURN 1;
    RETURN 0;
END;