CREATE TABLE [dbo].[TERMINO_PAGO] (
    [id_termino_pago]    INT            IDENTITY (1, 1) NOT NULL,
    [id_empresa]         INT            NOT NULL,
    [nombre]             NVARCHAR (100) NOT NULL,
    [dias_credito]       INT            NOT NULL,
    [activo]             BIT            CONSTRAINT [DF_TERMINO_PAGO_ACTIVO] DEFAULT ((1)) NOT NULL,
    [fecha_creacion]     DATETIME2 (7)  CONSTRAINT [DF_TERMINO_PAGO_FECHA] DEFAULT (sysdatetime()) NOT NULL,
    [fecha_modificacion] DATETIME2 (7)  NULL,
    CONSTRAINT [PK_TERMINO_PAGO] PRIMARY KEY CLUSTERED ([id_termino_pago] ASC),
    CONSTRAINT [CK_TERMINO_PAGO_DIAS] CHECK ([dias_credito]>=(0)),
    CONSTRAINT [CK_TERMINO_PAGO_NOMBRE] CHECK (len(ltrim(rtrim([nombre])))>(0)),
    CONSTRAINT [FK_TERMINO_PAGO_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [UQ_TERMINO_PAGO_EMPRESA] UNIQUE NONCLUSTERED ([id_termino_pago] ASC, [id_empresa] ASC),
    CONSTRAINT [UQ_TERMINO_PAGO_NOMBRE] UNIQUE NONCLUSTERED ([id_empresa] ASC, [nombre] ASC)
);

