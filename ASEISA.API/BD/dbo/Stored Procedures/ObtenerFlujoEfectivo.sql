

CREATE   PROCEDURE dbo.ObtenerFlujoEfectivo
    @id_empresa   INT,
    @fecha_inicio DATE,
    @fecha_fin    DATE
AS
BEGIN
    SET NOCOUNT ON;
 
    IF @fecha_inicio IS NULL OR @fecha_fin IS NULL OR @fecha_inicio > @fecha_fin
        THROW 50001, 'Rango de fechas inválido.', 1;
 
    DECLARE @saldo_inicial DECIMAL(18,2);
    -- Una CTE solo existe para la sentencia siguiente. Materializar para ambos resultados.
    DECLARE @flujo TABLE (categoria VARCHAR(20) NOT NULL, monto DECIMAL(18,2) NULL);

    SELECT @saldo_inicial = ISNULL(SUM(da.debe - da.haber), 0)
    FROM dbo.DETALLE_ASIENTO da
    INNER JOIN dbo.ASIENTO_CONTABLE ac ON ac.id_asiento = da.id_asiento
    INNER JOIN dbo.CUENTA_CONTABLE cc ON cc.id_cuenta_contable = da.id_cuenta_contable
    WHERE ac.id_empresa = @id_empresa
      AND ac.fecha_asiento < @fecha_inicio
      AND cc.categoria_flujo = 'Efectivo';
    ;WITH Efectivo AS (
        SELECT 
            da.id_asiento,
            SUM(da.debe - da.haber) AS neto
        FROM dbo.DETALLE_ASIENTO da
        INNER JOIN dbo.ASIENTO_CONTABLE ac ON ac.id_asiento = da.id_asiento
        INNER JOIN dbo.CUENTA_CONTABLE cc ON cc.id_cuenta_contable = da.id_cuenta_contable
        WHERE ac.id_empresa = @id_empresa
          AND ac.fecha_asiento BETWEEN @fecha_inicio AND @fecha_fin
          AND cc.categoria_flujo = 'Efectivo'
        GROUP BY da.id_asiento
        HAVING SUM(da.debe - da.haber) <> 0
    ),
 
    Contra AS (
        SELECT 
            e.id_asiento,
            e.neto,
            COALESCE(cc.categoria_flujo, 'Sin clasificar') AS categoria,
            CASE WHEN e.neto > 0 THEN da.haber ELSE da.debe END AS peso
        FROM Efectivo e
        INNER JOIN dbo.DETALLE_ASIENTO da ON da.id_asiento = e.id_asiento
        INNER JOIN dbo.CUENTA_CONTABLE cc ON cc.id_cuenta_contable = da.id_cuenta_contable
        WHERE cc.categoria_flujo IS NULL OR cc.categoria_flujo <> 'Efectivo'
    ),
 
    Flujo AS (
        SELECT 
            categoria,
            CAST(neto * peso / SUM(peso) OVER (PARTITION BY id_asiento) AS DECIMAL(18,2)) AS monto
        FROM Contra
        WHERE peso > 0
    )

    INSERT INTO @flujo (categoria, monto)
    SELECT categoria, monto FROM Flujo;

    SELECT
        cat.Categoria,
        ISNULL(SUM(CASE WHEN f.monto > 0 THEN f.monto END), 0) AS Entradas,
        ISNULL(-SUM(CASE WHEN f.monto < 0 THEN f.monto END), 0) AS Salidas,
        ISNULL(SUM(f.monto), 0) AS FlujoNeto
    FROM (VALUES 
        (1, 'Operacion'), 
        (2, 'Inversion'), 
        (3, 'Financiamiento'), 
        (4, 'Sin clasificar')
    ) cat (Orden, Categoria)
    LEFT JOIN @flujo f ON f.categoria = cat.Categoria
    GROUP BY cat.Orden, cat.Categoria
    ORDER BY cat.Orden;
 
    -- 6. Resultado 2: saldo
    SELECT 
        @saldo_inicial AS SaldoInicial,
        ISNULL(SUM(monto), 0) AS VariacionNeta,
        @saldo_inicial + ISNULL(SUM(monto), 0) AS SaldoFinal
    FROM @flujo;
END;