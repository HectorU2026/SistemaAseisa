CREATE TABLE [dbo].[TIPO_CUENTA] (
    [id_tipo_cuenta] INT          IDENTITY (1, 1) NOT NULL,
    [nombre]         VARCHAR (50) NOT NULL,
    [id_estado]      INT          DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_TIPO_CUENTA] PRIMARY KEY CLUSTERED ([id_tipo_cuenta] ASC),
    CONSTRAINT [FK_TIPO_CUENTA_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

