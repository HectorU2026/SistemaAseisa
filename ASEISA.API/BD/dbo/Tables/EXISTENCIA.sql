CREATE TABLE [dbo].[EXISTENCIA] (
    [id_existencia]       INT             IDENTITY (1, 1) NOT NULL,
    [id_producto]         INT             NOT NULL,
    [id_bodega]           INT             NOT NULL,
    [cantidad_disponible] DECIMAL (18, 4) DEFAULT ((0)) NOT NULL,
    [cantidad_reservada]  DECIMAL (18, 4) DEFAULT ((0)) NOT NULL,
    [costo_promedio]      DECIMAL (18, 4) NULL,
    [fecha_actualizacion] DATETIME2 (7)   DEFAULT (sysdatetime()) NOT NULL,
    CONSTRAINT [PK_EXISTENCIA] PRIMARY KEY CLUSTERED ([id_existencia] ASC),
    CONSTRAINT [FK_EXISTENCIA_BODEGA] FOREIGN KEY ([id_bodega]) REFERENCES [dbo].[BODEGA] ([id_bodega]),
    CONSTRAINT [FK_EXISTENCIA_PRODUCTO] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[PRODUCTO] ([id_producto]),
    CONSTRAINT [UQ_EXISTENCIA_PRODUCTO_BODEGA] UNIQUE NONCLUSTERED ([id_producto] ASC, [id_bodega] ASC)
);

