CREATE TABLE [dbo].[BODEGA] (
    [id_bodega]  INT           IDENTITY (1, 1) NOT NULL,
    [id_empresa] INT           NOT NULL,
    [codigo]     VARCHAR (50)  NOT NULL,
    [nombre]     VARCHAR (150) NOT NULL,
    [ubicacion]  VARCHAR (300) NULL,
    [id_estado]  INT           NOT NULL,
    CONSTRAINT [PK_BODEGA] PRIMARY KEY CLUSTERED ([id_bodega] ASC),
    CONSTRAINT [FK_BODEGA_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_BODEGA_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

