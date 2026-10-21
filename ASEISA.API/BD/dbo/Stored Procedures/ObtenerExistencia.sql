CREATE PROCEDURE ObtenerExistencia
    @id_empresa  INT,
    @id_producto INT,
    @id_bodega   INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.id_producto AS IdProducto,
        e.id_bodega AS IdBodega,
        e.cantidad_disponible AS CantidadDisponible,
        e.cantidad_reservada AS CantidadReservada
    FROM EXISTENCIA e
    INNER JOIN PRODUCTO p
        ON p.id_producto = e.id_producto
    INNER JOIN BODEGA b
        ON b.id_bodega = e.id_bodega
    WHERE e.id_producto = @id_producto
      AND e.id_bodega = @id_bodega
      AND p.id_empresa = @id_empresa
      AND b.id_empresa = @id_empresa;
END;
