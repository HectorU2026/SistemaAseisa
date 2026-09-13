CREATE TABLE [dbo].[EMPRESA] (
    [id_empresa]         INT           IDENTITY (1, 1) NOT NULL,
    [razon_social]       VARCHAR (200) NOT NULL,
    [nombre_comercial]   VARCHAR (200) NULL,
    [cedula_juridica]    VARCHAR (30)  NOT NULL,
    [correo]             VARCHAR (150) NULL,
    [telefono]           VARCHAR (30)  NULL,
    [direccion]          VARCHAR (300) NULL,
    [moneda_base]        VARCHAR (10)  NOT NULL,
    [id_estado]          INT           NOT NULL,
    [fecha_creacion]     DATETIME2 (7) DEFAULT (sysdatetime()) NOT NULL,
    [fecha_modificacion] DATETIME2 (7) NULL,
    CONSTRAINT [PK_EMPRESA] PRIMARY KEY CLUSTERED ([id_empresa] ASC),
    CONSTRAINT [FK_EMPRESA_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

