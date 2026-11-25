CREATE   PROCEDURE dbo.ObtenerBalanceGeneralDetalle
    @id_empresa        INT,
    @fecha_corte_1     DATE,
    @fecha_corte_2     DATE = NULL,
    @incluir_sin_mov   BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH SaldosPeriodo1 AS (
        SELECT 
            cc.id_cuenta_contable, cc.codigo, cc.nombre, cc.tipo_cuenta, cc.naturaleza, cc.nivel,
            CASE 
                WHEN cc.naturaleza = 'Deudora' THEN ISNULL(SUM(da.debe - da.haber), 0)
                ELSE ISNULL(SUM(da.haber - da.debe), 0)
            END AS Saldo
        FROM dbo.CUENTA_CONTABLE cc
        LEFT JOIN dbo.DETALLE_ASIENTO da ON cc.id_cuenta_contable = da.id_cuenta_contable
        LEFT JOIN dbo.ASIENTO_CONTABLE ac ON da.id_asiento = ac.id_asiento
        WHERE cc.id_empresa = @id_empresa
          AND cc.permite_movimientos = 1
          AND (ac.fecha_asiento <= @fecha_corte_1 OR ac.fecha_asiento IS NULL)
        GROUP BY cc.id_cuenta_contable, cc.codigo, cc.nombre, cc.tipo_cuenta, cc.naturaleza, cc.nivel
    ),
    SaldosPeriodo2 AS (
        SELECT 
            cc.id_cuenta_contable,
            CASE 
                WHEN cc.naturaleza = 'Deudora' THEN ISNULL(SUM(da.debe - da.haber), 0)
                ELSE ISNULL(SUM(da.haber - da.debe), 0)
            END AS Saldo
        FROM dbo.CUENTA_CONTABLE cc
        LEFT JOIN dbo.DETALLE_ASIENTO da ON cc.id_cuenta_contable = da.id_cuenta_contable
        LEFT JOIN dbo.ASIENTO_CONTABLE ac ON da.id_asiento = ac.id_asiento
        WHERE cc.id_empresa = @id_empresa
          AND cc.permite_movimientos = 1
          AND @fecha_corte_2 IS NOT NULL
          AND (ac.fecha_asiento <= @fecha_corte_2 OR ac.fecha_asiento IS NULL)
        GROUP BY cc.id_cuenta_contable, cc.naturaleza
    )
    SELECT 
        p1.codigo AS CodigoCuenta,
        p1.nombre AS NombreCuenta,
        p1.tipo_cuenta AS Clasificacion,
        p1.nivel AS Nivel,
        p1.Saldo AS SaldoActual,
        ISNULL(p2.Saldo, 0) AS SaldoAnterior,
        (p1.Saldo - ISNULL(p2.Saldo, 0)) AS Variacion
    FROM SaldosPeriodo1 p1
    LEFT JOIN SaldosPeriodo2 p2 ON p1.id_cuenta_contable = p2.id_cuenta_contable
    WHERE 
        (@incluir_sin_mov = 1 OR p1.Saldo <> 0 OR ISNULL(p2.Saldo, 0) <> 0)
        AND p1.tipo_cuenta IN ('Activo', 'Pasivo', 'Patrimonio')
    ORDER BY 
        CASE p1.tipo_cuenta 
            WHEN 'Activo' THEN 1 
            WHEN 'Pasivo' THEN 2 
            WHEN 'Patrimonio' THEN 3 
            ELSE 4 
        END,
        p1.codigo;
END;