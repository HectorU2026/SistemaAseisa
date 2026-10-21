CREATE TABLE [dbo].[INVENTARIO_FISICO] (
    [id_inventario_fisico] INT           IDENTITY (1, 1) NOT NULL,
    [id_producto]          INT           NOT NULL,
    [id_bodega]            INT           NOT NULL,
    [id_usuario]           INT           NOT NULL,
    [cantidad_inventario]  DECIMAL (18, 4) NOT NULL,
    [contado_fisicamente]  DECIMAL (18, 4) NOT NULL,
    [diferencia]           DECIMAL (18, 4) NOT NULL,
    [fecha_ajuste]         DATETIME2 (7) DEFAULT (sysdatetime()) NOT NULL,
    CONSTRAINT [PK_INVENTARIO_FISICO] PRIMARY KEY CLUSTERED ([id_inventario_fisico] ASC),
    CONSTRAINT [CK_INVENTARIO_FISICO_CONTADO] CHECK ([contado_fisicamente]>=(0)),
    CONSTRAINT [FK_INVENTARIO_FISICO_BODEGA] FOREIGN KEY ([id_bodega]) REFERENCES [dbo].[BODEGA] ([id_bodega]),
    CONSTRAINT [FK_INVENTARIO_FISICO_PRODUCTO] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[PRODUCTO] ([id_producto]),
    CONSTRAINT [FK_INVENTARIO_FISICO_USUARIO] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[USUARIO] ([id_usuario])
);
