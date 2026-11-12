CREATE   PROCEDURE dbo.AgregarCuentaContableRaiz
    @idEmpresa INT,
    @idUsuario INT,
    @codigo VARCHAR(50),
    @nombre VARCHAR(150),
    @tipoCuenta VARCHAR(50),
    @naturaleza VARCHAR(20),
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

    IF @codigo IS NULL OR @nombre IS NULL OR @conciliacionPagos IS NULL
       OR @tipoCuenta IS NULL OR @naturaleza IS NULL
       OR @tipoCuenta NOT IN ('Activo', 'Pasivo', 'Patrimonio', 'Ingreso', 'Gasto')
       OR @naturaleza NOT IN ('Deudora', 'Acreedora')
        THROW 51401, 'Complete los datos y seleccione una clasificación válida.', 1;

    IF @idEstado IS NULL OR @idEstado NOT IN (1, 2)
        THROW 51402, 'Seleccione un estado válido.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (
            SELECT 1 FROM dbo.CUENTA_CONTABLE WITH (UPDLOCK, HOLDLOCK)
            WHERE id_empresa = @idEmpresa AND codigo = @codigo
        )
            THROW 51404, 'Ya existe una cuenta con ese código en la empresa.', 1;

        INSERT INTO dbo.CUENTA_CONTABLE
            (id_empresa, codigo, nombre, tipo_cuenta, naturaleza, nivel, id_estado,
             permite_movimientos, id_cuenta_principal, conciliacion_pagos, impuestos, descripcion)
        VALUES
            (@idEmpresa, @codigo, @nombre, @tipoCuenta, @naturaleza, 1, @idEstado,
             0, NULL, @conciliacionPagos, @impuestos, @descripcion);

        DECLARE @idCuenta INT = CONVERT(INT, SCOPE_IDENTITY());
        DECLARE @nuevo NVARCHAR(MAX) = (
            SELECT * FROM dbo.CUENTA_CONTABLE
            WHERE id_empresa = @idEmpresa AND id_cuenta_contable = @idCuenta
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        );

        INSERT INTO dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES (@idUsuario, @idEmpresa, 'CUENTA_CONTABLE',
                CONVERT(VARCHAR(100), @idCuenta), 'AGREGAR', NULL, @nuevo);

        COMMIT TRANSACTION;
        SELECT @idCuenta AS IdCuentaContable;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;