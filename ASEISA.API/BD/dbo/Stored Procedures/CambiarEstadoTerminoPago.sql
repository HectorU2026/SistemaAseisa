CREATE   PROCEDURE dbo.CambiarEstadoTerminoPago
    @id_empresa INT,
    @id_usuario INT,
    @id_termino_pago INT,
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
        THROW 51008,
            'El usuario no pertenece a la empresa indicada.', 1;

    IF @activo IS NULL
        THROW 51014,
            'Debe indicar el estado del término de pago.', 1;

    DECLARE @estado_anterior BIT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @estado_anterior = activo
        FROM dbo.TERMINO_PAGO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_termino_pago = @id_termino_pago
          AND id_empresa = @id_empresa;

        IF @estado_anterior IS NULL
            THROW 51013,
                'El término de pago no existe en la empresa.', 1;

        IF @estado_anterior <> @activo
        BEGIN
            UPDATE dbo.TERMINO_PAGO
            SET activo = @activo,
                fecha_modificacion = SYSDATETIME()
            WHERE id_termino_pago = @id_termino_pago
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
                'TERMINO_PAGO',
                CONVERT(VARCHAR(100), @id_termino_pago),
                CASE WHEN @activo = 1
                     THEN 'ACTIVAR'
                     ELSE 'DESACTIVAR'
                END,
                CONCAT('Activo: ', @estado_anterior),
                CONCAT('Activo: ', @activo)
            );
        END;

        COMMIT TRANSACTION;

        SELECT
            @id_termino_pago AS IdTerminoPago,
            @activo AS Activo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;