

CREATE   PROCEDURE dbo.FacturarOrdenDeVenta
    @id_cotizacion   INT,
    @id_empresa      INT,
    @id_cliente      INT,
    @id_usuario      INT,
    @numero_venta    VARCHAR(50),
    @numero_factura  VARCHAR(50),
    @tipo_factura    VARCHAR(50),
    @condicion_venta VARCHAR(50),
    @moneda          VARCHAR(10),
    @tipo_cambio     DECIMAL(18,6),
    @subtotal        DECIMAL(18,2),
    @impuesto        DECIMAL(18,2),
    @descuento       DECIMAL(18,2),
    @total           DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @id_estado_orden_venta INT;
    DECLARE @id_estado_facturado INT;
    DECLARE @id_estado_emitida INT;

    SELECT @id_estado_orden_venta = id_estado FROM ESTADO WHERE nombre_estado = 'Orden de Venta';
    SELECT @id_estado_facturado = id_estado FROM ESTADO WHERE nombre_estado = 'Facturado';
    SELECT @id_estado_emitida = id_estado FROM ESTADO WHERE nombre_estado = 'Emitida';

    IF NOT EXISTS (
        SELECT 1 FROM COTIZACION 
        WHERE id_cotizacion = @id_cotizacion AND id_estado = @id_estado_orden_venta
    )
    BEGIN
        SELECT 0 AS IdFactura, 'La cotización no está en estado Orden de Venta';
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO VENTAS (
            id_empresa, id_cliente, id_cotizacion, id_usuario,
            numero_comprobante, tipo_comprobante, fecha_emision, condicion_venta,
            moneda, tipo_cambio, subtotal, impuesto, descuento, total, id_estado
        )
        VALUES (
            @id_empresa, @id_cliente, @id_cotizacion, @id_usuario,
            @numero_venta, 'Factura', SYSDATETIME(), @condicion_venta,
            @moneda, @tipo_cambio, @subtotal, @impuesto, @descuento, @total, @id_estado_emitida
        );

        DECLARE @id_venta INT = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO DETALLE_VENTA (
            id_venta, id_producto, cantidad, precio_unitario,
            porcentaje_descuento, porcentaje_impuesto, subtotal
        )
        SELECT 
            @id_venta, id_producto, cantidad, precio_unitario,
            porcentaje_descuento, porcentaje_impuesto, subtotal
        FROM DETALLE_COTIZACION
        WHERE id_cotizacion = @id_cotizacion;

        INSERT INTO FACTURA (
            id_venta, id_empresa, id_usuario, id_estado,
            numero_factura, fecha_emision, tipo_factura, moneda, tipo_cambio,
            subtotal, impuesto, descuento, total, saldo_pendiente
        )
        VALUES (
            @id_venta, @id_empresa, @id_usuario, @id_estado_emitida,
            @numero_factura, SYSDATETIME(), @tipo_factura, @moneda, @tipo_cambio,
            @subtotal, @impuesto, @descuento, @total, @total
        );

        DECLARE @id_factura INT = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO DETALLE_FACTURA (
            id_factura, id_producto, cantidad, precio_unitario,
            porcentaje_descuento, porcentaje_impuesto, subtotal
        )
        SELECT 
            @id_factura, id_producto, cantidad, precio_unitario,
            porcentaje_descuento, porcentaje_impuesto, subtotal
        FROM DETALLE_COTIZACION
        WHERE id_cotizacion = @id_cotizacion;

        UPDATE COTIZACION
        SET id_estado = @id_estado_facturado
        WHERE id_cotizacion = @id_cotizacion;

        COMMIT TRANSACTION;

        SELECT @id_factura AS IdFactura, 'Facturación exitosa' AS Mensaje;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;