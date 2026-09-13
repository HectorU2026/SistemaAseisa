CREATE TABLE [dbo].[PERIODO_CONTABLE] (
    [id_periodo]        INT           IDENTITY (1, 1) NOT NULL,
    [id_empresa]        INT           NOT NULL,
    [anio]              INT           NOT NULL,
    [mes]               INT           NOT NULL,
    [fecha_inicio]      DATE          NOT NULL,
    [fecha_fin]         DATE          NOT NULL,
    [fecha_cierre]      DATETIME2 (7) NULL,
    [id_usuario_cierre] INT           NULL,
    [id_estado]         INT           NOT NULL,
    CONSTRAINT [PK_PERIODO_CONTABLE] PRIMARY KEY CLUSTERED ([id_periodo] ASC),
    CONSTRAINT [FK_PERIODO_CONTABLE_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_PERIODO_CONTABLE_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_PERIODO_CONTABLE_USUARIO] FOREIGN KEY ([id_usuario_cierre]) REFERENCES [dbo].[USUARIO] ([id_usuario])
);

