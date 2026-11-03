

CREATE   PROCEDURE dbo.ObtenerCuentasBancarias
    @id_empresa INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        cb.id_cuenta_bancaria AS IdCuentaBancaria,
        cb.id_empresa         AS IdEmpresa,
        cb.numero_cuenta      AS NumeroCuenta,
        cb.id_tipo_cuenta     AS IdTipoCuenta,
        tc.nombre             AS TipoCuenta,
        cb.moneda             AS Moneda,
        cb.saldo_actual       AS SaldoActual,
        cb.id_cuenta_contable AS IdCuentaContable,
        cc.nombre             AS NombreCuentaContable,
        cb.id_estado          AS IdEstado
    FROM CUENTA_BANCARIA cb
    INNER JOIN TIPO_CUENTA tc ON tc.id_tipo_cuenta = cb.id_tipo_cuenta
    INNER JOIN CUENTA_CONTABLE cc ON cc.id_cuenta_contable = cb.id_cuenta_contable
    WHERE cb.id_empresa = @id_empresa
    ORDER BY cb.id_cuenta_bancaria DESC;
END;