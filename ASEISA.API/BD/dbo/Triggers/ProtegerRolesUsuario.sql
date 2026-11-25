CREATE TRIGGER dbo.ProtegerRolesUsuario ON dbo.ROL_USUARIO AFTER INSERT,UPDATE,DELETE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS(SELECT 1 FROM inserted I JOIN dbo.USUARIO U ON U.id_usuario=I.id_usuario JOIN dbo.ROL R ON R.id_rol=I.id_rol
        WHERE (U.es_plataforma=1 AND R.nombre<>'Superadministrador') OR (U.es_plataforma=0 AND R.nombre='Superadministrador'))
        THROW 51302,'El rol de plataforma es exclusivo.',1;
    DECLARE @empresas TABLE(id INT PRIMARY KEY);
    INSERT @empresas SELECT DISTINCT U.id_empresa FROM deleted D JOIN dbo.ROL R ON R.id_rol=D.id_rol
        JOIN dbo.ESTADO ER ON ER.id_estado=R.id_estado JOIN dbo.ESTADO EA ON EA.id_estado=D.id_estado JOIN dbo.USUARIO U ON U.id_usuario=D.id_usuario
        JOIN dbo.ESTADO EU ON EU.id_estado=U.id_estado
        WHERE R.nombre='Administrador' AND LTRIM(RTRIM(ER.nombre_estado))='Activo' AND LTRIM(RTRIM(EA.nombre_estado))='Activo'
        AND LTRIM(RTRIM(EU.nombre_estado))='Activo' AND U.estado_aprobacion='Aprobado' AND U.id_empresa IS NOT NULL;
    DECLARE @empresa INT;
    DECLARE empresas CURSOR LOCAL FAST_FORWARD FOR SELECT id FROM @empresas ORDER BY id;
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
    IF EXISTS(SELECT 1 FROM deleted D JOIN dbo.USUARIO U ON U.id_usuario=D.id_usuario WHERE U.es_plataforma=1)
       AND NOT EXISTS(SELECT 1 FROM dbo.USUARIO WHERE dbo.EsSuperadministradorActivo(id_usuario)=1)
        THROW 51304,'Debe conservarse un superadministrador activo.',1;
END;