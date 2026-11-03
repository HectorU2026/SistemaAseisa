
CREATE   PROCEDURE dbo.ObtenerTiposCuenta
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        tc.id_tipo_cuenta AS IdTipoCuenta,
        tc.nombre         AS Nombre
    FROM TIPO_CUENTA tc
    INNER JOIN ESTADO e ON e.id_estado = tc.id_estado
    WHERE e.nombre_estado = 'Activo'
    ORDER BY tc.nombre;
END;