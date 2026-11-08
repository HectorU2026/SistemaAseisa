
-- ============================================================
-- 2. AGREGAR MÉTODO DE PAGO
-- ============================================================

CREATE   PROCEDURE dbo.AgregarMetodoPago
    @id_empresa INT,
    @id_usuario INT,
    @nombre NVARCHAR(100)
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

    SET @nombre = LTRIM(RTRIM(@nombre));

    IF @nombre IS NULL OR LEN(@nombre) = 0
    BEGIN
        THROW 51020,
            'El nombre del método de pago es obligatorio.', 1;
    END;

    DECLARE @id_metodo_pago INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (
            SELECT 1
            FROM dbo.METODO_PAGO WITH (UPDLOCK, HOLDLOCK)
            WHERE id_empresa = @id_empresa
              AND nombre = @nombre
        )
        BEGIN
            THROW 51021,
                'Ya existe un método de pago con ese nombre.', 1;
        END;

        INSERT INTO dbo.METODO_PAGO (
            id_empresa,
            nombre,
            activo,
            fecha_creacion,
            fecha_modificacion
        )
        VALUES (
            @id_empresa,
            @nombre,
            1,
            SYSDATETIME(),
            NULL
        );

        SET @id_metodo_pago =
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
            'METODO_PAGO',
            CONVERT(VARCHAR(100), @id_metodo_pago),
            'INSERTAR',
            NULL,
            CONCAT(
                'Nombre: ', @nombre,
                '; Activo: 1'
            )
        );

        COMMIT TRANSACTION;

        SELECT @id_metodo_pago AS IdMetodoPago;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;