CREATE TABLE [dbo].[BITACORA] (
    [id_bitacora]    INT           IDENTITY (1, 1) NOT NULL,
    [id_usuario]     INT           NOT NULL,
    [id_empresa]     INT           NOT NULL,
    [tabla_afectada] VARCHAR (150) NOT NULL,
    [registro_id]    VARCHAR (100) NULL,
    [accion]         VARCHAR (100) NOT NULL,
    [valor_anterior] VARCHAR (MAX) NULL,
    [valor_nuevo]    VARCHAR (MAX) NULL,
    [fecha_accion]   DATETIME2 (7) DEFAULT (sysdatetime()) NOT NULL,
    CONSTRAINT [PK_BITACORA] PRIMARY KEY CLUSTERED ([id_bitacora] ASC),
    CONSTRAINT [FK_BITACORA_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_BITACORA_USUARIO] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[USUARIO] ([id_usuario])
);

