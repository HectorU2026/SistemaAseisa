CREATE TABLE [dbo].[CUENTA_POR_COBRAR] (
    [id_cuenta_cobrar]  INT             IDENTITY (1, 1) NOT NULL,
    [id_venta]          INT             NOT NULL,
    [id_cliente]        INT             NOT NULL,
    [fecha_emision]     DATETIME2 (7)   NOT NULL,
    [fecha_vencimiento] DATETIME2 (7)   NOT NULL,
    [monto_original]    DECIMAL (18, 2) NOT NULL,
    [saldo_pendiente]   DECIMAL (18, 2) NOT NULL,
    [id_estado]         INT             NOT NULL,
    CONSTRAINT [PK_CUENTA_POR_COBRAR] PRIMARY KEY CLUSTERED ([id_cuenta_cobrar] ASC),
    CONSTRAINT [FK_CUENTA_POR_COBRAR_CLIENTE] FOREIGN KEY ([id_cliente]) REFERENCES [dbo].[CLIENTE] ([id_cliente]),
    CONSTRAINT [FK_CUENTA_POR_COBRAR_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_CUENTA_POR_COBRAR_VENTA] FOREIGN KEY ([id_venta]) REFERENCES [dbo].[VENTAS] ([id_venta])
);

