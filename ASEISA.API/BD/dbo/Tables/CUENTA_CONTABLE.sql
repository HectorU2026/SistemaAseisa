CREATE TABLE [dbo].[CUENTA_CONTABLE] (
    [id_cuenta_contable]  INT            IDENTITY (1, 1) NOT NULL,
    [id_empresa]          INT            NOT NULL,
    [codigo]              VARCHAR (50)   NOT NULL,
    [nombre]              VARCHAR (150)  NOT NULL,
    [tipo_cuenta]         VARCHAR (50)   NOT NULL,
    [naturaleza]          VARCHAR (20)   NOT NULL,
    [permite_movimientos] BIT            DEFAULT ((1)) NOT NULL,
    [nivel]               INT            NOT NULL,
    [id_estado]           INT            NOT NULL,
    [id_cuenta_principal] INT            NULL,
    [conciliacion_pagos]  BIT            CONSTRAINT [DF_CUENTA_CONTABLE_CONCILIACION] DEFAULT ((0)) NOT NULL,
    [impuestos]           DECIMAL (9, 4) NULL,
    [descripcion]         VARCHAR (500)  NULL,
    CONSTRAINT [PK_CUENTA_CONTABLE] PRIMARY KEY CLUSTERED ([id_cuenta_contable] ASC),
    CONSTRAINT [CK_CUENTA_CONTABLE_PRINCIPAL_DISTINTA] CHECK ([id_cuenta_principal]<>[id_cuenta_contable]),
    CONSTRAINT [FK_CUENTA_CONTABLE_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_CUENTA_CONTABLE_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_CUENTA_CONTABLE_PRINCIPAL] FOREIGN KEY ([id_cuenta_principal], [id_empresa]) REFERENCES [dbo].[CUENTA_CONTABLE] ([id_cuenta_contable], [id_empresa]),
    CONSTRAINT [UQ_CUENTA_CONTABLE_ID_EMPRESA] UNIQUE NONCLUSTERED ([id_cuenta_contable] ASC, [id_empresa] ASC)
);

