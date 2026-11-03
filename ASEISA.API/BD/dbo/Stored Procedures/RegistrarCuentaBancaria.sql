
CREATE   PROCEDURE dbo.RegistrarCuentaBancaria
    @id_empresa         INT,
    @numero_cuenta      VARCHAR(100),
    @id_tipo_cuenta     INT,
    @moneda             VARCHAR(10),
    @saldo_actual       DECIMAL(18,2),
    @id_cuenta_contable INT,
    @id_estado          INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @saldo_actual < 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS IdCuentaBancaria, 'El saldo inicial no puede ser negativo.' AS Mensaje;
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM CUENTA_BANCARIA WHERE numero_cuenta = @numero_cuenta AND id_empresa = @id_empresa)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS IdCuentaBancaria, 'Ya existe una cuenta bancaria con este número en la empresa.' AS Mensaje;
            RETURN;
        END

        INSERT INTO CUENTA_BANCARIA (
            id_empresa, 
            numero_cuenta, 
            id_tipo_cuenta, 
            moneda, 
            saldo_actual, 
            id_cuenta_contable, 
            id_estado
        )
        VALUES (
            @id_empresa, 
            @numero_cuenta, 
            @id_tipo_cuenta, 
            @moneda, 
            @saldo_actual, 
            @id_cuenta_contable, 
            @id_estado
        );

        DECLARE @nueva_cuenta_id INT = CAST(SCOPE_IDENTITY() AS INT);

        COMMIT TRANSACTION;

        SELECT @nueva_cuenta_id AS IdCuentaBancaria, 'Cuenta bancaria registrada correctamente.' AS Mensaje;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;