CREATE TABLE [dbo].[ASIENTO_CONTABLE] (
    [id_asiento]     INT             IDENTITY (1, 1) NOT NULL,
    [id_empresa]     INT             NOT NULL,
    [numero_asiento] VARCHAR (50)    NOT NULL,
    [fecha_asiento]  DATE            NOT NULL,
    [descripcion]    VARCHAR (500)   NULL,
    [tipo_origen]    VARCHAR (50)    NULL,
    [total_debe]     DECIMAL (18, 2) NOT NULL,
    [total_haber]    DECIMAL (18, 2) NOT NULL,
    [id_estado]      INT             NOT NULL,
    [id_usuario]     INT             NOT NULL,
    [fecha_creacion] DATETIME2 (7)   DEFAULT (sysdatetime()) NOT NULL,
    CONSTRAINT [PK_ASIENTO_CONTABLE] PRIMARY KEY CLUSTERED ([id_asiento] ASC),
    CONSTRAINT [FK_ASIENTO_CONTABLE_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_ASIENTO_CONTABLE_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_ASIENTO_CONTABLE_USUARIO] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[USUARIO] ([id_usuario])
);

