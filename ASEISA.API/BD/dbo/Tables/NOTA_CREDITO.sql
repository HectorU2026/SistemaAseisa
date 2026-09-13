CREATE TABLE [dbo].[NOTA_CREDITO] (
    [id_nota_credito]     INT             IDENTITY (1, 1) NOT NULL,
    [id_factura]          INT             NOT NULL,
    [id_usuario]          INT             NOT NULL,
    [numero_nota_credito] VARCHAR (50)    NOT NULL,
    [fecha_emision]       DATETIME2 (7)   NOT NULL,
    [motivo]              VARCHAR (500)   NOT NULL,
    [monto]               DECIMAL (18, 2) NOT NULL,
    [fecha_creacion]      DATETIME2 (7)   DEFAULT (sysdatetime()) NOT NULL,
    [id_estado]           INT             NOT NULL,
    CONSTRAINT [PK_NOTA_CREDITO] PRIMARY KEY CLUSTERED ([id_nota_credito] ASC),
    CONSTRAINT [FK_NOTA_CREDITO_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_NOTA_CREDITO_FACTURA] FOREIGN KEY ([id_factura]) REFERENCES [dbo].[FACTURA] ([id_factura]),
    CONSTRAINT [FK_NOTA_CREDITO_USUARIO] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[USUARIO] ([id_usuario])
);

