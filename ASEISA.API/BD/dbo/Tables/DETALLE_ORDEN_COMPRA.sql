CREATE TABLE [dbo].[DETALLE_ORDEN_COMPRA] (
    [id_orden_compra] INT             NOT NULL,
    [id_producto]     INT             NOT NULL,
    [cantidad]        INT             NOT NULL,
    [precio_unitario] DECIMAL (18, 2) NOT NULL,
    [subtotal]        DECIMAL (18, 2) NULL,
    [porcentaje_iva]  DECIMAL (8, 4)  NULL,
    [monto_descuento] DECIMAL (18, 2) NULL,
    [impuesto]        DECIMAL (18, 2) NULL,
    [total]           DECIMAL (18, 2) NULL,
    CONSTRAINT [PK_DETALLE_ORDEN_COMPRA] PRIMARY KEY CLUSTERED ([id_orden_compra] ASC, [id_producto] ASC),
    CONSTRAINT [CK_DETALLE_COMPRA_DESCUENTO] CHECK ([monto_descuento] IS NULL OR [monto_descuento]>=(0)),
    CONSTRAINT [CK_DETALLE_COMPRA_IVA] CHECK ([impuesto] IS NULL OR [impuesto]>=(0)),
    CONSTRAINT [CK_DETALLE_COMPRA_IVA_PCT] CHECK ([porcentaje_iva] IS NULL OR [porcentaje_iva]>=(0)),
    CONSTRAINT [CK_DETALLE_COMPRA_TOTAL] CHECK ([total] IS NULL OR [total]>=(0)),
    CONSTRAINT [FK_DETALLE_ORDEN_COMPRA_ORDEN] FOREIGN KEY ([id_orden_compra]) REFERENCES [dbo].[ORDEN_COMPRA] ([id_orden_compra]),
    CONSTRAINT [FK_DETALLE_ORDEN_COMPRA_PRODUCTO] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[PRODUCTO] ([id_producto])
);

