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
    [fecha_modificacion]         DATETIME2 (7) NULL,
    [id_cuenta_cxc]              INT           NULL,
    [id_cuenta_impuesto_ventas]  INT           NULL,
    CONSTRAINT [PK_EMPRESA] PRIMARY KEY CLUSTERED ([id_empresa] ASC),
    CONSTRAINT [FK_EMPRESA_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_EMPRESA_CUENTA_CXC] FOREIGN KEY ([id_cuenta_cxc]) REFERENCES [dbo].[CUENTA_CONTABLE] ([id_cuenta_contable]),
    CONSTRAINT [FK_EMPRESA_CUENTA_IMPUESTO_VENTAS] FOREIGN KEY ([id_cuenta_impuesto_ventas]) REFERENCES [dbo].[CUENTA_CONTABLE] ([id_cuenta_contable])
);

