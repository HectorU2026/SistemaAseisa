CREATE   PROCEDURE dbo.AnularCompra
    @id_empresa INT,
    @id_usuario INT,
    @id_compra INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @@TRANCOUNT <> 0 THROW 51200, 'Ejecute sin una transaccion externa.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.USUARIO u
        INNER JOIN dbo.ESTADO e ON e.id_estado = u.id_estado
        WHERE u.id_usuario = @id_usuario AND u.id_empresa = @id_empresa
          AND LTRIM(RTRIM(e.nombre_estado)) = 'Activo')
        THROW 51201, 'El usuario no esta activo o no pertenece a la empresa.', 1;

        DECLARE @actual INT, @cancelada INT;
        SELECT @actual = id_estado FROM dbo.ORDEN_COMPRA WITH (UPDLOCK,HOLDLOCK)
        WHERE id_orden_compra = @id_compra AND id_empresa = @id_empresa;
        IF @actual IS NULL THROW 51209, 'La compra no existe en esta empresa.', 1;
        IF (SELECT COUNT(*) FROM dbo.ESTADO WITH (HOLDLOCK)
            WHERE LTRIM(RTRIM(nombre_estado)) = 'Cancelada') <> 1
            THROW 51219, 'Debe existir un unico estado Cancelada.', 1;
        SELECT @cancelada = id_estado FROM dbo.ESTADO
        WHERE LTRIM(RTRIM(nombre_estado)) = 'Cancelada';
        IF @actual = @cancelada
        BEGIN
            COMMIT TRANSACTION;
            SELECT @id_compra AS IdCompra;
            RETURN;
        END;
        IF EXISTS (SELECT 1 FROM dbo.CUENTA_POR_PAGAR WITH (UPDLOCK,HOLDLOCK)
                   WHERE id_orden_compra = @id_compra)
            THROW 51211, 'La compra tiene una cuenta por pagar vinculada; requiere el proceso financiero correspondiente.', 1;
        DECLARE @anterior VARCHAR(MAX) = (SELECT o.*,
            JSON_QUERY((SELECT dc.* FROM dbo.DETALLE_ORDEN_COMPRA dc
                        WHERE dc.id_orden_compra = o.id_orden_compra
                        ORDER BY dc.id_producto FOR JSON PATH)) AS detalle
         FROM dbo.ORDEN_COMPRA o WHERE o.id_orden_compra = @id_compra
           AND o.id_empresa = @id_empresa FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        UPDATE dbo.ORDEN_COMPRA SET id_estado = @cancelada,fecha_modificacion = SYSDATETIME()
        WHERE id_orden_compra = @id_compra AND id_empresa = @id_empresa;
        DECLARE @nuevo VARCHAR(MAX) = (SELECT o.*,
            JSON_QUERY((SELECT dc.* FROM dbo.DETALLE_ORDEN_COMPRA dc
                        WHERE dc.id_orden_compra = o.id_orden_compra
                        ORDER BY dc.id_producto FOR JSON PATH)) AS detalle
         FROM dbo.ORDEN_COMPRA o WHERE o.id_orden_compra = @id_compra
           AND o.id_empresa = @id_empresa FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        INSERT dbo.BITACORA(id_usuario,id_empresa,tabla_afectada,registro_id,
                            accion,valor_anterior,valor_nuevo)
        VALUES (@id_usuario,@id_empresa,'ORDEN_COMPRA',CONVERT(VARCHAR(100),@id_compra),
                'ANULAR',@anterior,@nuevo);
        COMMIT TRANSACTION;
        SELECT @id_compra AS IdCompra;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;