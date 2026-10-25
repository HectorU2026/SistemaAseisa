CREATE TABLE [dbo].[ESTADO_FACTURA] (
    [id_estado_factura] INT           IDENTITY (1, 1) NOT NULL,
    [nombre_estado]     VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_ESTADO_FACTURA] PRIMARY KEY CLUSTERED ([id_estado_factura] ASC),
    CONSTRAINT [UQ_ESTADO_FACTURA_NOMBRE] UNIQUE ([nombre_estado])
);
