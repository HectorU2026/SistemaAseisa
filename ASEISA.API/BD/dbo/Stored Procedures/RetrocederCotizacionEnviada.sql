
CREATE   PROCEDURE [dbo].[RetrocederCotizacionEnviada]
    @id_cotizacion INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_estado_enviada INT;
    DECLARE @id_estado_cotizacion INT;

    -- Buscar en la nueva tabla
    SELECT @id_estado_enviada = id_estado_cotizacion 
    FROM ESTADO_COTIZACION 
    WHERE nombre_estado = 'Cotización Enviada';

    SELECT @id_estado_cotizacion = id_estado_cotizacion 
    FROM ESTADO_COTIZACION 
    WHERE nombre_estado = 'Cotización';

    -- Validar usando la nueva columna
    IF NOT EXISTS (
        SELECT 1 
        FROM COTIZACION 
        WHERE id_cotizacion = @id_cotizacion 
          AND id_estado_cotizacion = @id_estado_enviada
    )
    BEGIN
        SELECT 0 AS FilasAfectadas;
        RETURN;
    END

    -- Actualizar la nueva columna
    UPDATE COTIZACION
    SET id_estado_cotizacion = @id_estado_cotizacion
    WHERE id_cotizacion = @id_cotizacion;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END;