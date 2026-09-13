CREATE TABLE [dbo].[DETALLE_CONCILIACION] (
    [id_detalle_conciliacion] INT            IDENTITY (1, 1) NOT NULL,
    [id_conciliacion]         INT            NOT NULL,
    [id_movimiento_bancario]  INT            NOT NULL,
    [id_asiento]              INT            NULL,
    [nivel_coincidencia]      DECIMAL (8, 4) NULL,
    [conciliado_auto]         BIT            DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_DETALLE_CONCILIACION] PRIMARY KEY CLUSTERED ([id_detalle_conciliacion] ASC),
    CONSTRAINT [FK_DETALLE_CONCILIACION_ASIENTO] FOREIGN KEY ([id_asiento]) REFERENCES [dbo].[ASIENTO_CONTABLE] ([id_asiento]),
    CONSTRAINT [FK_DETALLE_CONCILIACION_CONCILIACION] FOREIGN KEY ([id_conciliacion]) REFERENCES [dbo].[CONCILIACION_BANCARIA] ([id_conciliacion]),
    CONSTRAINT [FK_DETALLE_CONCILIACION_MOVIMIENTO] FOREIGN KEY ([id_movimiento_bancario]) REFERENCES [dbo].[MOVIMIENTO_BANCARIO] ([id_movimiento_bancario])
);

