CREATE PROCEDURE dbo.ProtegerAdministradorEmpresa @empresa INT AS
BEGIN
    SET NOCOUNT ON;
    -- Se ejecuta dentro de la transaccion de la operacion, siempre antes de tocar al usuario.
    IF @@TRANCOUNT=0 THROW 51300,'Transaccion requerida.',1;
    DECLARE @recurso NVARCHAR(255)=CONCAT(N'ASEISA:Administradores:',@empresa),@resultado INT;
    EXEC @resultado=sys.sp_getapplock @Resource=@recurso,@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=8000;
    IF @resultado<0 THROW 51301,'Operacion temporalmente no disponible.',1;
END;