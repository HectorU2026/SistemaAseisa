CREATE TABLE [dbo].[MOVIMIENTO_BANCARIO] (
    [id_movimiento_bancario] INT             IDENTITY (1, 1) NOT NULL,
    [id_cuenta_bancaria]     INT             NOT NULL,
    [fecha_movimiento]       DATE            NOT NULL,
    [descripcion]            VARCHAR (500)   NULL,
    [referencia]             VARCHAR (150)   NULL,
    [tipo_movimiento]        VARCHAR (50)    NOT NULL,
    [monto]                  DECIMAL (18, 2) NOT NULL,
    [origen]                 VARCHAR (100)   NULL,
    [id_estado]              INT             NOT NULL,
    CONSTRAINT [PK_MOVIMIENTO_BANCARIO] PRIMARY KEY CLUSTERED ([id_movimiento_bancario] ASC),
    CONSTRAINT [FK_MOVIMIENTO_BANCARIO_CUENTA] FOREIGN KEY ([id_cuenta_bancaria]) REFERENCES [dbo].[CUENTA_BANCARIA] ([id_cuenta_bancaria]),
    CONSTRAINT [FK_MOVIMIENTO_BANCARIO_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

