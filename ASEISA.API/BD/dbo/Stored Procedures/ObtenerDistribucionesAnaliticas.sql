CREATE PROCEDURE [dbo].[ObtenerDistribucionesAnaliticas]
    @idEmpresa INT, @idDistribucion INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.id_distribucion_analitica AS IdDistribucion, d.descripcion AS Descripcion,
        d.importe AS Importe, d.referencia AS Referencia, d.unidad AS Unidad,
        d.id_proveedor AS IdProveedor, pr.nombre_razon_social AS Proveedor,
        d.id_producto AS IdProducto, p.nombre AS Producto, CAST(CASE WHEN d.id_estado = 1 THEN 1 ELSE 0 END AS BIT) AS Activo,
        d.fecha_creacion AS FechaCreacion, d.fecha_modificacion AS FechaModificacion
    FROM dbo.DISTRIBUCION_ANALITICA d
    JOIN dbo.PROVEEDOR pr ON pr.id_proveedor = d.id_proveedor AND pr.id_empresa = d.id_empresa
    JOIN dbo.PRODUCTO p ON p.id_producto = d.id_producto AND p.id_empresa = d.id_empresa
    WHERE d.id_empresa = @idEmpresa AND (@idDistribucion IS NULL OR d.id_distribucion_analitica = @idDistribucion)
    ORDER BY d.id_distribucion_analitica DESC;
END;