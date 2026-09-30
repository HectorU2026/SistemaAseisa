CREATE TABLE [dbo].[PERMISO] (
    [id_permiso]  INT           IDENTITY (1, 1) NOT NULL,
    [modulo]      VARCHAR (100) NOT NULL,
    [accion]      VARCHAR (100) NOT NULL,
    [pantalla]    VARCHAR (150) NULL,
    [descripcion] VARCHAR (300) NULL,
    [id_estado]   INT           NOT NULL,
    CONSTRAINT [PK_PERMISO] PRIMARY KEY CLUSTERED ([id_permiso] ASC),
    CONSTRAINT [FK_PERMISO_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

