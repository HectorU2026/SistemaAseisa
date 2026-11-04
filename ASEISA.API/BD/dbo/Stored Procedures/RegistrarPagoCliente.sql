CREATE PROCEDURE [dbo].[RegistrarPagoCliente]
    @id_empresa INT,
    @id_usuario INT,
    @id_cuenta_cobrar INT,
    @id_cuenta_bancaria INT,
    @fecha_pago DATETIME2,
    @monto DECIMAL(18,2),
    @metodo_pago VARCHAR(50),
    @referencia VARCHAR(150) = NULL,
    @id_estado INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Validar que la cuenta bancaria exista y pertenezca a la empresa
        IF NOT EXISTS (SELECT 1 FROM dbo.CUENTA_BANCARIA WITH (UPDLOCK) WHERE id_cuenta_bancaria = @id_cuenta_bancaria AND id_empresa = @id_empresa)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 4; -- Código 4: Cuenta bancaria inválida
            RETURN;
        END;

        -- 2. Validar que la cuenta por cobrar exista y el monto no supere el saldo
        DECLARE @saldo_pendiente DECIMAL(18,2);
        
        SELECT @saldo_pendiente = cpc.saldo_pendiente
        FROM dbo.CUENTA_POR_COBRAR cpc WITH (UPDLOCK)
        INNER JOIN dbo.VENTAS v ON cpc.id_venta = v.id_venta
        WHERE cpc.id_cuenta_cobrar = @id_cuenta_cobrar AND v.id_empresa = @id_empresa;

        IF @saldo_pendiente IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 5; -- Código 5: Cuenta por cobrar no encontrada
            RETURN;
        END;

        IF @monto > @saldo_pendiente
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 6; -- Código 6: El pago supera el saldo pendiente de la factura
            RETURN;
        END;

        -- 3. Insertar el pago
        INSERT INTO dbo.PAGO_CLIENTE (id_cuenta_cobrar, id_cuenta_bancaria, fecha_pago, monto, metodo_pago, referencia, id_estado)
        VALUES (@id_cuenta_cobrar, @id_cuenta_bancaria, @fecha_pago, @monto, @metodo_pago, @referencia, @id_estado);
        
        DECLARE @id_pago INT = SCOPE_IDENTITY();

        -- 4. Actualizar saldo de la cuenta por cobrar
        UPDATE dbo.CUENTA_POR_COBRAR
        SET saldo_pendiente = saldo_pendiente - @monto
        WHERE id_cuenta_cobrar = @id_cuenta_cobrar;

        -- 5. Actualizar saldo del banco
        UPDATE dbo.CUENTA_BANCARIA
        SET saldo_actual = saldo_actual + @monto
        WHERE id_cuenta_bancaria = @id_cuenta_bancaria;

        -- 6. Generar el movimiento bancario automático
        INSERT INTO dbo.MOVIMIENTO_BANCARIO (id_cuenta_bancaria, fecha_movimiento, descripcion, referencia, tipo_movimiento, monto, origen, id_estado)
        VALUES (@id_cuenta_bancaria, CAST(@fecha_pago AS DATE), 'Pago de cliente por factura', @referencia, 'Ingreso', @monto, 'PAGO_CLIENTE', @id_estado);

        -- 7. Registrar en bitácora
        DECLARE @nuevo VARCHAR(MAX) = (SELECT * FROM dbo.PAGO_CLIENTE WHERE id_pago_cliente = @id_pago FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        INSERT INTO dbo.BITACORA (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES (@id_usuario, @id_empresa, 'PAGO_CLIENTE', @id_pago, 'REGISTRAR_PAGO', NULL, @nuevo);

        COMMIT TRANSACTION;
        SELECT 1; -- Éxito
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;