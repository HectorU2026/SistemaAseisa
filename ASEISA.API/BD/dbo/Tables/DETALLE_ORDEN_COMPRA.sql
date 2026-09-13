CREATE TABLE [dbo].[DETALLE_ORDEN_COMPRA] (
    [id_orden_compra] INT             NOT NULL,
    [id_producto]     INT             NOT NULL,
    [cantidad]        INT             NOT NULL,
    [precio_unitario] DECIMAL (18, 2) NOT NULL,
    [subtotal]        DECIMAL (18, 2) NULL,
    CONSTRAINT [PK_DETALLE_ORDEN_COMPRA] PRIMARY KEY CLUSTERED ([id_orden_compra] ASC, [id_producto] ASC),
    CONSTRAINT [FK_DETALLE_ORDEN_COMPRA_ORDEN] FOREIGN KEY ([id_orden_compra]) REFERENCES [dbo].[ORDEN_COMPRA] ([id_orden_compra]),
    CONSTRAINT [FK_DETALLE_ORDEN_COMPRA_PRODUCTO] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[PRODUCTO] ([id_producto])
);

