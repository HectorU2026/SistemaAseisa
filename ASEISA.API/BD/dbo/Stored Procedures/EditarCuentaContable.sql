CREATE   PROCEDURE dbo.EditarCuentaContable
    @idEmpresa INT,
    @idUsuario INT,
    @idCuentaContable INT,
    @idCuentaPrincipal INT,
    @nombre VARCHAR(150),
    @conciliacionPagos BIT,
    @impuestos DECIMAL(9, 4) = NULL,
    @descripcion VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @nombre = NULLIF(TRIM(@nombre), '');

    IF @nombre IS NULL OR @conciliacionPagos IS NULL
        THROW 51401, 'Escriba todos los datos requeridos.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @anterior NVARCHAR(MAX), @nuevo NVARCHAR(MAX),
                @principalAnterior INT, @nivel INT,
                @tipo VARCHAR(50), @naturaleza VARCHAR(20);

        SELECT @principalAnterior = id_cuenta_principal, @nivel = nivel,
               @tipo = tipo_cuenta, @naturaleza = naturaleza
        FROM dbo.CUENTA_CONTABLE WITH (UPDLOCK, HOLDLOCK)
        WHERE id_empresa = @idEmpresa AND id_cuenta_contable = @idCuentaContable;

        IF @nivel IS NULL
            THROW 51405, 'La cuenta contable no existe.', 1;

        IF @idCuentaPrincipal IS NULL AND @principalAnterior IS NOT NULL
            THROW 51403, 'Seleccione una cuenta principal activa de su empresa.', 1;

        IF @idCuentaPrincipal = @idCuentaContable
            THROW 51407, 'Una cuenta no puede ser su propia cuenta principal.', 1;

        IF @idCuentaPrincipal IS NOT NULL
           AND (@principalAnterior IS NULL OR @principalAnterior <> @idCuentaPrincipal)
        BEGIN
            DECLARE @nivelPrincipal INT;

            SELECT @nivelPrincipal = nivel
            FROM dbo.CUENTA_CONTABLE WITH (HOLDLOCK)
            WHERE id_empresa = @idEmpresa AND id_cuenta_contable = @idCuentaPrincipal
              AND id_estado = 1 AND tipo_cuenta = @tipo AND naturaleza = @naturaleza;

            IF @nivelPrincipal IS NULL
                THROW 51403, 'Seleccione una cuenta principal activa de la misma empresa, tipo y naturaleza.', 1;

            IF EXISTS (
                SELECT 1 FROM dbo.CUENTA_CONTABLE WITH (UPDLOCK, HOLDLOCK)
                WHERE id_empresa = @idEmpresa AND id_cuenta_principal = @idCuentaContable
            )
                THROW 51408, 'No se puede cambiar la principal de una cuenta que tiene subcuentas.', 1;

            SET @nivel = @nivelPrincipal + 1;
        END;

        SELECT @anterior = (
            SELECT * FROM dbo.CUENTA_CONTABLE
            WHERE id_empresa = @idEmpresa AND id_cuenta_contable = @idCuentaContable
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        );

        UPDATE dbo.CUENTA_CONTABLE
        SET nombre = @nombre, id_cuenta_principal = @idCuentaPrincipal,
            conciliacion_pagos = @conciliacionPagos, impuestos = @impuestos,
            descripcion = @descripcion, nivel = @nivel
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
                CONVERT(VARCHAR(100), @idCuentaContable), 'EDITAR', @anterior, @nuevo);

        COMMIT TRANSACTION;
        SELECT @idCuentaContable AS IdCuentaContable;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;