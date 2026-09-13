CREATE TABLE [dbo].[ROL_PERMISO] (
    [id_rol]           INT           NOT NULL,
    [id_permiso]       INT           NOT NULL,
    [id_estado]        INT           NOT NULL,
    [fecha_asignacion] DATETIME2 (7) DEFAULT (sysdatetime()) NOT NULL,
    CONSTRAINT [PK_ROL_PERMISO] PRIMARY KEY CLUSTERED ([id_rol] ASC, [id_permiso] ASC),
    CONSTRAINT [FK_ROL_PERMISO_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_ROL_PERMISO_PERMISO] FOREIGN KEY ([id_permiso]) REFERENCES [dbo].[PERMISO] ([id_permiso]),
    CONSTRAINT [FK_ROL_PERMISO_ROL] FOREIGN KEY ([id_rol]) REFERENCES [dbo].[ROL] ([id_rol])
);

