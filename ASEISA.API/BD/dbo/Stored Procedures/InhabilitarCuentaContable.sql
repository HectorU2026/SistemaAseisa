
-- CON-006: inhabilitar sin borrar la cuenta.
CREATE   PROCEDURE dbo.InhabilitarCuentaContable
    @idEmpresa INT,
    @idUsuario INT,
    @idCuentaContable INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @estado INT, @anterior NVARCHAR(MAX), @nuevo NVARCHAR(MAX);

        SELECT @estado = id_estado
        FROM dbo.CUENTA_CONTABLE WITH (UPDLOCK, HOLDLOCK)
        WHERE id_empresa = @idEmpresa
          AND id_cuenta_contable = @idCuentaContable;

        IF @estado IS NULL
            THROW 51405, 'La cuenta contable no existe.', 1;

        IF @estado = 2
            THROW 51409, 'La cuenta contable ya está inactiva.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.CUENTA_CONTABLE WITH (UPDLOCK, HOLDLOCK)
            WHERE id_empresa = @idEmpresa
              AND id_cuenta_principal = @idCuentaContable
              AND id_estado = 1
        )
            THROW 51410, 'La cuenta tiene subcuentas activas.', 1;

        SET @anterior = (
            SELECT * FROM dbo.CUENTA_CONTABLE
            WHERE id_empresa = @idEmpresa
              AND id_cuenta_contable = @idCuentaContable
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        );

        UPDATE dbo.CUENTA_CONTABLE
        SET id_estado = 2
        WHERE id_empresa = @idEmpresa
          AND id_cuenta_contable = @idCuentaContable;

        SET @nuevo = (
            SELECT * FROM dbo.CUENTA_CONTABLE
            WHERE id_empresa = @idEmpresa
              AND id_cuenta_contable = @idCuentaContable
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        );

        INSERT INTO dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id,
             accion, valor_anterior, valor_nuevo)
        VALUES
            (@idUsuario, @idEmpresa, 'CUENTA_CONTABLE',
             CONVERT(VARCHAR(100), @idCuentaContable), 'INHABILITAR', @anterior, @nuevo);

        COMMIT TRANSACTION;
        SELECT @idCuentaContable AS IdCuentaContable;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;