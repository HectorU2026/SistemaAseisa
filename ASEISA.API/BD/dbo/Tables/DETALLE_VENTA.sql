CREATE TABLE [dbo].[DETALLE_VENTA] (
    [id_detalle_venta]     INT             IDENTITY (1, 1) NOT NULL,
    [id_venta]             INT             NOT NULL,
    [id_producto]          INT             NOT NULL,
    [cantidad]             DECIMAL (18, 4) NOT NULL,
    [precio_unitario]      DECIMAL (18, 2) NOT NULL,
    [porcentaje_descuento] DECIMAL (8, 4)  NULL,
    [porcentaje_impuesto]  DECIMAL (8, 4)  NULL,
    [subtotal]             DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_DETALLE_VENTA] PRIMARY KEY CLUSTERED ([id_detalle_venta] ASC),
    CONSTRAINT [FK_DETALLE_VENTA_PRODUCTO] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[PRODUCTO] ([id_producto]),
    CONSTRAINT [FK_DETALLE_VENTA_VENTAS] FOREIGN KEY ([id_venta]) REFERENCES [dbo].[VENTAS] ([id_venta])
);

