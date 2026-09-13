CREATE TABLE [dbo].[COTIZACION] (
    [id_cotizacion]     INT             IDENTITY (1, 1) NOT NULL,
    [id_empresa]        INT             NOT NULL,
    [id_cliente]        INT             NOT NULL,
    [numero_cotizacion] VARCHAR (50)    NOT NULL,
    [fecha_emision]     DATETIME2 (7)   NOT NULL,
    [fecha_vencimiento] DATETIME2 (7)   NULL,
    [moneda]            VARCHAR (10)    NULL,
    [tipo_cambio]       DECIMAL (18, 6) NULL,
    [subtotal]          DECIMAL (18, 2) NOT NULL,
    [impuesto]          DECIMAL (18, 2) NOT NULL,
    [descuento]         DECIMAL (18, 2) NOT NULL,
    [total]             DECIMAL (18, 2) NOT NULL,
    [id_estado]         INT             NOT NULL,
    CONSTRAINT [PK_COTIZACION] PRIMARY KEY CLUSTERED ([id_cotizacion] ASC),
    CONSTRAINT [FK_COTIZACION_CLIENTE] FOREIGN KEY ([id_cliente]) REFERENCES [dbo].[CLIENTE] ([id_cliente]),
    CONSTRAINT [FK_COTIZACION_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_COTIZACION_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado])
);

