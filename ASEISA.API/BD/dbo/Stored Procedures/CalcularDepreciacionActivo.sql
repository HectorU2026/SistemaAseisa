CREATE PROCEDURE [dbo].[CalcularDepreciacionActivo]
    @id_empresa INT,
    @id_activo_fijo INT,
    @periodo VARCHAR(20), 
    @id_estado INT,
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @costo DECIMAL(18,2), @residual DECIMAL(18,2), @vida_util INT;
        
        -- 1. Validar que el activo exista y pertenezca a la empresa
        SELECT 
            @costo = costo_adquisicion, 
            @residual = valor_residual, 
            @vida_util = vida_util_meses
        FROM dbo.ACTIVO_FIJO WITH (UPDLOCK)
        WHERE id_activo_fijo = @id_activo_fijo AND id_empresa = @id_empresa;

        IF @costo IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 3; -- Código 3: Activo no encontrado
            RETURN;
        END;

        IF @vida_util <= 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 4; -- Código 4: Vida útil inválida
            RETURN;
        END;

        -- 2. Validar que no se haya calculado ya para este periodo
        IF EXISTS (SELECT 1 FROM dbo.DEPRECIACION WHERE id_activo_fijo = @id_activo_fijo AND periodo = @periodo)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 5; -- Código 5: Ya existe depreciación para este periodo
            RETURN;
        END;

        -- 3. Calcular montos
        DECLARE @monto_depreciacion DECIMAL(18,2) = (@costo - @residual) / @vida_util;
        
        DECLARE @depreciacion_acumulada_anterior DECIMAL(18,2) = 
            ISNULL((SELECT SUM(monto_despreciacion) FROM dbo.DEPRECIACION WHERE id_activo_fijo = @id_activo_fijo), 0);
            
        DECLARE @nueva_acumulada DECIMAL(18,2) = @depreciacion_acumulada_anterior + @monto_depreciacion;
        DECLARE @valor_en_libros DECIMAL(18,2) = @costo - @nueva_acumulada;

        -- Evitar depreciar por debajo del valor residual (si ya cumplió su vida útil)
        IF @valor_en_libros < @residual
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 6; -- Código 6: El activo ya está totalmente depreciado
            RETURN;
        END;

        -- 4. Insertar la depreciación
        INSERT INTO dbo.DEPRECIACION 
        (
            id_activo_fijo, id_asiento, periodo, monto_despreciacion, 
            depreciacion_acumulada, valor_en_libros, fecha_calculo, id_estado
        )
        VALUES 
        (
            @id_activo_fijo, NULL, @periodo, @monto_depreciacion, 
            @nueva_acumulada, @valor_en_libros, SYSDATETIME(), @id_estado
        );

        DECLARE @id_depreciacion INT = SCOPE_IDENTITY();

        -- 5. Registrar en la bitácora
        DECLARE @nuevo VARCHAR(MAX) = (SELECT * FROM dbo.DEPRECIACION WHERE id_depreciacion = @id_depreciacion FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        
        INSERT INTO dbo.BITACORA (id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES (@id_usuario, @id_empresa, 'DEPRECIACION', @id_depreciacion, 'CALCULAR_DEPRECIACION', NULL, @nuevo);

        COMMIT TRANSACTION;
        SELECT 1; -- Código 1: Éxito
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;