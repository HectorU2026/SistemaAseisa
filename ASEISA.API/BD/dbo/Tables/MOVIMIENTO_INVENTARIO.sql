CREATE TABLE [dbo].[MOVIMIENTO_INVENTARIO] (
    [id_movimiento_inventario] INT             IDENTITY (1, 1) NOT NULL,
    [id_producto]              INT             NOT NULL,
    [id_bodega]                INT             NOT NULL,
    [tipo_movimiento]          VARCHAR (50)    NOT NULL,
    [cantidad]                 DECIMAL (18, 4) NOT NULL,
    [costo_unitario]           DECIMAL (18, 4) NULL,
    [fecha_movimiento]         DATETIME2 (7)   NOT NULL,
    [documento_origen]         VARCHAR (100)   NULL,
    [observacion]              VARCHAR (500)   NULL,
    [id_usuario]               INT             NOT NULL,
    [id_estado]                INT             NOT NULL,
    CONSTRAINT [PK_MOVIMIENTO_INVENTARIO] PRIMARY KEY CLUSTERED ([id_movimiento_inventario] ASC),
    CONSTRAINT [FK_MOVIMIENTO_INVENTARIO_BODEGA] FOREIGN KEY ([id_bodega]) REFERENCES [dbo].[BODEGA] ([id_bodega]),
    CONSTRAINT [FK_MOVIMIENTO_INVENTARIO_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_MOVIMIENTO_INVENTARIO_PRODUCTO] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[PRODUCTO] ([id_producto]),
    CONSTRAINT [FK_MOVIMIENTO_INVENTARIO_USUARIO] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[USUARIO] ([id_usuario])
);

