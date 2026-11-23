CREATE PROCEDURE dbo.ObtenerEstadosFactura
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_estado_factura AS IdEstadoFactura,
        nombre_estado AS NombreEstado
    FROM dbo.ESTADO_FACTURA
    ORDER BY nombre_estado;
END;
