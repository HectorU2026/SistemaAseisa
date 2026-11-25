CREATE TABLE [dbo].[FACTURA_XML_ACADEMICO] (
    [id_factura]         INT             NOT NULL,
    [id_empresa]         INT             NOT NULL,
    [xml_firmado]        VARBINARY (MAX) NOT NULL,
    [huella_contenido]   VARCHAR (128)   NOT NULL,
    [algoritmo_firma]    VARCHAR (100)   NOT NULL,
    [fecha_generacion]   DATETIME2 (7)   CONSTRAINT [DF_FACTURA_XML_ACADEMICO_FECHA] DEFAULT (sysdatetime()) NOT NULL,
    [aviso]              VARCHAR (500)   CONSTRAINT [DF_FACTURA_XML_ACADEMICO_AVISO] DEFAULT ('Documento académico de simulación 4.4. No es un comprobante electrónico válido ante el Ministerio de Hacienda ni ante la DGT.') NOT NULL,
    CONSTRAINT [PK_FACTURA_XML_ACADEMICO] PRIMARY KEY CLUSTERED ([id_factura] ASC),
    CONSTRAINT [FK_FACTURA_XML_ACADEMICO_FACTURA] FOREIGN KEY ([id_factura]) REFERENCES [dbo].[FACTURA] ([id_factura]),
    CONSTRAINT [FK_FACTURA_XML_ACADEMICO_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [CK_FACTURA_XML_ACADEMICO_XML] CHECK (DATALENGTH([xml_firmado]) > 0),
    CONSTRAINT [CK_FACTURA_XML_ACADEMICO_HUELLA] CHECK (LEN(LTRIM(RTRIM([huella_contenido]))) > 0),
    CONSTRAINT [CK_FACTURA_XML_ACADEMICO_ALGORITMO] CHECK (LEN(LTRIM(RTRIM([algoritmo_firma]))) > 0),
    CONSTRAINT [CK_FACTURA_XML_ACADEMICO_AVISO] CHECK (LEN(LTRIM(RTRIM([aviso]))) > 0)
);
GO

CREATE NONCLUSTERED INDEX [IX_FACTURA_XML_ACADEMICO_EMPRESA]
    ON [dbo].[FACTURA_XML_ACADEMICO] ([id_empresa] ASC);
