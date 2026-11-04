CREATE PROCEDURE [dbo].[ObtenerCuentasPorCobrar]
    @id_empresa INT,
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Insertar en bitácora la acción de consulta
    INSERT INTO dbo.BITACORA (id_usuario, id_empresa, tabla_afectada, accion)
    VALUES (@id_usuario, @id_empresa, 'CUENTA_POR_COBRAR', 'CONSULTAR_LISTADO');

    -- 2. Retornar el listado de cuentas por cobrar con días de atraso
    SELECT 
        cpc.id_cuenta_cobrar,
        c.nombre_razon_social AS nombre_cliente,
        f.numero_factura,
        cpc.fecha_emision,
        cpc.fecha_vencimiento,
        cpc.monto_original,
        cpc.saldo_pendiente,
        CASE 
            WHEN cpc.saldo_pendiente > 0 AND CAST(cpc.fecha_vencimiento AS DATE) < CAST(SYSDATETIME() AS DATE) THEN 'Atrasado'
            ELSE 'Al día'
        END AS estado_morosidad,
        CASE 
            WHEN CAST(cpc.fecha_vencimiento AS DATE) < CAST(SYSDATETIME() AS DATE) 
            THEN DATEDIFF(DAY, cpc.fecha_vencimiento, SYSDATETIME())
            ELSE 0 
        END AS dias_atraso,
        e.nombre_estado AS estado_registro
    FROM dbo.CUENTA_POR_COBRAR cpc WITH (NOLOCK)
    INNER JOIN dbo.CLIENTE c WITH (NOLOCK) ON cpc.id_cliente = c.id_cliente
    INNER JOIN dbo.VENTAS v WITH (NOLOCK) ON cpc.id_venta = v.id_venta
    INNER JOIN dbo.FACTURA f WITH (NOLOCK) ON v.id_venta = f.id_venta
    INNER JOIN dbo.ESTADO e WITH (NOLOCK) ON cpc.id_estado = e.id_estado
    WHERE c.id_empresa = @id_empresa
      AND cpc.saldo_pendiente > 0
    ORDER BY c.nombre_razon_social ASC, cpc.fecha_vencimiento ASC;
END;