CREATE TABLE [dbo].[ESTADO] (
    [id_estado]     INT           IDENTITY (1, 1) NOT NULL,
    [nombre_estado] VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_ESTADO] PRIMARY KEY CLUSTERED ([id_estado] ASC)
);

