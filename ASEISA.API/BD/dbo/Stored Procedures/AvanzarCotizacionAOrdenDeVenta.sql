

CREATE   PROCEDURE dbo.AvanzarCotizacionAOrdenDeVenta
    @id_cotizacion INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @id_estado_enviada INT;
    DECLARE @id_estado_orden_venta INT;
    DECLARE @id_empresa INT;
    DECLARE @id_bodega INT;

    SELECT @id_estado_enviada = id_estado FROM ESTADO WHERE nombre_estado = 'Cotización Enviada';
    SELECT @id_estado_orden_venta = id_estado FROM ESTADO WHERE nombre_estado = 'Orden de Venta';

    SELECT @id_empresa = id_empresa 
    FROM COTIZACION 
    WHERE id_cotizacion = @id_cotizacion AND id_estado = @id_estado_enviada;

    IF @id_empresa IS NULL
    BEGIN
        SELECT 0 AS Exito, 'La cotización no existe o no está en estado Cotización Enviada' AS Mensaje;
        RETURN;
    END

    SELECT TOP 1 @id_bodega = id_bodega FROM BODEGA WHERE id_empresa = @id_empresa;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (
            SELECT 1 
            FROM DETALLE_COTIZACION d
            INNER JOIN PRODUCTO p ON p.id_producto = d.id_producto
            LEFT JOIN EXISTENCIA e WITH (UPDLOCK) ON e.id_producto = d.id_producto AND e.id_bodega = @id_bodega
            WHERE d.id_cotizacion = @id_cotizacion 
              AND p.inventariable = 1
              AND ISNULL(e.cantidad_disponible, 0) < d.cantidad
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Exito, 'No hay suficiente inventario para avanzar a Orden de Venta.' AS Mensaje;
            RETURN;
        END

        UPDATE e
        SET e.cantidad_disponible = e.cantidad_disponible - d.cantidad,
            e.fecha_actualizacion = SYSDATETIME()
        FROM EXISTENCIA e
        INNER JOIN DETALLE_COTIZACION d ON e.id_producto = d.id_producto
        INNER JOIN PRODUCTO p ON p.id_producto = e.id_producto
        WHERE d.id_cotizacion = @id_cotizacion 
          AND e.id_bodega = @id_bodega
          AND p.inventariable = 1;

        UPDATE COTIZACION
        SET id_estado = @id_estado_orden_venta
        WHERE id_cotizacion = @id_cotizacion;

        COMMIT TRANSACTION;
        SELECT 1 AS Exito, 'Cotización avanzada a Orden de Venta exitosamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;