CREATE TRIGGER dbo.ProtegerRolPlataforma ON dbo.ROL AFTER UPDATE,DELETE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS(SELECT 1 FROM inserted I JOIN dbo.ROL_USUARIO RU ON RU.id_rol=I.id_rol JOIN dbo.USUARIO U ON U.id_usuario=RU.id_usuario
        WHERE (U.es_plataforma=1 AND I.nombre<>'Superadministrador') OR (U.es_plataforma=0 AND I.nombre='Superadministrador'))
        THROW 51302,'No se permite cambiar el ambito de roles asignados.',1;
    DECLARE @empresa INT;
    DECLARE empresas CURSOR LOCAL FAST_FORWARD FOR
        SELECT DISTINCT U.id_empresa FROM deleted D JOIN dbo.ESTADO ER ON ER.id_estado=D.id_estado
        JOIN dbo.ROL_USUARIO RU ON RU.id_rol=D.id_rol JOIN dbo.ESTADO EA ON EA.id_estado=RU.id_estado
        JOIN dbo.USUARIO U ON U.id_usuario=RU.id_usuario JOIN dbo.ESTADO EU ON EU.id_estado=U.id_estado
        WHERE D.nombre='Administrador' AND LTRIM(RTRIM(ER.nombre_estado))='Activo' AND LTRIM(RTRIM(EA.nombre_estado))='Activo'
          AND LTRIM(RTRIM(EU.nombre_estado))='Activo' AND U.estado_aprobacion='Aprobado' AND U.id_empresa IS NOT NULL ORDER BY U.id_empresa;
    OPEN empresas;FETCH NEXT FROM empresas INTO @empresa;
    WHILE @@FETCH_STATUS=0
    BEGIN
        EXEC dbo.ProtegerAdministradorEmpresa @empresa;
        IF EXISTS(SELECT 1 FROM dbo.EMPRESA E JOIN dbo.ESTADO S ON S.id_estado=E.id_estado WHERE E.id_empresa=@empresa AND LTRIM(RTRIM(S.nombre_estado))='Activo')
          AND NOT EXISTS(SELECT 1 FROM dbo.USUARIO U WITH(UPDLOCK,HOLDLOCK) WHERE U.id_empresa=@empresa AND dbo.EsAdministradorActivo(U.id_usuario,@empresa)=1)
            THROW 51303,'La empresa activa debe conservar un administrador habilitado.',1;
        FETCH NEXT FROM empresas INTO @empresa;
    END;
    CLOSE empresas;DEALLOCATE empresas;
    IF EXISTS(SELECT 1 FROM deleted WHERE nombre='Superadministrador')
        AND EXISTS(SELECT 1 FROM dbo.USUARIO WHERE es_plataforma=1)
        AND NOT EXISTS(SELECT 1 FROM dbo.USUARIO WITH(UPDLOCK,HOLDLOCK) WHERE dbo.EsSuperadministradorActivo(id_usuario)=1)
        THROW 51304,'Debe conservarse un superadministrador activo.',1;
END;