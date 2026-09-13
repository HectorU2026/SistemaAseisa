CREATE TABLE [dbo].[PAGO_PROVEEDOR] (
    [id_pago_proveedor]  INT             IDENTITY (1, 1) NOT NULL,
    [id_cuenta_pagar]    INT             NOT NULL,
    [id_cuenta_bancaria] INT             NOT NULL,
    [fecha_pago]         DATETIME2 (7)   NOT NULL,
    [monto]              DECIMAL (18, 2) NOT NULL,
    [metodo_pago]        VARCHAR (50)    NULL,
    [referencia]         VARCHAR (150)   NULL,
    [id_estado]          INT             NOT NULL,
    CONSTRAINT [PK_PAGO_PROVEEDOR] PRIMARY KEY CLUSTERED ([id_pago_proveedor] ASC),
    CONSTRAINT [FK_PAGO_PROVEEDOR_CUENTA_BANCARIA] FOREIGN KEY ([id_cuenta_bancaria]) REFERENCES [dbo].[CUENTA_BANCARIA] ([id_cuenta_bancaria]),
    CONSTRAINT [FK_PAGO_PROVEEDOR_CUENTA_PAGAR] FOREIGN KEY ([id_cuenta_pagar]) REFERENCES [dbo].[CUENTA_POR_PAGAR] ([id_cuenta_pagar]),
    CONSTRAINT [FK_PAGO_PROVEEDOR_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

