CREATE TABLE [dbo].[DETALLE_FACTURA] (
    [id_factura]           INT             NOT NULL,
    [id_producto]          INT             NOT NULL,
    [cantidad]             DECIMAL (18, 4) NOT NULL,
    [precio_unitario]      DECIMAL (18, 2) NOT NULL,
    [porcentaje_descuento] DECIMAL (8, 4)  NULL,
    [porcentaje_impuesto]  DECIMAL (8, 4)  NULL,
    [subtotal]             DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_DETALLE_FACTURA] PRIMARY KEY CLUSTERED ([id_factura] ASC, [id_producto] ASC),
    CONSTRAINT [FK_DETALLE_FACTURA_FACTURA] FOREIGN KEY ([id_factura]) REFERENCES [dbo].[FACTURA] ([id_factura]),
    CONSTRAINT [FK_DETALLE_FACTURA_PRODUCTO] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[PRODUCTO] ([id_producto])
);

