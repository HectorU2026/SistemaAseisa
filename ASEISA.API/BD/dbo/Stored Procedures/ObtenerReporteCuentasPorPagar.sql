
CREATE   PROCEDURE [dbo].[ObtenerReporteCuentasPorPagar]
    @id_empresa        INT,
    @id_proveedor      INT = NULL,
    @solo_vencidas     BIT = 0,
    @fecha_corte       DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @fecha_corte = ISNULL(@fecha_corte, CAST(GETDATE() AS DATE));

    SELECT 
    p.id_proveedor AS IdProveedor,
    p.nombre_razon_social AS Proveedor,
    cpp.id_cuenta_pagar AS IdCuentaPagar,
    oc.numero_orden AS NumeroFacturaCompra,
    cpp.fecha_emision AS FechaEmision,
    cpp.fecha_vencimiento AS FechaVencimiento,
    cpp.monto_original AS MontoTotal,
    (cpp.monto_original - cpp.saldo_pendiente) AS PagosAplicados,
    cpp.saldo_pendiente AS SaldoPendiente,
    CASE 
        WHEN cpp.saldo_pendiente = 0 THEN 'Pagada'
        WHEN cpp.fecha_vencimiento < @fecha_corte THEN 'Vencida'
        ELSE 'Por Vencer'
    END AS EstadoVencimiento,
    CASE 
        WHEN cpp.fecha_vencimiento < @fecha_corte AND cpp.saldo_pendiente > 0 
        THEN DATEDIFF(DAY, cpp.fecha_vencimiento, @fecha_corte)
        ELSE 0
    END AS DiasAtraso
    FROM dbo.CUENTA_POR_PAGAR cpp
    INNER JOIN dbo.PROVEEDOR p ON p.id_proveedor = cpp.id_proveedor
    INNER JOIN dbo.ORDEN_COMPRA oc ON oc.id_orden_compra = cpp.id_orden_compra
    WHERE cpp.id_empresa = @id_empresa
      AND cpp.saldo_pendiente > 0
      AND (@id_proveedor IS NULL OR cpp.id_proveedor = @id_proveedor)
      AND (@solo_vencidas = 0 OR cpp.fecha_vencimiento < @fecha_corte)
    ORDER BY p.nombre_razon_social, cpp.fecha_vencimiento;

    IF NOT EXISTS (
        SELECT 1 
        FROM dbo.CUENTA_POR_PAGAR 
        WHERE id_empresa = @id_empresa AND saldo_pendiente > 0
    )
    BEGIN
        SELECT 
            'No hay cuentas por pagar registradas para los filtros seleccionados' AS Mensaje;
    END;
END;