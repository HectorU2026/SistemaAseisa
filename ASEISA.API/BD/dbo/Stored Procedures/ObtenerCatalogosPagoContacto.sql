CREATE   PROCEDURE dbo.ObtenerCatalogosPagoContacto
    @id_empresa INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Dos conjuntos de resultados: usar QueryMultipleAsync en DA.
    SELECT id_termino_pago AS IdTerminoPago, nombre AS Nombre, dias_credito AS DiasCredito
    FROM dbo.TERMINO_PAGO WHERE id_empresa = @id_empresa AND activo = 1 ORDER BY nombre;
    SELECT id_metodo_pago AS IdMetodoPago, nombre AS Nombre
    FROM dbo.METODO_PAGO WHERE id_empresa = @id_empresa AND activo = 1 ORDER BY nombre;
END;