
CREATE   PROCEDURE dbo.ValidarInventarioCotizacion
    @id_cotizacion INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_empresa INT;
    DECLARE @id_bodega INT;

    SELECT @id_empresa = id_empresa FROM COTIZACION WHERE id_cotizacion = @id_cotizacion;
    SELECT TOP 1 @id_bodega = id_bodega FROM BODEGA WHERE id_empresa = @id_empresa;

    SELECT 
        p.nombre AS Producto,
        d.cantidad AS CantidadRequerida,
        ISNULL(e.cantidad_disponible, 0) AS CantidadDisponible,
        (ISNULL(e.cantidad_disponible, 0) - d.cantidad) AS Faltante
    FROM DETALLE_COTIZACION d
    INNER JOIN PRODUCTO p ON p.id_producto = d.id_producto
    LEFT JOIN EXISTENCIA e ON e.id_producto = d.id_producto AND e.id_bodega = @id_bodega
    WHERE d.id_cotizacion = @id_cotizacion 
      AND p.inventariable = 1
      AND ISNULL(e.cantidad_disponible, 0) < d.cantidad;
END;