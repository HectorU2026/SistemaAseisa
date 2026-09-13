CREATE TABLE [dbo].[PRODUCTO] (
    [id_producto]        INT             IDENTITY (1, 1) NOT NULL,
    [id_empresa]         INT             NOT NULL,
    [codigo]             VARCHAR (50)    NOT NULL,
    [nombre]             VARCHAR (150)   NOT NULL,
    [descripcion]        VARCHAR (300)   NULL,
    [tipo_producto]      VARCHAR (50)    NOT NULL,
    [unidad_medida]      VARCHAR (50)    NULL,
    [costo_promedio]     DECIMAL (18, 4) NULL,
    [precio_venta]       DECIMAL (18, 2) NOT NULL,
    [inventariable]      BIT             DEFAULT ((1)) NOT NULL,
    [id_estado]          INT             NOT NULL,
    [fecha_creacion]     DATETIME2 (7)   DEFAULT (sysdatetime()) NOT NULL,
    [fecha_modificacion] DATETIME2 (7)   NULL,
    CONSTRAINT [PK_PRODUCTO] PRIMARY KEY CLUSTERED ([id_producto] ASC),
    CONSTRAINT [FK_PRODUCTO_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_PRODUCTO_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

