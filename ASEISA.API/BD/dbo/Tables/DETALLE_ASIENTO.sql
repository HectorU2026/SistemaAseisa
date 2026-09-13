CREATE TABLE [dbo].[DETALLE_ASIENTO] (
    [id_asiento]                INT             NOT NULL,
    [id_cuenta_contable]        INT             NOT NULL,
    [id_distribucion_analitica] INT             NULL,
    [descripcion]               VARCHAR (500)   NULL,
    [debe]                      DECIMAL (18, 2) DEFAULT ((0)) NOT NULL,
    [haber]                     DECIMAL (18, 2) DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_DETALLE_ASIENTO] PRIMARY KEY CLUSTERED ([id_asiento] ASC, [id_cuenta_contable] ASC),
    CONSTRAINT [FK_DETALLE_ASIENTO_ASIENTO] FOREIGN KEY ([id_asiento]) REFERENCES [dbo].[ASIENTO_CONTABLE] ([id_asiento]),
    CONSTRAINT [FK_DETALLE_ASIENTO_CUENTA] FOREIGN KEY ([id_cuenta_contable]) REFERENCES [dbo].[CUENTA_CONTABLE] ([id_cuenta_contable]),
    CONSTRAINT [FK_DETALLE_ASIENTO_DISTRIBUCION] FOREIGN KEY ([id_distribucion_analitica]) REFERENCES [dbo].[DISTRIBUCION_ANALITICA] ([id_distribucion_analitica])
);

