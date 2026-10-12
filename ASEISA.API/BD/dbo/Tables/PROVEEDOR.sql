CREATE TABLE [dbo].[PROVEEDOR] (
    [id_proveedor]        INT           IDENTITY (1, 1) NOT NULL,
    [id_empresa]          INT           NOT NULL,
    [nombre_razon_social] VARCHAR (200) NOT NULL,
    [identificacion]      VARCHAR (50)  NOT NULL,
    [correo]              VARCHAR (150) NULL,
    [telefono]            VARCHAR (30)  NULL,
    [direccion]           VARCHAR (300) NULL,
    [dias_credito]        INT           NULL,
    [id_estado]           INT           NOT NULL,
    [id_contacto]         INT           NULL,
    CONSTRAINT [PK_PROVEEDOR] PRIMARY KEY CLUSTERED ([id_proveedor] ASC),
    CONSTRAINT [FK_PROVEEDOR_CONTACTO] FOREIGN KEY ([id_contacto], [id_empresa]) REFERENCES [dbo].[CONTACTO] ([id_contacto], [id_empresa]),
    CONSTRAINT [FK_PROVEEDOR_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_PROVEEDOR_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_PROVEEDOR_CONTACTO]
    ON [dbo].[PROVEEDOR]([id_contacto] ASC) WHERE ([id_contacto] IS NOT NULL);

