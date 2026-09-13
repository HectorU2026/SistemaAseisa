CREATE TABLE [dbo].[DISTRIBUCION_ANALITICA] (
    [id_distribucion_analitica] INT             IDENTITY (1, 1) NOT NULL,
    [id_empresa]                INT             NOT NULL,
    [id_proveedor]              INT             NOT NULL,
    [id_producto]               INT             NOT NULL,
    [descripcion]               VARCHAR (500)   NULL,
    [fecha]                     DATETIME2 (7)   NOT NULL,
    [importe]                   DECIMAL (18, 2) NOT NULL,
    [id_estado]                 INT             NOT NULL,
    [referencia]                VARCHAR (150)   NULL,
    [unidad]                    VARCHAR (100)   NULL,
    [fecha_creacion]            DATETIME2 (7)   DEFAULT (sysdatetime()) NOT NULL,
    [fecha_modificacion]        DATETIME2 (7)   NULL,
    CONSTRAINT [PK_DISTRIBUCION_ANALITICA] PRIMARY KEY CLUSTERED ([id_distribucion_analitica] ASC),
    CONSTRAINT [FK_DISTRIBUCION_ANALITICA_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_DISTRIBUCION_ANALITICA_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_DISTRIBUCION_ANALITICA_PRODUCTO] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[PRODUCTO] ([id_producto]),
    CONSTRAINT [FK_DISTRIBUCION_ANALITICA_PROVEEDOR] FOREIGN KEY ([id_proveedor]) REFERENCES [dbo].[PROVEEDOR] ([id_proveedor])
);

