CREATE TABLE [dbo].[DETALLE_COTIZACION] (
    [id_detalle_cotizacion] INT             IDENTITY (1, 1) NOT NULL,
    [id_cotizacion]         INT             NOT NULL,
    [id_producto]           INT             NOT NULL,
    [cantidad]              DECIMAL (18, 4) NOT NULL,
    [precio_unitario]       DECIMAL (18, 2) NOT NULL,
    [porcentaje_descuento]  DECIMAL (8, 4)  NULL,
    [porcentaje_impuesto]   DECIMAL (8, 4)  NULL,
    [subtotal]              DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_DETALLE_COTIZACION] PRIMARY KEY CLUSTERED ([id_detalle_cotizacion] ASC),
    CONSTRAINT [FK_DETALLE_COTIZACION_COTIZACION] FOREIGN KEY ([id_cotizacion]) REFERENCES [dbo].[COTIZACION] ([id_cotizacion]),
    CONSTRAINT [FK_DETALLE_COTIZACION_PRODUCTO] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[PRODUCTO] ([id_producto])
);

