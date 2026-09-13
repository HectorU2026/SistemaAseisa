CREATE TABLE [dbo].[CLIENTE] (
    [id_cliente]          INT             IDENTITY (1, 1) NOT NULL,
    [id_empresa]          INT             NOT NULL,
    [tipo_cliente]        VARCHAR (50)    NOT NULL,
    [nombre_razon_social] VARCHAR (200)   NOT NULL,
    [identificacion]      VARCHAR (50)    NOT NULL,
    [correo]              VARCHAR (150)   NULL,
    [telefono]            VARCHAR (30)    NULL,
    [direccion]           VARCHAR (300)   NULL,
    [limite_credito]      DECIMAL (18, 2) NULL,
    [dias_credito]        INT             NULL,
    [id_estado]           INT             NOT NULL,
    [fecha_creacion]      DATETIME2 (7)   DEFAULT (sysdatetime()) NOT NULL,
    [fecha_modificacion]  DATETIME2 (7)   NULL,
    CONSTRAINT [PK_CLIENTE] PRIMARY KEY CLUSTERED ([id_cliente] ASC),
    CONSTRAINT [FK_CLIENTE_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_CLIENTE_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

