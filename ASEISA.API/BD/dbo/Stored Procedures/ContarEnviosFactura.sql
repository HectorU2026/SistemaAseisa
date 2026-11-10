CREATE PROCEDURE dbo.ContarEnviosFactura
    @id_factura INT,
    @id_empresa INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1)
    FROM dbo.BITACORA
    WHERE id_empresa = @id_empresa
      AND tabla_afectada = 'FACTURA'
      AND accion = 'ENVIAR'
      AND registro_id = CONVERT(VARCHAR(100), @id_factura);
END;
