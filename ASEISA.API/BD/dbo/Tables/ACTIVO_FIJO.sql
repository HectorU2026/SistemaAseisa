CREATE TABLE [dbo].[ACTIVO_FIJO] (
    [id_activo_fijo]         INT             IDENTITY (1, 1) NOT NULL,
    [id_empresa]             INT             NOT NULL,
    [codigo]                 VARCHAR (50)    NOT NULL,
    [nombre]                 VARCHAR (150)   NOT NULL,
    [descripcion]            VARCHAR (500)   NULL,
    [fecha_adquisicion]      DATE            NOT NULL,
    [costo_adquisicion]      DECIMAL (18, 2) NOT NULL,
    [valor_residual]         DECIMAL (18, 2) NOT NULL,
    [vida_util_meses]        INT             NOT NULL,
    [metodo_depreciacion]    VARCHAR (100)   NOT NULL,
    [id_cuenta_activo]       INT             NOT NULL,
    [id_cuenta_depreciacion] INT             NOT NULL,
    [id_estado]              INT             NOT NULL,
    CONSTRAINT [PK_ACTIVO_FIJO] PRIMARY KEY CLUSTERED ([id_activo_fijo] ASC),
    CONSTRAINT [FK_ACTIVO_FIJO_CUENTA_ACTIVO] FOREIGN KEY ([id_cuenta_activo]) REFERENCES [dbo].[CUENTA_CONTABLE] ([id_cuenta_contable]),
    CONSTRAINT [FK_ACTIVO_FIJO_CUENTA_DEPRECIACION] FOREIGN KEY ([id_cuenta_depreciacion]) REFERENCES [dbo].[CUENTA_CONTABLE] ([id_cuenta_contable]),
    CONSTRAINT [FK_ACTIVO_FIJO_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_ACTIVO_FIJO_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

