
CREATE   PROCEDURE dbo.ObtenerLibroMayor  
    @id_empresa         INT,  
    @id_cuenta_contable INT,  
    @fecha_inicio       DATE,  
    @fecha_fin          DATE  
AS  
BEGIN  
    SET NOCOUNT ON;  
  
    IF @fecha_inicio IS NULL OR @fecha_fin IS NULL OR @fecha_inicio > @fecha_fin  
        THROW 50001, 'Rango de fechas inválido.', 1;  
  
    DECLARE @codigo VARCHAR(50), @nombre VARCHAR(150), @naturaleza VARCHAR(20);  
  
    SELECT @codigo = codigo, @nombre = nombre, @naturaleza = naturaleza  
    FROM dbo.CUENTA_CONTABLE  
    WHERE id_cuenta_contable = @id_cuenta_contable  
      AND id_empresa = @id_empresa;  
  
    IF @naturaleza IS NULL  
        THROW 50003, 'La cuenta contable no existe en la empresa.', 1;  
  
    DECLARE @saldo_inicial DECIMAL(18,2) =  
    (  
        SELECT ISNULL(SUM(CASE WHEN @naturaleza = 'Deudora' THEN da.debe - da.haber  
                               ELSE da.haber - da.debe END), 0)  
        FROM dbo.DETALLE_ASIENTO da  
        INNER JOIN dbo.ASIENTO_CONTABLE ac ON ac.id_asiento = da.id_asiento  
        INNER JOIN dbo.ESTADO e ON e.id_estado = ac.id_estado AND e.nombre_estado <> 'Borrador'  
        WHERE ac.id_empresa = @id_empresa  
          AND da.id_cuenta_contable = @id_cuenta_contable  
          AND ac.fecha_asiento < @fecha_inicio  
    );  
  
    SELECT  
        ac.id_asiento                                            AS IdAsiento,  
        ac.fecha_asiento                                         AS Fecha,  
        ac.numero_asiento                                        AS Referencia,  
        COALESCE(NULLIF(da.descripcion, ''), ac.descripcion)     AS Descripcion,  
        da.debe                                                  AS Debito,  
        da.haber                                                 AS Credito,  
        @saldo_inicial + SUM(CASE WHEN @naturaleza = 'Deudora' THEN da.debe - da.haber  
                                  ELSE da.haber - da.debe END)  
                         OVER (ORDER BY ac.fecha_asiento, ac.id_asiento  
                               ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS SaldoAcumulado  
    INTO #Mayor  
    FROM dbo.DETALLE_ASIENTO da  
    INNER JOIN dbo.ASIENTO_CONTABLE ac ON ac.id_asiento = da.id_asiento  
    INNER JOIN dbo.ESTADO e ON e.id_estado = ac.id_estado AND e.nombre_estado <> 'Borrador'  
    WHERE ac.id_empresa = @id_empresa  
      AND da.id_cuenta_contable = @id_cuenta_contable  
      AND ac.fecha_asiento BETWEEN @fecha_inicio AND @fecha_fin;  
  
    SELECT  
        @codigo                                   AS CodigoCuenta,  
        @nombre                                   AS NombreCuenta,  
        @naturaleza                               AS Naturaleza,  
        @saldo_inicial                            AS SaldoInicial,  
        ISNULL(SUM(Debito), 0)                    AS TotalDebito,  
        ISNULL(SUM(Credito), 0)                   AS TotalCredito,  
        @saldo_inicial + CASE WHEN @naturaleza = 'Deudora'  
                              THEN ISNULL(SUM(Debito), 0) - ISNULL(SUM(Credito), 0)  
                              ELSE ISNULL(SUM(Credito), 0) - ISNULL(SUM(Debito), 0) END AS SaldoFinal  
    FROM #Mayor;  
  
    SELECT Fecha, Referencia, Descripcion, Debito, Credito, SaldoAcumulado  
    FROM #Mayor  
    ORDER BY Fecha, IdAsiento;  
END;