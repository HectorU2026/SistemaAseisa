CREATE   PROCEDURE dbo.AgregarTerminoPago
    @id_empresa INT,
    @id_usuario INT,
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

    DECLARE @id_termino_pago INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (
            SELECT 1
            FROM dbo.TERMINO_PAGO WITH (UPDLOCK, HOLDLOCK)
            WHERE id_empresa = @id_empresa
              AND nombre = @nombre
        )
            THROW 51012,
                'Ya existe un término de pago con ese nombre.', 1;

        INSERT INTO dbo.TERMINO_PAGO (
            id_empresa,
            nombre,
            dias_credito,
            activo,
            fecha_creacion,
            fecha_modificacion
        )
        VALUES (
            @id_empresa,
            @nombre,
            @dias_credito,
            1,
            SYSDATETIME(),
            NULL
        );

        SET @id_termino_pago =
            CONVERT(INT, SCOPE_IDENTITY());

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
            'INSERTAR',
            NULL,
            CONCAT(
                'Nombre: ', @nombre,
                '; DiasCredito: ', @dias_credito,
                '; Activo: 1'
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