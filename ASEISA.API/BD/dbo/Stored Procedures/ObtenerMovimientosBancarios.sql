
CREATE   PROCEDURE [dbo].[ObtenerMovimientosBancarios]
    @id_empresa INT,
    @id_cuenta_bancaria INT = NULL,
    @pagina INT = 1,
    @tamano_pagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    -- Defensa: valores inválidos
    IF @pagina IS NULL OR @pagina < 1 SET @pagina = 1;
    IF @tamano_pagina IS NULL OR @tamano_pagina < 1 SET @tamano_pagina = 10;
    IF @tamano_pagina > 100 SET @tamano_pagina = 100;

    DECLARE @Offset INT = (@pagina - 1) * @tamano_pagina;

    -- 1. Total de registros
    SELECT COUNT(*)
    FROM dbo.MOVIMIENTO_BANCARIO mb WITH (NOLOCK)
    INNER JOIN dbo.CUENTA_BANCARIA cb WITH (NOLOCK)
        ON mb.id_cuenta_bancaria = cb.id_cuenta_bancaria
    WHERE cb.id_empresa = @id_empresa
      AND (@id_cuenta_bancaria IS NULL OR mb.id_cuenta_bancaria = @id_cuenta_bancaria);

    -- 2. Página solicitada (aliases = modelo MovimientoBancarioItem)
    SELECT
        mb.id_movimiento_bancario AS IdMovimientoBancario,
        cb.numero_cuenta         AS NumeroCuenta,
        mb.fecha_movimiento      AS FechaMovimiento,
        mb.tipo_movimiento       AS TipoMovimiento,
        mb.monto                 AS Monto,
        mb.descripcion           AS Descripcion,
        mb.referencia            AS Referencia,
        e.nombre_estado          AS Estado
    FROM dbo.MOVIMIENTO_BANCARIO mb WITH (NOLOCK)
    INNER JOIN dbo.CUENTA_BANCARIA cb WITH (NOLOCK)
        ON mb.id_cuenta_bancaria = cb.id_cuenta_bancaria
    INNER JOIN dbo.ESTADO e WITH (NOLOCK)      -- TODO: cambiar a ESTADO_MOVIMIENTO_BANCARIO
        ON mb.id_estado = e.id_estado
    WHERE cb.id_empresa = @id_empresa
      AND (@id_cuenta_bancaria IS NULL OR mb.id_cuenta_bancaria = @id_cuenta_bancaria)
    ORDER BY mb.fecha_movimiento DESC, mb.id_movimiento_bancario DESC
    OFFSET @Offset ROWS FETCH NEXT @tamano_pagina ROWS ONLY;
END;