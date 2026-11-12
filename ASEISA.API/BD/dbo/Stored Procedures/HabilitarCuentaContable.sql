CREATE   PROCEDURE dbo.HabilitarCuentaContable
    @idEmpresa INT,
    @idUsuario INT,
    @idCuentaContable INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @estado INT, @idPrincipal INT,
                @anterior NVARCHAR(MAX), @nuevo NVARCHAR(MAX);

        SELECT @estado = id_estado, @idPrincipal = id_cuenta_principal
        FROM dbo.CUENTA_CONTABLE WITH (UPDLOCK, HOLDLOCK)
        WHERE id_empresa = @idEmpresa AND id_cuenta_contable = @idCuentaContable;

        IF @estado IS NULL
            THROW 51405, 'La cuenta contable no existe.', 1;

        IF @estado <> 2
            THROW 51411, 'Solo se pueden habilitar cuentas inactivas.', 1;

        IF @idPrincipal IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM dbo.CUENTA_CONTABLE WITH (HOLDLOCK)
            WHERE id_empresa = @idEmpresa
              AND id_cuenta_contable = @idPrincipal AND id_estado = 1
        )
            THROW 51412, 'Primero habilite la cuenta principal.', 1;

        SELECT @anterior = (
            SELECT * FROM dbo.CUENTA_CONTABLE
            WHERE id_empresa = @idEmpresa AND id_cuenta_contable = @idCuentaContable
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        );

        UPDATE dbo.CUENTA_CONTABLE
        SET id_estado = 1
        WHERE id_empresa = @idEmpresa AND id_cuenta_contable = @idCuentaContable;

        SELECT @nuevo = (
            SELECT * FROM dbo.CUENTA_CONTABLE
            WHERE id_empresa = @idEmpresa AND id_cuenta_contable = @idCuentaContable
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        );

        INSERT dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id,
             accion, valor_anterior, valor_nuevo)
        VALUES (@idUsuario, @idEmpresa, 'CUENTA_CONTABLE',
                CONVERT(VARCHAR(100), @idCuentaContable), 'HABILITAR', @anterior, @nuevo);

        COMMIT TRANSACTION;
        SELECT @idCuentaContable AS IdCuentaContable;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;