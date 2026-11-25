CREATE PROCEDURE dbo.CambiarEstadoEmpresaPlataforma @actor INT,@empresa INT,@activo BIT AS
BEGIN
    SET NOCOUNT ON;SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF dbo.EsSuperadministradorActivo(@actor)=0 THROW 51025,'Operacion de plataforma no autorizada.',1;
        IF @activo IS NULL THROW 51307,'Estado requerido.',1;
        DECLARE @estado INT=(SELECT MIN(id_estado) FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado))=CASE WHEN @activo=1 THEN 'Activo' ELSE 'Inactivo' END);
        IF @estado IS NULL THROW 51020,'Catalogo de estados incompleto.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.EMPRESA WITH(UPDLOCK,HOLDLOCK) WHERE id_empresa=@empresa) THROW 51015,'Empresa no disponible.',1;
        UPDATE dbo.EMPRESA SET id_estado=@estado,fecha_modificacion=SYSDATETIME() WHERE id_empresa=@empresa AND id_estado<>@estado;
        IF @@ROWCOUNT>0 INSERT dbo.BITACORA(id_usuario,id_empresa,id_empresa_objetivo,tabla_afectada,registro_id,accion)
            VALUES(@actor,NULL,@empresa,'EMPRESA',CONVERT(VARCHAR(100),@empresa),CASE WHEN @activo=1 THEN 'ACTIVAR_EMPRESA' ELSE 'DESACTIVAR_EMPRESA' END);
        COMMIT;SELECT @empresa;
    END TRY
    BEGIN CATCH IF XACT_STATE()<>0 ROLLBACK;THROW;END CATCH;
END;