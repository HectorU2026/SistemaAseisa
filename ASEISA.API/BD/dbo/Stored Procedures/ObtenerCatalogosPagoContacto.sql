
CREATE PROCEDURE dbo.ObtenerCatalogosPagoContacto
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
        THROW 51008, 'El usuario no pertenece a la empresa indicada.', 1;

    INSERT dbo.BITACORA
        (id_usuario, id_empresa, tabla_afectada, registro_id,
         accion, valor_anterior, valor_nuevo)
    VALUES
        (@id_usuario, @id_empresa, 'EMPRESA',
         CONVERT(VARCHAR(100), @id_empresa),
         'CONSULTAR', NULL,
         '{"consulta":"Catalogos de pago de contactos"}');

    SELECT
        id_termino_pago AS IdTerminoPago,
        nombre AS Nombre,
        dias_credito AS DiasCredito
    FROM dbo.TERMINO_PAGO
    WHERE id_empresa = @id_empresa
      AND activo = 1
    ORDER BY nombre;

    SELECT
        id_metodo_pago AS IdMetodoPago,
        nombre AS Nombre
    FROM dbo.METODO_PAGO
    WHERE id_empresa = @id_empresa
      AND activo = 1
    ORDER BY nombre;
END;