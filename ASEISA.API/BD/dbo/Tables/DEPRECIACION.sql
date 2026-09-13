CREATE TABLE [dbo].[DEPRECIACION] (
    [id_depreciacion]        INT             IDENTITY (1, 1) NOT NULL,
    [id_activo_fijo]         INT             NOT NULL,
    [id_asiento]             INT             NULL,
    [periodo]                VARCHAR (20)    NOT NULL,
    [monto_despreciacion]    DECIMAL (18, 2) NOT NULL,
    [depreciacion_acumulada] DECIMAL (18, 2) NOT NULL,
    [valor_en_libros]        DECIMAL (18, 2) NOT NULL,
    [fecha_calculo]          DATETIME2 (7)   NOT NULL,
    [id_estado]              INT             NOT NULL,
    CONSTRAINT [PK_DEPRECIACION] PRIMARY KEY CLUSTERED ([id_depreciacion] ASC),
    CONSTRAINT [FK_DEPRECIACION_ACTIVO] FOREIGN KEY ([id_activo_fijo]) REFERENCES [dbo].[ACTIVO_FIJO] ([id_activo_fijo]),
    CONSTRAINT [FK_DEPRECIACION_ASIENTO] FOREIGN KEY ([id_asiento]) REFERENCES [dbo].[ASIENTO_CONTABLE] ([id_asiento]),
    CONSTRAINT [FK_DEPRECIACION_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

