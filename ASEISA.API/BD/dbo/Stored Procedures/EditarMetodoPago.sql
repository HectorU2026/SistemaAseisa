
-- ============================================================
-- 3. EDITAR MÉTODO DE PAGO
-- ============================================================

CREATE   PROCEDURE dbo.EditarMetodoPago
    @id_empresa INT,
    @id_usuario INT,
    @id_metodo_pago INT,
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

    DECLARE @nombre_anterior NVARCHAR(100);

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @nombre_anterior = nombre
        FROM dbo.METODO_PAGO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_metodo_pago = @id_metodo_pago
          AND id_empresa = @id_empresa;

        IF @nombre_anterior IS NULL
        BEGIN
            THROW 51022,
                'El método de pago no existe en la empresa.', 1;
        END;

        IF EXISTS (
            SELECT 1
            FROM dbo.METODO_PAGO WITH (UPDLOCK, HOLDLOCK)
            WHERE id_empresa = @id_empresa
              AND nombre = @nombre
              AND id_metodo_pago <> @id_metodo_pago
        )
        BEGIN
            THROW 51021,
                'Ya existe otro método de pago con ese nombre.', 1;
        END;

        UPDATE dbo.METODO_PAGO
        SET
            nombre = @nombre,
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
            'ACTUALIZAR',
            CONCAT('Nombre: ', @nombre_anterior),
            CONCAT('Nombre: ', @nombre)
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