CREATE TRIGGER dbo.ProtegerUsuarioPlataforma ON dbo.USUARIO AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS(SELECT 1 FROM inserted I JOIN deleted D ON D.id_usuario=I.id_usuario
        WHERE I.es_plataforma<>D.es_plataforma OR I.id_empresa<>D.id_empresa
        OR (I.id_empresa IS NULL AND D.id_empresa IS NOT NULL) OR (I.id_empresa IS NOT NULL AND D.id_empresa IS NULL))
        THROW 51305,'No se permite transferir la identidad entre empresas o ambitos.',1;
    IF EXISTS(SELECT 1 FROM deleted D JOIN dbo.ESTADO E ON E.id_estado=D.id_estado
        WHERE D.es_plataforma=1 AND D.estado_aprobacion='Aprobado' AND LTRIM(RTRIM(E.nombre_estado))='Activo')
        AND NOT EXISTS(SELECT 1 FROM dbo.USUARIO WITH(UPDLOCK,HOLDLOCK) WHERE dbo.EsSuperadministradorActivo(id_usuario)=1)
        THROW 51304,'Debe conservarse un superadministrador activo.',1;
    DECLARE @empresa INT;
    DECLARE empresas CURSOR LOCAL FAST_FORWARD FOR
        SELECT DISTINCT D.id_empresa FROM deleted D JOIN dbo.ESTADO E ON E.id_estado=D.id_estado
        WHERE D.id_empresa IS NOT NULL AND D.estado_aprobacion='Aprobado' AND LTRIM(RTRIM(E.nombre_estado))='Activo'
        AND EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU JOIN dbo.ROL R ON R.id_rol=RU.id_rol JOIN dbo.ESTADO ER ON ER.id_estado=R.id_estado JOIN dbo.ESTADO S ON S.id_estado=RU.id_estado
            WHERE RU.id_usuario=D.id_usuario AND R.nombre='Administrador' AND LTRIM(RTRIM(ER.nombre_estado))='Activo' AND LTRIM(RTRIM(S.nombre_estado))='Activo') ORDER BY D.id_empresa;
    OPEN empresas; FETCH NEXT FROM empresas INTO @empresa;
    WHILE @@FETCH_STATUS=0
    BEGIN
        EXEC dbo.ProtegerAdministradorEmpresa @empresa;
        IF EXISTS(SELECT 1 FROM dbo.EMPRESA E JOIN dbo.ESTADO S ON S.id_estado=E.id_estado WHERE E.id_empresa=@empresa AND LTRIM(RTRIM(S.nombre_estado))='Activo')
            AND NOT EXISTS(SELECT 1 FROM dbo.USUARIO U WITH(UPDLOCK,HOLDLOCK) WHERE U.id_empresa=@empresa AND dbo.EsAdministradorActivo(U.id_usuario,@empresa)=1)
            THROW 51303,'La empresa activa debe conservar un administrador habilitado.',1;
        FETCH NEXT FROM empresas INTO @empresa;
    END;
    CLOSE empresas; DEALLOCATE empresas;
END;