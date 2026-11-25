CREATE TABLE [dbo].[CUENTA_POR_PAGAR] (
    [id_cuenta_pagar]   INT             IDENTITY (1, 1) NOT NULL,
    [id_orden_compra]   INT             NOT NULL,
    [id_proveedor]      INT             NOT NULL,
    [fecha_emision]     DATETIME2 (7)   NOT NULL,
    [fecha_vencimiento] DATETIME2 (7)   NOT NULL,
    [monto_original]    DECIMAL (18, 2) NOT NULL,
    [saldo_pendiente]   DECIMAL (18, 2) NOT NULL,
    [id_estado]         INT             NOT NULL,
    CONSTRAINT [PK_CUENTA_POR_PAGAR] PRIMARY KEY CLUSTERED ([id_cuenta_pagar] ASC),
    CONSTRAINT [FK_CUENTA_POR_PAGAR_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_CUENTA_POR_PAGAR_ORDEN] FOREIGN KEY ([id_orden_compra]) REFERENCES [dbo].[ORDEN_COMPRA] ([id_orden_compra]),
    CONSTRAINT [FK_CUENTA_POR_PAGAR_PROVEEDOR] FOREIGN KEY ([id_proveedor]) REFERENCES [dbo].[PROVEEDOR] ([id_proveedor])
);

