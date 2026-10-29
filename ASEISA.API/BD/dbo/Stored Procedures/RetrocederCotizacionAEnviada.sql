
CREATE   PROCEDURE dbo.RetrocederCotizacionAEnviada
    @id_cotizacion INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @id_estado_enviada INT;
    DECLARE @id_estado_orden_venta INT;
    DECLARE @id_empresa INT;
    DECLARE @id_bodega INT;

    SELECT @id_estado_enviada = id_estado_cotizacion FROM ESTADO_COTIZACION WHERE nombre_estado = 'Cotización Enviada';
    SELECT @id_estado_orden_venta = id_estado_cotizacion FROM ESTADO_COTIZACION WHERE nombre_estado = 'Orden de Venta';

    SELECT @id_empresa = id_empresa 
    FROM COTIZACION 
    WHERE id_cotizacion = @id_cotizacion AND id_estado_cotizacion = @id_estado_orden_venta;

    IF @id_empresa IS NULL
    BEGIN
        SELECT 0 AS Exito, 'La cotización no existe o no está en estado Orden de Venta' AS Mensaje;
        RETURN;
    END

    SELECT TOP 1 @id_bodega = id_bodega FROM BODEGA WHERE id_empresa = @id_empresa;

    BEGIN TRY
        BEGIN TRANSACTION;
        -- Devolver stock de producto
        UPDATE e
        SET e.cantidad_disponible = e.cantidad_disponible + d.cantidad,
            e.fecha_actualizacion = SYSDATETIME()
        FROM EXISTENCIA e
        INNER JOIN DETALLE_COTIZACION d ON e.id_producto = d.id_producto
        INNER JOIN PRODUCTO p ON p.id_producto = e.id_producto
        WHERE d.id_cotizacion = @id_cotizacion 
          AND e.id_bodega = @id_bodega
          AND p.inventariable = 1;

        UPDATE COTIZACION
        SET id_estado_cotizacion = @id_estado_enviada
        WHERE id_cotizacion = @id_cotizacion;

        COMMIT TRANSACTION;
        SELECT 1 AS Exito, 'Cotización retrocedida a Cotización Enviada exitosamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;