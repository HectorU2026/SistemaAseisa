CREATE   PROCEDURE dbo.ObtenerCuentasPorPagar
    @id_empresa INT,
    @busqueda VARCHAR(150) = NULL,
    @id_estado INT = NULL,
    @fecha_vencimiento DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        cxp.id_cuenta_pagar AS IdCuentaPagar,
        oc.numero_orden     AS ConsecutivoFactura,
        prov.nombre_razon_social AS Proveedor,
        cxp.fecha_emision   AS FechaEmision,
        cxp.fecha_vencimiento AS FechaVencimiento,
        cxp.monto_original  AS MontoTotal,
        cxp.saldo_pendiente AS SaldoPendiente,
        e.nombre_estado     AS Estado
    FROM CUENTA_POR_PAGAR cxp
    INNER JOIN ORDEN_COMPRA oc ON oc.id_orden_compra = cxp.id_orden_compra
    INNER JOIN PROVEEDOR prov ON prov.id_proveedor = cxp.id_proveedor
    INNER JOIN ESTADO e ON e.id_estado = cxp.id_estado
    WHERE cxp.id_empresa = @id_empresa

      AND (
          @busqueda IS NULL 
          OR oc.numero_orden LIKE '%' + @busqueda + '%'
          OR prov.nombre_razon_social LIKE '%' + @busqueda + '%'
      )
      
      AND (
          @id_estado IS NULL 
          OR cxp.id_estado = @id_estado
      )

      AND (
          @fecha_vencimiento IS NULL 
          OR CAST(cxp.fecha_vencimiento AS DATE) = @fecha_vencimiento
      )
    ORDER BY cxp.fecha_vencimiento ASC;
END;