CREATE TABLE [dbo].[PAGO_CLIENTE] (
    [id_pago_cliente]    INT             IDENTITY (1, 1) NOT NULL,
    [id_cuenta_cobrar]   INT             NOT NULL,
    [id_cuenta_bancaria] INT             NULL,
    [fecha_pago]         DATETIME2 (7)   NOT NULL,
    [monto]              DECIMAL (18, 2) NOT NULL,
    [metodo_pago]        VARCHAR (50)    NOT NULL,
    [referencia]         VARCHAR (150)   NULL,
    [id_estado]          INT             NOT NULL,
    CONSTRAINT [PK_PAGO_CLIENTE] PRIMARY KEY CLUSTERED ([id_pago_cliente] ASC),
    CONSTRAINT [FK_PAGO_CLIENTE_CUENTA_BANCARIA] FOREIGN KEY ([id_cuenta_bancaria]) REFERENCES [dbo].[CUENTA_BANCARIA] ([id_cuenta_bancaria]),
    CONSTRAINT [FK_PAGO_CLIENTE_CUENTA_COBRAR] FOREIGN KEY ([id_cuenta_cobrar]) REFERENCES [dbo].[CUENTA_POR_COBRAR] ([id_cuenta_cobrar]),
    CONSTRAINT [FK_PAGO_CLIENTE_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

