CREATE PROCEDURE dbo.InhabilitarDistribucionAnalitica
    @idEmpresa INT, @idUsuario INT, @idDistribucion INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @anterior NVARCHAR(MAX), @nuevo NVARCHAR(MAX);
        SELECT @anterior = (
            SELECT * FROM dbo.DISTRIBUCION_ANALITICA WITH (UPDLOCK, HOLDLOCK)
            WHERE id_empresa = @idEmpresa AND id_distribucion_analitica = @idDistribucion
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );
        IF TRY_CONVERT(INT, JSON_VALUE(@anterior, '$.id_estado')) = 2
    THROW 51302, 'La distribución analítica ya está inactiva.', 1;

UPDATE dbo.DISTRIBUCION_ANALITICA
SET id_estado = 2,
    fecha_modificacion = SYSDATETIME()
WHERE id_empresa = @idEmpresa
  AND id_distribucion_analitica = @idDistribucion;
        SELECT @nuevo = (
            SELECT * FROM dbo.DISTRIBUCION_ANALITICA
            WHERE id_empresa = @idEmpresa AND id_distribucion_analitica = @idDistribucion
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );
        INSERT dbo.BITACORA (id_usuario,id_empresa,tabla_afectada,registro_id,accion,valor_anterior,valor_nuevo)
        VALUES (@idUsuario,@idEmpresa,'DISTRIBUCION_ANALITICA',CONVERT(VARCHAR(100),@idDistribucion),
                'INHABILITAR',@anterior,@nuevo);
        COMMIT TRANSACTION;
        SELECT @idDistribucion;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;