
CREATE   PROCEDURE dbo.FacturarOrdenCompra
    @id_orden_compra INT,
    @id_empresa      INT,
    @id_proveedor    INT, -- Aunque no se use en el update, viene en el request
    @id_usuario      INT,
    @fecha_factura   DATETIME2,
    @id_termino_pago INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @id_estado_orden_compra INT;
        DECLARE @id_estado_facturada INT;
        DECLARE @total DECIMAL(18,2);

        SELECT @id_estado_orden_compra = id_estado FROM ESTADO WHERE nombre_estado = 'Orden de Compra';
        SELECT @id_estado_facturada = id_estado FROM ESTADO WHERE nombre_estado = 'Facturada';

        SELECT 
            @total = total
        FROM ORDEN_COMPRA WITH (UPDLOCK, HOLDLOCK)
        WHERE id_orden_compra = @id_orden_compra 
          AND id_empresa = @id_empresa 
          AND id_estado = @id_estado_orden_compra;

        IF @total IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Exito, 'La compra no está en estado orden de compra, no se puede realizar la factura' AS Mensaje;
            RETURN;
        END

        UPDATE ORDEN_COMPRA
        SET id_estado = @id_estado_facturada
        WHERE id_orden_compra = @id_orden_compra;

        COMMIT TRANSACTION;
        SELECT 1 AS Exito, 'Factura creada correctamente' AS Mensaje;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;