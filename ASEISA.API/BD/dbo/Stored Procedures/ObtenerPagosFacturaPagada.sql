CREATE PROCEDURE dbo.ObtenerPagosFacturaPagada
    @id_empresa INT,
    @id_factura INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_venta INT;

    SELECT @id_venta = f.id_venta
    FROM dbo.FACTURA f
    INNER JOIN dbo.ESTADO_FACTURA ef
        ON ef.id_estado_factura = f.id_estado_factura
    WHERE f.id_factura = @id_factura
      AND f.id_empresa = @id_empresa
      AND ef.nombre_estado = N'Pagada';

    IF @id_venta IS NULL
        RETURN;

    IF (
            SELECT COUNT(*)
            FROM dbo.FACTURA
            WHERE id_venta = @id_venta
        ) <> 1
        RETURN;

    IF (
            SELECT COUNT(*)
            FROM dbo.CUENTA_POR_COBRAR
            WHERE id_venta = @id_venta
        ) <> 1
        RETURN;

    SELECT
        p.id_pago_cliente      AS IdPagoCliente,
        p.fecha_pago           AS FechaPago,
        p.monto                AS Monto,
        p.metodo_pago          AS MetodoPago,
        p.referencia           AS Referencia,
        e.nombre_estado        AS Estado
    FROM dbo.PAGO_CLIENTE p
    INNER JOIN dbo.ESTADO e
        ON e.id_estado = p.id_estado
       AND e.nombre_estado = N'Activo'
    INNER JOIN dbo.CUENTA_POR_COBRAR cxc
        ON cxc.id_cuenta_cobrar = p.id_cuenta_cobrar
       AND cxc.id_venta = @id_venta
    ORDER BY
        p.fecha_pago ASC,
        p.id_pago_cliente ASC;
END;
