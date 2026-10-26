CREATE PROCEDURE dbo.ObtenerFacturas
    @id_empresa INT,
    @id_estado_factura INT = NULL,
    @nombre_cliente VARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @nombre_cliente = NULLIF(LTRIM(RTRIM(@nombre_cliente)), '');

    SELECT
        f.id_factura           AS IdFactura,
        f.numero_factura       AS NumeroFactura,
        cl.nombre_razon_social AS NombreCliente,
        v.condicion_venta      AS CondicionVenta,
        f.id_estado_factura    AS IdEstadoFactura,
        ef.nombre_estado       AS Estado,
        f.fecha_emision        AS FechaEmision,
        f.fecha_vencimiento    AS FechaVencimiento,
        f.id_termino_pago      AS IdTerminoPago,
        f.termino_pago_nombre  AS TerminoPagoNombre,
        f.total                AS Total
    FROM dbo.FACTURA f
    INNER JOIN dbo.VENTAS v
        ON v.id_venta = f.id_venta
       AND v.id_empresa = f.id_empresa
    INNER JOIN dbo.CLIENTE cl
        ON cl.id_cliente = v.id_cliente
       AND cl.id_empresa = f.id_empresa
    INNER JOIN dbo.ESTADO_FACTURA ef
        ON ef.id_estado_factura = f.id_estado_factura
    WHERE f.id_empresa = @id_empresa
      AND (@id_estado_factura IS NULL OR f.id_estado_factura = @id_estado_factura)
      AND (@nombre_cliente IS NULL OR CHARINDEX(@nombre_cliente, cl.nombre_razon_social) > 0)
    ORDER BY f.fecha_emision DESC;
END;
