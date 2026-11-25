CREATE PROCEDURE dbo.ObtenerNotasCreditoReporteFacturacion
    @id_empresa INT,
    @id_factura INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        nc.id_nota_credito        AS IdNotaCredito,
        nc.numero_nota_credito    AS NumeroNotaCredito,
        nc.fecha_emision          AS FechaEmision,
        nc.motivo                 AS Motivo,
        nc.monto                  AS Monto,
        nc.descripcion_adicional  AS DescripcionAdicional,
        e.nombre_estado           AS Estado,
        nc.id_factura             AS IdFactura
    FROM dbo.NOTA_CREDITO nc
    INNER JOIN dbo.ESTADO e
        ON e.id_estado = nc.id_estado
       AND e.nombre_estado = N'Activo'
    INNER JOIN dbo.FACTURA f
        ON f.id_factura = nc.id_factura
       AND f.id_factura = @id_factura
       AND f.id_empresa = @id_empresa
    INNER JOIN dbo.ESTADO_FACTURA ef
        ON ef.id_estado_factura = f.id_estado_factura
       AND ef.nombre_estado IN (N'Emitida', N'Pagada')
    ORDER BY
        nc.fecha_emision ASC,
        nc.id_nota_credito ASC;
END;
