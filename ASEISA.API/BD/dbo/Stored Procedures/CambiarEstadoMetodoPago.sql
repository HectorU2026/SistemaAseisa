
-- ============================================================
-- 4. CAMBIAR ESTADO DEL MÉTODO DE PAGO
-- ============================================================

CREATE   PROCEDURE dbo.CambiarEstadoMetodoPago
    @id_empresa INT,
    @id_usuario INT,
    @id_metodo_pago INT,
    @activo BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.USUARIO
        WHERE id_usuario = @id_usuario
          AND id_empresa = @id_empresa
    )
    BEGIN
        THROW 51008,
            'El usuario no pertenece a la empresa indicada.', 1;
    END;

    IF @activo IS NULL
    BEGIN
        THROW 51023,
            'Debe indicar el estado del método de pago.', 1;
    END;

    DECLARE @estado_anterior BIT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @estado_anterior = activo
        FROM dbo.METODO_PAGO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_metodo_pago = @id_metodo_pago
          AND id_empresa = @id_empresa;

        IF @estado_anterior IS NULL
        BEGIN
            THROW 51022,
                'El método de pago no existe en la empresa.', 1;
        END;

        IF @estado_anterior <> @activo
        BEGIN
            UPDATE dbo.METODO_PAGO
            SET
                activo = @activo,
                fecha_modificacion = SYSDATETIME()
            WHERE id_metodo_pago = @id_metodo_pago
              AND id_empresa = @id_empresa;

            INSERT INTO dbo.BITACORA (
                id_usuario,
                id_empresa,
                tabla_afectada,
                registro_id,
                accion,
                valor_anterior,
                valor_nuevo
            )
            VALUES (
                @id_usuario,
                @id_empresa,
                'METODO_PAGO',
                CONVERT(VARCHAR(100), @id_metodo_pago),
                CASE
                    WHEN @activo = 1 THEN 'ACTIVAR'
                    ELSE 'DESACTIVAR'
                END,
                CONCAT('Activo: ', @estado_anterior),
                CONCAT('Activo: ', @activo)
            );
        END;

        COMMIT TRANSACTION;

        SELECT
            @id_metodo_pago AS IdMetodoPago,
            @activo AS Activo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;