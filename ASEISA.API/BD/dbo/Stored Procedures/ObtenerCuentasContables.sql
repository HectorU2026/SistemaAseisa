CREATE   PROCEDURE dbo.ObtenerCuentasContables
    @id_empresa INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        cc.id_cuenta_contable AS IdCuentaContable,
        cc.codigo + ' - ' + cc.nombre AS NombreCompleto
    FROM CUENTA_CONTABLE cc
    INNER JOIN ESTADO e ON e.id_estado = cc.id_estado
    WHERE cc.id_empresa = @id_empresa
      AND e.nombre_estado = 'Activo'
    ORDER BY cc.codigo;
END;