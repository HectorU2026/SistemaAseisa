CREATE   PROCEDURE dbo.EditarTerminoPago
    @id_empresa INT,
    @id_usuario INT,
    @id_termino_pago INT,
    @nombre NVARCHAR(100),
    @dias_credito INT
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

    SET @nombre = LTRIM(RTRIM(@nombre));

    IF @nombre IS NULL OR LEN(@nombre) = 0
        THROW 51010,
            'El nombre del término de pago es obligatorio.', 1;

    IF @dias_credito IS NULL OR @dias_credito < 0
        THROW 51011,
            'Los días de crédito deben ser cero o mayores.', 1;

    DECLARE
        @nombre_anterior NVARCHAR(100),
        @dias_anteriores INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @nombre_anterior = nombre,
            @dias_anteriores = dias_credito
        FROM dbo.TERMINO_PAGO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_termino_pago = @id_termino_pago
          AND id_empresa = @id_empresa;

        IF @nombre_anterior IS NULL
            THROW 51013,
                'El término de pago no existe en la empresa.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.TERMINO_PAGO WITH (UPDLOCK, HOLDLOCK)
            WHERE id_empresa = @id_empresa
              AND nombre = @nombre
              AND id_termino_pago <> @id_termino_pago
        )
            THROW 51012,
                'Ya existe otro término de pago con ese nombre.', 1;

        UPDATE dbo.TERMINO_PAGO
        SET nombre = @nombre,
            dias_credito = @dias_credito,
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
            'ACTUALIZAR',
            CONCAT(
                'Nombre: ', @nombre_anterior,
                '; DiasCredito: ', @dias_anteriores
            ),
            CONCAT(
                'Nombre: ', @nombre,
                '; DiasCredito: ', @dias_credito
            )
        );

        COMMIT TRANSACTION;

        SELECT @id_termino_pago AS IdTerminoPago;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;