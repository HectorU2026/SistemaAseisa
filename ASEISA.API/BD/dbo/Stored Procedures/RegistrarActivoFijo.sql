CREATE PROCEDURE [dbo].[RegistrarActivoFijo]
    @id_empresa INT,
    @codigo VARCHAR(50),
    @nombre VARCHAR(150),
    @descripcion VARCHAR(500) = NULL,
    @fecha_adquisicion DATE,
    @costo_adquisicion DECIMAL(18,2),
    @valor_residual DECIMAL(18,2),
    @vida_util_meses INT,
    @metodo_depreciacion VARCHAR(100),
    @id_cuenta_activo INT,
    @id_cuenta_depreciacion INT,
    @id_estado INT,
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validar que la cuenta de activo exista para la empresa
        IF NOT EXISTS (SELECT 1 FROM CUENTA_CONTABLE WHERE id_cuenta_contable = @id_cuenta_activo AND id_empresa = @id_empresa)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 4; -- Código 4: Cuenta inválida
            RETURN;
        END;

        -- Validar que la cuenta de depreciación exista
        IF NOT EXISTS (SELECT 1 FROM CUENTA_CONTABLE WHERE id_cuenta_contable = @id_cuenta_depreciacion AND id_empresa = @id_empresa)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 4;
            RETURN;
        END;

        INSERT INTO ACTIVO_FIJO
        (
            id_empresa, codigo, nombre, descripcion, fecha_adquisicion,
            costo_adquisicion, valor_residual, vida_util_meses,
            metodo_depreciacion, id_cuenta_activo, id_cuenta_depreciacion, id_estado
        )
        VALUES
        (
            @id_empresa, @codigo, @nombre, @descripcion, @fecha_adquisicion,
            @costo_adquisicion, @valor_residual, @vida_util_meses,
            @metodo_depreciacion, @id_cuenta_activo, @id_cuenta_depreciacion, @id_estado
        );

        DECLARE @id_activo INT = SCOPE_IDENTITY();

        -- Registro en Bitácora
        DECLARE @nuevo VARCHAR(MAX) = (SELECT * FROM ACTIVO_FIJO WHERE id_activo_fijo = @id_activo FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        
        INSERT INTO BITACORA (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES (@id_usuario, @id_empresa, 'ACTIVO_FIJO', @id_activo, 'CREAR_ACTIVO', NULL, @nuevo);

        COMMIT TRANSACTION;
        SELECT 1; -- Código 1: Éxito
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;