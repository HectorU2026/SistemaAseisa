/*
Post-Deployment: catálogo ESTADO_FACTURA.
Idempotente por nombre_estado. No fija ids ni usa IDENTITY_INSERT.
*/
SET NOCOUNT ON;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.ESTADO_FACTURA
    WHERE nombre_estado = N'Borrador'
)
    INSERT dbo.ESTADO_FACTURA (nombre_estado)
    VALUES (N'Borrador');

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.ESTADO_FACTURA
    WHERE nombre_estado = N'Emitida'
)
    INSERT dbo.ESTADO_FACTURA (nombre_estado)
    VALUES (N'Emitida');

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.ESTADO_FACTURA
    WHERE nombre_estado = N'Inactiva'
)
    INSERT dbo.ESTADO_FACTURA (nombre_estado)
    VALUES (N'Inactiva');

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.ESTADO_FACTURA
    WHERE nombre_estado = N'Pagada'
)
    INSERT dbo.ESTADO_FACTURA (nombre_estado)
    VALUES (N'Pagada');
