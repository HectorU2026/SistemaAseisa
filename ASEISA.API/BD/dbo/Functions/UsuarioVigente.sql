CREATE FUNCTION dbo.UsuarioVigente(@usuario INT) RETURNS BIT AS
BEGIN
    IF dbo.EsSuperadministradorActivo(@usuario)=1 RETURN 1;
    IF EXISTS(SELECT 1 FROM dbo.USUARIO U INNER JOIN dbo.ESTADO EU ON EU.id_estado=U.id_estado
        INNER JOIN dbo.EMPRESA E ON E.id_empresa=U.id_empresa INNER JOIN dbo.ESTADO EE ON EE.id_estado=E.id_estado
        WHERE U.id_usuario=@usuario AND U.es_plataforma=0 AND U.estado_aprobacion='Aprobado'
        AND LTRIM(RTRIM(EU.nombre_estado))='Activo' AND LTRIM(RTRIM(EE.nombre_estado))='Activo'
        AND NOT EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU INNER JOIN dbo.ROL R ON R.id_rol=RU.id_rol WHERE RU.id_usuario=U.id_usuario AND R.nombre='Superadministrador')) RETURN 1;
    RETURN 0;
END;