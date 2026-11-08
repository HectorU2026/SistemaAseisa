CREATE PROCEDURE [dbo].[ObtenerMovimientosBancarios]
    @id_empresa INT,
    @id_cuenta_bancaria INT = NULL, 
    @pagina INT = 1,
    @tamano_pagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@pagina - 1) * @tamano_pagina;

    -- 1. Retornar el total de registros 
    SELECT COUNT(*) 
    FROM dbo.MOVIMIENTO_BANCARIO mb WITH (NOLOCK)
    INNER JOIN dbo.CUENTA_BANCARIA cb WITH (NOLOCK) ON mb.id_cuenta_bancaria = cb.id_cuenta_bancaria
    WHERE cb.id_empresa = @id_empresa
      AND (@id_cuenta_bancaria IS NULL OR mb.id_cuenta_bancaria = @id_cuenta_bancaria);

    -- 2. Retornar los registros de la página solicitada
    SELECT 
        mb.id_movimiento_bancario,
        cb.numero_cuenta,
        mb.fecha_movimiento,
        mb.tipo_movimiento,
        mb.monto,
        mb.descripcion,
        mb.referencia,
        e.nombre_estado AS estado
    FROM dbo.MOVIMIENTO_BANCARIO mb WITH (NOLOCK)
    INNER JOIN dbo.CUENTA_BANCARIA cb WITH (NOLOCK) ON mb.id_cuenta_bancaria = cb.id_cuenta_bancaria
    INNER JOIN dbo.ESTADO e WITH (NOLOCK) ON mb.id_estado = e.id_estado
    WHERE cb.id_empresa = @id_empresa
      AND (@id_cuenta_bancaria IS NULL OR mb.id_cuenta_bancaria = @id_cuenta_bancaria)
    ORDER BY mb.fecha_movimiento DESC, mb.id_movimiento_bancario DESC -- De más reciente a más antiguo
    OFFSET @Offset ROWS FETCH NEXT @tamano_pagina ROWS ONLY;
END;