CREATE TABLE [dbo].[CONTACTO] (
    [id_contacto]             INT            IDENTITY (1, 1) NOT NULL,
    [id_empresa]              INT            NOT NULL,
    [cedula]                  NVARCHAR (50)  NULL,
    [identificacion_fiscal]   NVARCHAR (50)  NULL,
    [nombre]                  NVARCHAR (200) NOT NULL,
    [correo]                  NVARCHAR (150) NOT NULL,
    [telefono]                NVARCHAR (30)  NOT NULL,
    [pais]                    NVARCHAR (100) NOT NULL,
    [provincia]               NVARCHAR (100) NULL,
    [canton]                  NVARCHAR (100) NULL,
    [distrito]                NVARCHAR (100) NULL,
    [detalle_direccion]       NVARCHAR (300) NOT NULL,
    [rol_contacto]            VARCHAR (10)   NOT NULL,
    [id_estado]               INT            NOT NULL,
    [id_vendedor]             INT            NULL,
    [id_comprador]            INT            NULL,
    [id_termino_pago_ventas]  INT            NULL,
    [id_metodo_pago_ventas]   INT            NULL,
    [id_termino_pago_compras] INT            NULL,
    [id_metodo_pago_compras]  INT            NULL,
    [fecha_creacion]          DATETIME2 (7)  CONSTRAINT [DF_CONTACTO_FECHA] DEFAULT (sysdatetime()) NOT NULL,
    [fecha_modificacion]      DATETIME2 (7)  NULL,
    CONSTRAINT [PK_CONTACTO] PRIMARY KEY CLUSTERED ([id_contacto] ASC),
    CONSTRAINT [CK_CONTACTO_CEDULA] CHECK ([cedula] IS NULL OR len(ltrim(rtrim([cedula])))>(0)),
    CONSTRAINT [CK_CONTACTO_DIRECCION_CR] CHECK ([pais]<>N'Costa Rica' OR [cedula] IS NOT NULL AND [provincia] IS NOT NULL AND [canton] IS NOT NULL AND [distrito] IS NOT NULL AND len(ltrim(rtrim([provincia])))>(0) AND len(ltrim(rtrim([canton])))>(0) AND len(ltrim(rtrim([distrito])))>(0)),
    CONSTRAINT [CK_CONTACTO_FISCAL] CHECK ([identificacion_fiscal] IS NULL OR len(ltrim(rtrim([identificacion_fiscal])))>(0)),
    CONSTRAINT [CK_CONTACTO_FISCAL_EXTRANJERO] CHECK ([pais]=N'Costa Rica' OR [identificacion_fiscal] IS NOT NULL),
    CONSTRAINT [CK_CONTACTO_IDENTIFICACION] CHECK ([cedula] IS NOT NULL AND len(ltrim(rtrim([cedula])))>(0) OR [identificacion_fiscal] IS NOT NULL AND len(ltrim(rtrim([identificacion_fiscal])))>(0)),
    CONSTRAINT [CK_CONTACTO_PAGOS_ROL] CHECK (([rol_contacto]<>'Cliente' OR [id_termino_pago_compras] IS NULL AND [id_metodo_pago_compras] IS NULL AND [id_comprador] IS NULL) AND ([rol_contacto]<>'Proveedor' OR [id_termino_pago_ventas] IS NULL AND [id_metodo_pago_ventas] IS NULL AND [id_vendedor] IS NULL)),
    CONSTRAINT [CK_CONTACTO_REQUERIDOS] CHECK (len(ltrim(rtrim([nombre])))>(0) AND len(ltrim(rtrim([correo])))>(0) AND len(ltrim(rtrim([telefono])))>(0) AND len(ltrim(rtrim([pais])))>(0) AND len(ltrim(rtrim([detalle_direccion])))>(0)),
    CONSTRAINT [CK_CONTACTO_ROL] CHECK ([rol_contacto]='Ambos' OR [rol_contacto]='Proveedor' OR [rol_contacto]='Cliente'),
    CONSTRAINT [FK_CONTACTO_COMPRADOR] FOREIGN KEY ([id_comprador]) REFERENCES [dbo].[USUARIO] ([id_usuario]),
    CONSTRAINT [FK_CONTACTO_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_CONTACTO_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_CONTACTO_METODO_COMPRAS] FOREIGN KEY ([id_metodo_pago_compras], [id_empresa]) REFERENCES [dbo].[METODO_PAGO] ([id_metodo_pago], [id_empresa]),
    CONSTRAINT [FK_CONTACTO_METODO_VENTAS] FOREIGN KEY ([id_metodo_pago_ventas], [id_empresa]) REFERENCES [dbo].[METODO_PAGO] ([id_metodo_pago], [id_empresa]),
    CONSTRAINT [FK_CONTACTO_TERMINO_COMPRAS] FOREIGN KEY ([id_termino_pago_compras], [id_empresa]) REFERENCES [dbo].[TERMINO_PAGO] ([id_termino_pago], [id_empresa]),
    CONSTRAINT [FK_CONTACTO_TERMINO_VENTAS] FOREIGN KEY ([id_termino_pago_ventas], [id_empresa]) REFERENCES [dbo].[TERMINO_PAGO] ([id_termino_pago], [id_empresa]),
    CONSTRAINT [FK_CONTACTO_VENDEDOR] FOREIGN KEY ([id_vendedor]) REFERENCES [dbo].[USUARIO] ([id_usuario]),
    CONSTRAINT [UQ_CONTACTO_EMPRESA] UNIQUE NONCLUSTERED ([id_contacto] ASC, [id_empresa] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_CONTACTO_FILTROS]
    ON [dbo].[CONTACTO]([id_empresa] ASC, [rol_contacto] ASC, [id_estado] ASC)
    INCLUDE([nombre], [cedula], [identificacion_fiscal], [correo], [telefono]);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_CONTACTO_FISCAL]
    ON [dbo].[CONTACTO]([id_empresa] ASC, [identificacion_fiscal] ASC) WHERE ([identificacion_fiscal] IS NOT NULL);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_CONTACTO_CEDULA]
    ON [dbo].[CONTACTO]([id_empresa] ASC, [cedula] ASC) WHERE ([cedula] IS NOT NULL);

