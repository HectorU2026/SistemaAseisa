CREATE TABLE [dbo].[METODO_PAGO] (
    [id_metodo_pago]     INT            IDENTITY (1, 1) NOT NULL,
    [id_empresa]         INT            NOT NULL,
    [nombre]             NVARCHAR (100) NOT NULL,
    [activo]             BIT            CONSTRAINT [DF_METODO_PAGO_ACTIVO] DEFAULT ((1)) NOT NULL,
    [fecha_creacion]     DATETIME2 (7)  CONSTRAINT [DF_METODO_PAGO_FECHA] DEFAULT (sysdatetime()) NOT NULL,
    [fecha_modificacion] DATETIME2 (7)  NULL,
    CONSTRAINT [PK_METODO_PAGO] PRIMARY KEY CLUSTERED ([id_metodo_pago] ASC),
    CONSTRAINT [CK_METODO_PAGO_NOMBRE] CHECK (len(ltrim(rtrim([nombre])))>(0)),
    CONSTRAINT [FK_METODO_PAGO_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [UQ_METODO_PAGO_EMPRESA] UNIQUE NONCLUSTERED ([id_metodo_pago] ASC, [id_empresa] ASC),
    CONSTRAINT [UQ_METODO_PAGO_NOMBRE] UNIQUE NONCLUSTERED ([id_empresa] ASC, [nombre] ASC)
);

