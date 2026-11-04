CREATE PROCEDURE [dbo].[ObtenerHistorialDepreciacion]
    @id_empresa INT,
    @id_activo_fijo INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        d.id_depreciacion,
        d.id_activo_fijo,
        a.nombre AS nombre_activo,
        a.costo_adquisicion,
        d.periodo,
        d.monto_despreciacion,
        d.depreciacion_acumulada,
        d.valor_en_libros,
        d.fecha_calculo,
        e.nombre_estado AS estado
    FROM dbo.DEPRECIACION d WITH (NOLOCK)
    INNER JOIN dbo.ACTIVO_FIJO a WITH (NOLOCK) ON d.id_activo_fijo = a.id_activo_fijo
    INNER JOIN dbo.ESTADO e WITH (NOLOCK) ON d.id_estado = e.id_estado
    WHERE a.id_empresa = @id_empresa 
      AND d.id_activo_fijo = @id_activo_fijo
    ORDER BY d.fecha_calculo DESC;
END;