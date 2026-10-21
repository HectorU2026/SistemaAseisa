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
    [id_termino_pago]        INT             NULL,
    [termino_pago_nombre]    NVARCHAR (100)  NULL,
    [dias_credito]           INT             NULL,
    [porcentaje_descuento]   DECIMAL (5, 2)  NULL,
    [monto_descuento]        DECIMAL (18, 2) NULL,
    [confirmacion_llegada]   BIT             NULL,
    [fecha_creacion]         DATETIME2 (7)   NULL,
    [fecha_modificacion]     DATETIME2 (7)   NULL,
    CONSTRAINT [PK_ORDEN_COMPRA] PRIMARY KEY CLUSTERED ([id_orden_compra] ASC),
    CONSTRAINT [CK_ORDEN_COMPRA_DESCUENTO_MONTO] CHECK ([monto_descuento] IS NULL OR [monto_descuento]>=(0)),
    CONSTRAINT [CK_ORDEN_COMPRA_DESCUENTO_PCT] CHECK ([porcentaje_descuento] IS NULL OR [porcentaje_descuento]>=(0) AND [porcentaje_descuento]<=(100)),
    CONSTRAINT [CK_ORDEN_COMPRA_DIAS_CREDITO] CHECK ([dias_credito] IS NULL OR [dias_credito]>=(0)),
    CONSTRAINT [FK_ORDEN_COMPRA_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_ORDEN_COMPRA_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_ORDEN_COMPRA_PROVEEDOR] FOREIGN KEY ([id_proveedor]) REFERENCES [dbo].[PROVEEDOR] ([id_proveedor]),
    CONSTRAINT [FK_ORDEN_COMPRA_TERMINO_EMPRESA] FOREIGN KEY ([id_termino_pago], [id_empresa]) REFERENCES [dbo].[TERMINO_PAGO] ([id_termino_pago], [id_empresa]),
    CONSTRAINT [FK_ORDEN_COMPRA_USUARIO] FOREIGN KEY ([id_usuario_solicitante]) REFERENCES [dbo].[USUARIO] ([id_usuario])
);


GO
CREATE NONCLUSTERED INDEX [IX_ORDEN_COMPRA_EMPRESA_FECHA]
    ON [dbo].[ORDEN_COMPRA]([id_empresa] ASC, [fecha_emision] DESC, [id_orden_compra] DESC)
    INCLUDE([id_proveedor], [numero_orden], [total], [id_estado]);

