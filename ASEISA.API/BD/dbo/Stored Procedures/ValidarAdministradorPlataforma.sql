CREATE PROCEDURE dbo.ValidarAdministradorPlataforma @actor INT,@empresa INT,@usuario INT=NULL AS
BEGIN
    SET NOCOUNT ON;
    IF dbo.EsSuperadministradorActivo(@actor)=0 THROW 51025,'Operacion de plataforma no autorizada.',1;
    IF NOT EXISTS(SELECT 1 FROM dbo.EMPRESA E JOIN dbo.ESTADO S ON S.id_estado=E.id_estado WHERE E.id_empresa=@empresa AND LTRIM(RTRIM(S.nombre_estado))='Activo') THROW 51015,'Empresa no disponible.',1;
    IF @usuario IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.USUARIO U JOIN dbo.ROL_USUARIO RU ON RU.id_usuario=U.id_usuario
        JOIN dbo.ROL R ON R.id_rol=RU.id_rol JOIN dbo.ESTADO EA ON EA.id_estado=RU.id_estado JOIN dbo.ESTADO ER ON ER.id_estado=R.id_estado
        WHERE U.id_usuario=@usuario AND U.id_empresa=@empresa AND U.es_plataforma=0 AND R.nombre='Administrador' AND LTRIM(RTRIM(EA.nombre_estado))='Activo' AND LTRIM(RTRIM(ER.nombre_estado))='Activo') THROW 51015,'Administrador no disponible.',1;
    IF (SELECT COUNT(*) FROM dbo.ROL R JOIN dbo.ESTADO E ON E.id_estado=R.id_estado WHERE R.nombre='Administrador' AND LTRIM(RTRIM(E.nombre_estado))='Activo')<>1 THROW 51020,'Catalogo de administradores ambiguo.',1;
    SELECT R.id_rol IdRol,E.id_estado IdEstado FROM dbo.ROL R JOIN dbo.ESTADO E ON E.id_estado=R.id_estado WHERE R.nombre='Administrador' AND LTRIM(RTRIM(E.nombre_estado))='Activo';
END;