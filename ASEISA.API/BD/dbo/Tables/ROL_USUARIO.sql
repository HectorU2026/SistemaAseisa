CREATE TABLE [dbo].[ROL_USUARIO] (
    [id_usuario]       INT           NOT NULL,
    [id_rol]           INT           NOT NULL,
    [id_estado]        INT           NOT NULL,
    [fecha_asignacion] DATETIME2 (7) DEFAULT (sysdatetime()) NOT NULL,
    CONSTRAINT [PK_ROL_USUARIO] PRIMARY KEY CLUSTERED ([id_usuario] ASC, [id_rol] ASC),
    CONSTRAINT [FK_ROL_USUARIO_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_ROL_USUARIO_ROL] FOREIGN KEY ([id_rol]) REFERENCES [dbo].[ROL] ([id_rol]),
    CONSTRAINT [FK_ROL_USUARIO_USUARIO] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[USUARIO] ([id_usuario])
);

