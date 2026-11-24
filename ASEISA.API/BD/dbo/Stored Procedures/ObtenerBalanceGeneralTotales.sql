

CREATE   PROCEDURE [dbo].[ObtenerBalanceGeneralTotales]
    @id_empresa        INT,
    @fecha_corte_1     DATE,
    @fecha_corte_2     DATE = NULL,
    @incluir_sin_mov   BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @es_provisional BIT = 1;
    DECLARE @periodo_cerrado INT;

    SELECT TOP 1 @periodo_cerrado = id_estado
    FROM dbo.PERIODO_CONTABLE
    WHERE id_empresa = @id_empresa
      AND @fecha_corte_1 BETWEEN fecha_inicio AND fecha_fin;

    IF @periodo_cerrado IS NOT NULL AND @periodo_cerrado = (SELECT id_estado FROM dbo.ESTADO WHERE nombre_estado = 'Cerrado')
        SET @es_provisional = 0;

    ;WITH SaldosPeriodo1 AS (
        SELECT 
            cc.id_cuenta_contable, cc.tipo_cuenta, cc.naturaleza,
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
        GROUP BY cc.id_cuenta_contable, cc.tipo_cuenta, cc.naturaleza
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
    ),
    -- 1. Saldos base (Activo, Pasivo, Patrimonio) + Utilidad en una sola fila por clasificación
    Totales AS (
        SELECT 
            p1.tipo_cuenta AS Clasificacion,
            SUM(p1.Saldo) AS SaldoActual,
            SUM(ISNULL(p2.Saldo, 0)) AS SaldoAnterior,
            SUM(p1.Saldo - ISNULL(p2.Saldo, 0)) AS Variacion
        FROM SaldosPeriodo1 p1
        LEFT JOIN SaldosPeriodo2 p2 ON p1.id_cuenta_contable = p2.id_cuenta_contable
        WHERE 
            (@incluir_sin_mov = 1 OR p1.Saldo <> 0 OR ISNULL(p2.Saldo, 0) <> 0)
            AND p1.tipo_cuenta IN ('Activo', 'Pasivo', 'Patrimonio')
        GROUP BY p1.tipo_cuenta

        UNION ALL

        -- Utilidad del Ejercicio (Ingresos - Gastos) se suma al Patrimonio
        SELECT 
            'Patrimonio' AS Clasificacion,
            ISNULL(SUM(CASE 
                WHEN p1.tipo_cuenta = 'Ingreso' THEN p1.Saldo 
                WHEN p1.tipo_cuenta = 'Gasto' THEN -p1.Saldo
                ELSE 0 
            END), 0) AS SaldoActual,
            0 AS SaldoAnterior,
            0 AS Variacion
        FROM SaldosPeriodo1 p1
        WHERE p1.tipo_cuenta IN ('Ingreso', 'Gasto')
    )
    -- 2. Agrupar el resultado final para consolidar Patrimonio + Utilidad
    SELECT 
        Clasificacion,
        SUM(SaldoActual) AS SaldoActual,
        SUM(SaldoAnterior) AS SaldoAnterior,
        SUM(Variacion) AS Variacion,
        @es_provisional AS EsProvisional
    FROM Totales
    GROUP BY Clasificacion
    ORDER BY Clasificacion;
END;