CREATE   PROCEDURE dbo.AgregarCuentaContable
    @idEmpresa INT,
    @idUsuario INT,
    @idCuentaPrincipal INT,
    @codigo VARCHAR(50),
    @nombre VARCHAR(150),
    @conciliacionPagos BIT,
    @idEstado INT,
    @impuestos DECIMAL(9, 4) = NULL,
    @descripcion VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @codigo = NULLIF(TRIM(@codigo), '');
    SET @nombre = NULLIF(TRIM(@nombre), '');

    IF @codigo IS NULL OR @nombre IS NULL OR @idCuentaPrincipal IS NULL
       OR @conciliacionPagos IS NULL OR @idEstado IS NULL
        THROW 51401, 'Escriba todos los datos requeridos.', 1;

    IF @idEstado NOT IN (1, 2)
        THROW 51402, 'Seleccione un estado válido.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @tipoCuenta VARCHAR(50),
                @naturaleza VARCHAR(20),
                @nivel INT,
                @idCuenta INT,
                @nuevo NVARCHAR(MAX);

        SELECT @tipoCuenta = tipo_cuenta,
               @naturaleza = naturaleza,
               @nivel = nivel + 1
        FROM dbo.CUENTA_CONTABLE WITH (HOLDLOCK)
        WHERE id_cuenta_contable = @idCuentaPrincipal
          AND id_empresa = @idEmpresa
          AND id_estado = 1;

        IF @nivel IS NULL
            THROW 51403, 'Seleccione una cuenta principal activa de su empresa.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.CUENTA_CONTABLE WITH (UPDLOCK, HOLDLOCK)
            WHERE id_empresa = @idEmpresa AND codigo = @codigo
        )
            THROW 51404, 'Ya existe una cuenta con ese código en la empresa.', 1;

        INSERT INTO dbo.CUENTA_CONTABLE
        (
            id_empresa, codigo, nombre, tipo_cuenta, naturaleza, nivel,
            id_estado, id_cuenta_principal, conciliacion_pagos, impuestos, descripcion
        )
        VALUES
        (
            @idEmpresa, @codigo, @nombre, @tipoCuenta, @naturaleza, @nivel,
            @idEstado, @idCuentaPrincipal, @conciliacionPagos, @impuestos, @descripcion
        );

        SET @idCuenta = CONVERT(INT, SCOPE_IDENTITY());

        SET @nuevo = (
            SELECT *
            FROM dbo.CUENTA_CONTABLE
            WHERE id_cuenta_contable = @idCuenta AND id_empresa = @idEmpresa
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        INSERT INTO dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id,
             accion, valor_anterior, valor_nuevo)
        VALUES
            (@idUsuario, @idEmpresa, 'CUENTA_CONTABLE',
             CONVERT(VARCHAR(100), @idCuenta), 'AGREGAR', NULL, @nuevo);

        COMMIT TRANSACTION;

        SELECT @idCuenta AS IdCuentaContable;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;