CREATE TABLE [dbo].[USUARIO] (
    [id_usuario]         INT           IDENTITY (1, 1) NOT NULL,
    [id_empresa]         INT           NOT NULL,
    [tipo_cliente]       VARCHAR (50)  NULL,
    [nombre]             VARCHAR (100) NOT NULL,
    [primer_apellido]    VARCHAR (100) NOT NULL,
    [segundo_apellido]   VARCHAR (100) NULL,
    [correo]             VARCHAR (150) NOT NULL,
    [nombre_usuario]     VARCHAR (100) NOT NULL,
    [contrasena_hash]    VARCHAR (500) NOT NULL,
    [ultimo_acceso]      DATETIME2 (7) NULL,
    [id_estado]          INT           NOT NULL,
    [fecha_creacion]     DATETIME2 (7) DEFAULT (sysdatetime()) NOT NULL,
    [fecha_modificacion] DATETIME2 (7) NULL,
    CONSTRAINT [PK_USUARIO] PRIMARY KEY CLUSTERED ([id_usuario] ASC),
    CONSTRAINT [FK_USUARIO_EMPRESA] FOREIGN KEY ([id_empresa]) REFERENCES [dbo].[EMPRESA] ([id_empresa]),
    CONSTRAINT [FK_USUARIO_ESTADO] FOREIGN KEY ([id_estado]) REFERENCES [dbo].[ESTADO] ([id_estado]),
    CONSTRAINT [UQ_USUARIO_CORREO] UNIQUE NONCLUSTERED ([correo] ASC),
    CONSTRAINT [UQ_USUARIO_NOMBRE_USUARIO] UNIQUE NONCLUSTERED ([nombre_usuario] ASC)
);

