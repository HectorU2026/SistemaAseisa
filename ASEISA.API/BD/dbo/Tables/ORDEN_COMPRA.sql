CREATE TABLE [dbo].[ORDEN_COMPRA] (
    [id_orden_compra]        INT             IDENTITY (1, 1) NOT NULL,
    [id_empresa]             INT             NOT NULL,
    [id_proveedor]           INT             NOT NULL,
    [id_usuario_solicitante] INT             NOT NULL,
    [numero_orden]           VARCHAR (50)    NOT NULL,
    [fecha_emision]          DATETIME2 (7)   NOT NULL,
    [fecha_requerida]        DATETIME2 (7)   NULL,
    [subtotal]               DECIMAL (18, 2) NOT NULL,
    [impuesto]               DECIMAL (18, 2) NOT NULL,
    [total]                  DECIMAL (18, 2) NOT NULL,
    [id_estado]              INT             NOT NULL,
    CONSTRAINT [PK_ORDEN_COMPRA] PRIMARY KEY CLUSTERED ([id_orden_compra] ASC),
    CONSTRAINT [FK_ORDEN_COMPRA_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_ORDEN_COMPRA_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_ORDEN_COMPRA_PROVEEDOR] FOREIGN KEY ([id_proveedor]) REFERENCES [dbo].[PROVEEDOR] ([id_proveedor]),
    CONSTRAINT [FK_ORDEN_COMPRA_USUARIO] FOREIGN KEY ([id_usuario_solicitante]) REFERENCES [dbo].[USUARIO] ([id_usuario])
);

