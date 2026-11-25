CREATE TRIGGER dbo.ValidarAmbitoBitacora ON dbo.BITACORA AFTER INSERT,UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS(SELECT 1 FROM inserted I JOIN dbo.USUARIO U ON U.id_usuario=I.id_usuario
        WHERE (I.id_empresa IS NULL AND U.es_plataforma=0) OR (I.id_empresa IS NOT NULL AND U.es_plataforma=1))
        THROW 51309,'Ambito de auditoria invalido.',1;
END;