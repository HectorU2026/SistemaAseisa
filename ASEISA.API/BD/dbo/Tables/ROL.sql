CREATE TABLE [dbo].[ROL] (
    [id_rol]         INT           IDENTITY (1, 1) NOT NULL,
    [nombre]         VARCHAR (100) NOT NULL,
    [id_estado]      INT           NOT NULL,
    [fecha_creacion] DATETIME2 (7) DEFAULT (sysdatetime()) NOT NULL,
    CONSTRAINT [PK_ROL] PRIMARY KEY CLUSTERED ([id_rol] ASC),
    CONSTRAINT [FK_ROL_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

