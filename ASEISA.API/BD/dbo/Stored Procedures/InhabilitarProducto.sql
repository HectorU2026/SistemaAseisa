CREATE PROCEDURE InhabilitarProducto
    @id_producto INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Códigos: 1 éxito (incluye ya inactivo); 3 no existe; 8 catálogo ESTADO inválido.
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @id_estado_actual INT;
        DECLARE @id_inactivo INT;

        SELECT @id_estado_actual = id_estado
        FROM PRODUCTO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_producto = @id_producto;

        IF @id_estado_actual IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 3;
            RETURN;
        END;

        IF
        (
            SELECT COUNT(*)
            FROM ESTADO WITH (HOLDLOCK)
            WHERE LTRIM(RTRIM(nombre_estado)) = 'Activo'
        ) <> 1
        OR
        (
            SELECT COUNT(*)
            FROM ESTADO WITH (HOLDLOCK)
            WHERE LTRIM(RTRIM(nombre_estado)) = 'Inactivo'
        ) <> 1
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 8;
            RETURN;
        END;

        SELECT @id_inactivo = id_estado
        FROM ESTADO WITH (HOLDLOCK)
        WHERE LTRIM(RTRIM(nombre_estado)) = 'Inactivo';

        IF @id_estado_actual = @id_inactivo
        BEGIN
            COMMIT TRANSACTION;
            SELECT 1;
            RETURN;
        END;

        UPDATE PRODUCTO
        SET id_estado = @id_inactivo,
            fecha_modificacion = SYSDATETIME()
        WHERE id_producto = @id_producto;

        COMMIT TRANSACTION;
        SELECT 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
