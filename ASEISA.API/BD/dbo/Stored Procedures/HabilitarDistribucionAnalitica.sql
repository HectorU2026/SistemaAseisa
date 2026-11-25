CREATE   PROCEDURE dbo.HabilitarDistribucionAnalitica
    @idEmpresa INT,
    @idUsuario INT,
    @idDistribucion INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @estado INT,
                @anterior NVARCHAR(MAX),
                @nuevo NVARCHAR(MAX);

        SELECT @estado = id_estado
        FROM dbo.DISTRIBUCION_ANALITICA WITH (UPDLOCK, HOLDLOCK)
        WHERE id_empresa = @idEmpresa
          AND id_distribucion_analitica = @idDistribucion;

        IF @estado IS NULL
            THROW 51301, 'La distribución analítica no existe.', 1;

        IF @estado <> 2
            THROW 51306, 'Solo se pueden habilitar distribuciones inactivas.', 1;

        SELECT @anterior = (
            SELECT *
            FROM dbo.DISTRIBUCION_ANALITICA
            WHERE id_empresa = @idEmpresa
              AND id_distribucion_analitica = @idDistribucion
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        );

        UPDATE dbo.DISTRIBUCION_ANALITICA
        SET id_estado = 1,
            fecha_modificacion = SYSDATETIME()
        WHERE id_empresa = @idEmpresa
          AND id_distribucion_analitica = @idDistribucion;

        SELECT @nuevo = (
            SELECT *
            FROM dbo.DISTRIBUCION_ANALITICA
            WHERE id_empresa = @idEmpresa
              AND id_distribucion_analitica = @idDistribucion
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
        );

        INSERT INTO dbo.BITACORA
            (id_usuario, id_empresa, tabla_afectada, registro_id,
             accion, valor_anterior, valor_nuevo)
        VALUES
            (@idUsuario, @idEmpresa, 'DISTRIBUCION_ANALITICA',
             CONVERT(VARCHAR(100), @idDistribucion),
             'HABILITAR', @anterior, @nuevo);

        COMMIT TRANSACTION;

        SELECT @idDistribucion;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;