CREATE TABLE [dbo].[CONFIGURACION_CONTABLE] (
    [id_configuracion]   INT          IDENTITY (1, 1) NOT NULL,
    [id_empresa]         INT          NOT NULL,
    [clave]              VARCHAR (50) NOT NULL,
    [id_cuenta_contable] INT          NOT NULL,
    CONSTRAINT [PK_CONFIGURACION_CONTABLE] PRIMARY KEY CLUSTERED ([id_configuracion] ASC),
    CONSTRAINT [FK_CONFIG_CONTABLE_CUENTA] FOREIGN KEY ([id_cuenta_contable]) REFERENCES [dbo].[CUENTA_CONTABLE] ([id_cuenta_contable]),
    CONSTRAINT [FK_CONFIG_CONTABLE_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [UQ_CONFIGURACION_CONTABLE] UNIQUE NONCLUSTERED ([id_empresa] ASC, [clave] ASC)
);

