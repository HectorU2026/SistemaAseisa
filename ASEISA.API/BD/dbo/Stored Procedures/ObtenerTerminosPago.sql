CREATE   PROCEDURE dbo.ObtenerTerminosPago
    @id_empresa INT,
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.USUARIO
        WHERE id_usuario = @id_usuario
          AND id_empresa = @id_empresa
    )
        THROW 51008,
            'El usuario no pertenece a la empresa indicada.', 1;

    SELECT
        id_termino_pago AS IdTerminoPago,
        id_empresa AS IdEmpresa,
        nombre AS Nombre,
        dias_credito AS DiasCredito,
        activo AS Activo,
        fecha_creacion AS FechaCreacion,
        fecha_modificacion AS FechaModificacion
    FROM dbo.TERMINO_PAGO
    WHERE id_empresa = @id_empresa
    ORDER BY activo DESC, dias_credito ASC, nombre ASC;
END;