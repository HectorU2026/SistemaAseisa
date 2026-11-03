CREATE TABLE [dbo].[ESTADO_COTIZACION] (
    [id_estado_cotizacion] INT           IDENTITY (1, 1) NOT NULL,
    [nombre_estado]        VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_ESTADO_COTIZACION] PRIMARY KEY CLUSTERED ([id_estado_cotizacion] ASC)
);

