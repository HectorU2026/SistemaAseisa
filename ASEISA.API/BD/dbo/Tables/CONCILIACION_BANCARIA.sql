CREATE TABLE [dbo].[CONCILIACION_BANCARIA] (
    [id_conciliacion]    INT             IDENTITY (1, 1) NOT NULL,
    [id_cuenta_bancaria] INT             NOT NULL,
    [fecha_inicio]       DATE            NOT NULL,
    [fecha_fin]          DATE            NOT NULL,
    [saldo_banco]        DECIMAL (18, 2) NOT NULL,
    [saldo_libros]       VARCHAR (100)   NULL,
    [diferencia]         DECIMAL (18, 2) NOT NULL,
    [procesada_ia]       BIT             DEFAULT ((0)) NOT NULL,
    [id_usuario]         INT             NOT NULL,
    [conciliado_auto]    BIT             DEFAULT ((0)) NOT NULL,
    [observacion]        VARCHAR (500)   NULL,
    [id_estado]          INT             NOT NULL,
    CONSTRAINT [PK_CONCILIACION_BANCARIA] PRIMARY KEY CLUSTERED ([id_conciliacion] ASC),
    CONSTRAINT [FK_CONCILIACION_BANCARIA_CUENTA] FOREIGN KEY ([id_cuenta_bancaria]) REFERENCES [dbo].[CUENTA_BANCARIA] ([id_cuenta_bancaria]),
    CONSTRAINT [FK_CONCILIACION_BANCARIA_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [FK_CONCILIACION_BANCARIA_USUARIO] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[USUARIO] ([id_usuario])
);

