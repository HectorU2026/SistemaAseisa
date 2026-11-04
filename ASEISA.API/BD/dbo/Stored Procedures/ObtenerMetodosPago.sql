/* ============================================================
   CONF-005 - ADMINISTRACIÓN DE MÉTODOS DE PAGO
   ============================================================ */

-- ============================================================
-- 1. OBTENER MÉTODOS DE PAGO
-- ============================================================

CREATE   PROCEDURE dbo.ObtenerMetodosPago
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
    BEGIN
        THROW 51008,
            'El usuario no pertenece a la empresa indicada.', 1;
    END;

    SELECT
        id_metodo_pago AS IdMetodoPago,
        id_empresa AS IdEmpresa,
        nombre AS Nombre,
        activo AS Activo,
        fecha_creacion AS FechaCreacion,
        fecha_modificacion AS FechaModificacion
    FROM dbo.METODO_PAGO
    WHERE id_empresa = @id_empresa
    ORDER BY activo DESC, nombre ASC;
END;