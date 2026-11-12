-- Lista, detalle y selector de cuentas principales.
CREATE   PROCEDURE dbo.ConsultarCuentasContables
    @idEmpresa INT,
    @idCuentaContable INT = NULL,
    @soloActivas BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.id_cuenta_contable AS IdCuentaContable,
           c.codigo AS Codigo,
           c.nombre AS Nombre,
           c.tipo_cuenta AS TipoCuenta,
           c.naturaleza AS Naturaleza,
           c.nivel AS Nivel,
           c.permite_movimientos AS PermiteMovimientos,
           c.id_estado AS IdEstado,
           e.nombre_estado AS Estado,
           c.id_cuenta_principal AS IdCuentaPrincipal,
           p.nombre AS CuentaPrincipal,
           c.conciliacion_pagos AS ConciliacionPagos,
           c.impuestos AS Impuestos,
           c.descripcion AS Descripcion
    FROM dbo.CUENTA_CONTABLE c
    INNER JOIN dbo.ESTADO e ON e.id_estado = c.id_estado
    LEFT JOIN dbo.CUENTA_CONTABLE p
        ON p.id_cuenta_contable = c.id_cuenta_principal
       AND p.id_empresa = c.id_empresa
    WHERE c.id_empresa = @idEmpresa
      AND (@idCuentaContable IS NULL
           OR c.id_cuenta_contable = @idCuentaContable)
      AND (@soloActivas = 0 OR c.id_estado = 1)
    ORDER BY c.codigo, c.id_cuenta_contable;
END;