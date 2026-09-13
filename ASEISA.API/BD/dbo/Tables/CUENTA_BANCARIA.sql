CREATE TABLE [dbo].[CUENTA_BANCARIA] (
    [id_cuenta_bancaria] INT             IDENTITY (1, 1) NOT NULL,
    [id_empresa]         INT             NOT NULL,
    [numero_cuenta]      VARCHAR (100)   NOT NULL,
    [tipo_cuenta]        VARCHAR (50)    NOT NULL,
    [moneda]             VARCHAR (10)    NOT NULL,
    [saldo_actual]       DECIMAL (18, 2) DEFAULT ((0)) NOT NULL,
    [id_cuenta_contable] INT             NOT NULL,
    [id_estado]          INT             NOT NULL,
    CONSTRAINT [PK_CUENTA_BANCARIA] PRIMARY KEY CLUSTERED ([id_cuenta_bancaria] ASC),
    CONSTRAINT [FK_CUENTA_BANCARIA_CUENTA_CONTABLE] FOREIGN KEY ([id_cuenta_contable]) REFERENCES [dbo].[CUENTA_CONTABLE] ([id_cuenta_contable]),
    CONSTRAINT [FK_CUENTA_BANCARIA_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_CUENTA_BANCARIA_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

