CREATE PROCEDURE [dbo].[AgregarDistribucionAnalitica]
    @idEmpresa INT, @idUsuario INT,
    @idProveedor INT, @idProducto INT, @importe DECIMAL(18,2),
    @descripcion NVARCHAR(300) = NULL, @referencia NVARCHAR(100) = NULL, @unidad NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @anterior NVARCHAR(MAX), @nuevo NVARCHAR(MAX);
        DECLARE @idDistribucion INT;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.PROVEEDOR p WITH (HOLDLOCK)
            JOIN dbo.ESTADO e WITH (HOLDLOCK) ON e.id_estado = p.id_estado
            WHERE p.id_proveedor = @idProveedor AND p.id_empresa = @idEmpresa
              AND e.nombre_estado = 'Activo'
        ) THROW 51303, 'Seleccione un proveedor activo de su empresa.', 1;
        IF NOT EXISTS (
            SELECT 1 FROM dbo.PRODUCTO p WITH (HOLDLOCK)
            JOIN dbo.ESTADO e WITH (HOLDLOCK) ON e.id_estado = p.id_estado
            WHERE p.id_producto = @idProducto AND p.id_empresa = @idEmpresa
              AND e.nombre_estado = 'Activo'
        ) THROW 51304, 'Seleccione un producto activo de su empresa.', 1;
        IF @importe IS NULL
            THROW 51305, 'Escriba todos los datos requeridos.', 1;
        INSERT INTO dbo.DISTRIBUCION_ANALITICA
(
    id_empresa, id_proveedor, id_producto, importe,
    descripcion, referencia, unidad, fecha, id_estado, fecha_creacion
)
VALUES
(
    @idEmpresa, @idProveedor, @idProducto, @importe,
    @descripcion, @referencia, @unidad, SYSDATETIME(), 1, SYSDATETIME()
);
        SET @idDistribucion = CONVERT(INT,SCOPE_IDENTITY());
        SELECT @nuevo = (
            SELECT * FROM dbo.DISTRIBUCION_ANALITICA
            WHERE id_empresa = @idEmpresa AND id_distribucion_analitica = @idDistribucion
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );
        INSERT dbo.BITACORA (id_usuario,id_empresa,tabla_afectada,registro_id,accion,valor_anterior,valor_nuevo)
        VALUES (@idUsuario,@idEmpresa,'DISTRIBUCION_ANALITICA',CONVERT(VARCHAR(100),@idDistribucion),
                'AGREGAR',@anterior,@nuevo);
        COMMIT TRANSACTION;
        SELECT @idDistribucion;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;