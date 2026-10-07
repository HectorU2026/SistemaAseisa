CREATE   PROCEDURE dbo.AgregarContacto
    @id_empresa INT,
    @id_usuario INT,
    @nombre NVARCHAR(200),
    @correo NVARCHAR(150),
    @telefono NVARCHAR(30),
    @pais NVARCHAR(100),
    @detalle_direccion NVARCHAR(300),
    @rol_contacto VARCHAR(10),
    @id_estado INT,
    @cedula NVARCHAR(50) = NULL,
    @identificacion_fiscal NVARCHAR(50) = NULL,
    @provincia NVARCHAR(100) = NULL,
    @canton NVARCHAR(100) = NULL,
    @distrito NVARCHAR(100) = NULL,
    @id_vendedor INT = NULL,
    @id_comprador INT = NULL,
    @id_termino_pago_ventas INT = NULL,
    @id_metodo_pago_ventas INT = NULL,
    @id_termino_pago_compras INT = NULL,
    @id_metodo_pago_compras INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    -- El procedimiento es propietario de la transaccion.
    IF @@TRANCOUNT <> 0 THROW 51000, 'AgregarContacto debe ejecutarse sin una transaccion externa.', 1;
    SET @cedula = NULLIF(LTRIM(RTRIM(@cedula)), N'');
    SET @identificacion_fiscal = NULLIF(LTRIM(RTRIM(@identificacion_fiscal)), N'');
    SET @nombre = NULLIF(LTRIM(RTRIM(@nombre)), N'');
    SET @correo = NULLIF(LTRIM(RTRIM(@correo)), N'');
    SET @telefono = NULLIF(LTRIM(RTRIM(@telefono)), N'');
    SET @pais = NULLIF(LTRIM(RTRIM(@pais)), N'');
    SET @provincia = NULLIF(LTRIM(RTRIM(@provincia)), N'');
    SET @canton = NULLIF(LTRIM(RTRIM(@canton)), N'');
    SET @distrito = NULLIF(LTRIM(RTRIM(@distrito)), N'');
    SET @detalle_direccion = NULLIF(LTRIM(RTRIM(@detalle_direccion)), N'');
    SET @rol_contacto = NULLIF(LTRIM(RTRIM(@rol_contacto)), N'');
    IF @nombre IS NULL OR @correo IS NULL OR @telefono IS NULL OR @pais IS NULL OR @detalle_direccion IS NULL
        THROW 51001, 'Complete nombre, correo, telefono, pais y detalle de direccion.', 1;
    IF @rol_contacto IS NULL OR @rol_contacto NOT IN ('Cliente','Proveedor','Ambos')
        THROW 51002, 'Seleccione un rol de contacto valido.', 1;
    IF @cedula IS NULL AND @identificacion_fiscal IS NULL
        THROW 51003, 'Ingrese cedula o identificacion fiscal.', 1;
    IF @pais = N'Costa Rica' AND (@cedula IS NULL OR @provincia IS NULL OR @canton IS NULL OR @distrito IS NULL)
        THROW 51004, 'Para Costa Rica se requieren cedula, provincia, canton y distrito.', 1;
    IF @pais <> N'Costa Rica' AND @identificacion_fiscal IS NULL
        THROW 51005, 'El contacto extranjero requiere identificacion fiscal.', 1;
    -- Comprobacion basica SQL. El backend y la pantalla validaran el formato completo.
    IF @correo NOT LIKE N'%_@_%._%' OR @correo LIKE N'% %'
        THROW 51006, 'El correo no tiene un formato valido.', 1;
    IF (@rol_contacto = 'Cliente' AND (@id_termino_pago_compras IS NOT NULL OR @id_metodo_pago_compras IS NOT NULL OR @id_comprador IS NOT NULL))
        OR (@rol_contacto = 'Proveedor' AND (@id_termino_pago_ventas IS NOT NULL OR @id_metodo_pago_ventas IS NOT NULL OR @id_vendedor IS NOT NULL))
        THROW 51007, 'Los responsables y pagos no corresponden al rol del contacto.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.USUARIO WITH (HOLDLOCK) WHERE id_usuario = @id_usuario AND id_empresa = @id_empresa)
            THROW 51008, 'El usuario no pertenece a la empresa indicada.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO WITH (HOLDLOCK) WHERE id_estado = @id_estado AND LTRIM(RTRIM(nombre_estado)) IN ('Activo','Inactivo'))
            THROW 51009, 'El contacto requiere un estado Activo o Inactivo del catalogo ESTADO.', 1;
        IF @id_vendedor IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.USUARIO WITH (HOLDLOCK) WHERE id_usuario = @id_vendedor AND id_empresa = @id_empresa)
            THROW 51010, 'El vendedor no pertenece a la empresa.', 1;
        IF @id_comprador IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.USUARIO WITH (HOLDLOCK) WHERE id_usuario = @id_comprador AND id_empresa = @id_empresa)
            THROW 51011, 'El comprador no pertenece a la empresa.', 1;
    IF @id_termino_pago_ventas IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.TERMINO_PAGO WITH (HOLDLOCK) WHERE id_termino_pago = @id_termino_pago_ventas AND id_empresa = @id_empresa AND activo = 1)
        THROW 51012, 'Seleccione un termino o metodo de pago activo de la empresa.', 1;
    IF @id_termino_pago_compras IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.TERMINO_PAGO WITH (HOLDLOCK) WHERE id_termino_pago = @id_termino_pago_compras AND id_empresa = @id_empresa AND activo = 1)
        THROW 51012, 'Seleccione un termino o metodo de pago activo de la empresa.', 1;
    IF @id_metodo_pago_ventas IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.METODO_PAGO WITH (HOLDLOCK) WHERE id_metodo_pago = @id_metodo_pago_ventas AND id_empresa = @id_empresa AND activo = 1)
        THROW 51012, 'Seleccione un termino o metodo de pago activo de la empresa.', 1;
    IF @id_metodo_pago_compras IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.METODO_PAGO WITH (HOLDLOCK) WHERE id_metodo_pago = @id_metodo_pago_compras AND id_empresa = @id_empresa AND activo = 1)
        THROW 51012, 'Seleccione un termino o metodo de pago activo de la empresa.', 1;
        DECLARE @conflicto INT, @mensaje NVARCHAR(2048);
        SELECT TOP (1) @conflicto = id_contacto FROM dbo.CONTACTO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_empresa = @id_empresa AND ((@cedula IS NOT NULL AND cedula = @cedula)
            OR (@identificacion_fiscal IS NOT NULL AND identificacion_fiscal = @identificacion_fiscal))
        ORDER BY id_contacto;
        IF @conflicto IS NOT NULL
        BEGIN
            SELECT @mensaje = CONCAT(N'Identificacion registrada en el contacto ', id_contacto, N': ', nombre)
            FROM dbo.CONTACTO WHERE id_contacto = @conflicto;
            THROW 51013, @mensaje, 1;
        END;
        -- Los registros anteriores a CONTACTO tambien deben comprobarse.
        IF EXISTS (SELECT 1 FROM dbo.CLIENTE WITH (UPDLOCK, HOLDLOCK) WHERE id_empresa = @id_empresa AND id_contacto IS NULL
            AND (identificacion = @cedula OR identificacion = @identificacion_fiscal))
            OR EXISTS (SELECT 1 FROM dbo.PROVEEDOR WITH (UPDLOCK, HOLDLOCK) WHERE id_empresa = @id_empresa AND id_contacto IS NULL
            AND (identificacion = @cedula OR identificacion = @identificacion_fiscal))
            THROW 51014, 'La identificacion existe en un cliente/proveedor anterior. Vincule ese registro antes de crear el contacto.', 1;
        INSERT dbo.CONTACTO (id_empresa, cedula, identificacion_fiscal, nombre, correo, telefono, pais, provincia, canton, distrito, detalle_direccion, rol_contacto, id_estado, id_vendedor, id_comprador, id_termino_pago_ventas, id_metodo_pago_ventas, id_termino_pago_compras, id_metodo_pago_compras)
        VALUES (@id_empresa, @cedula, @identificacion_fiscal, @nombre, @correo, @telefono, @pais, @provincia, @canton, @distrito, @detalle_direccion, @rol_contacto, @id_estado, @id_vendedor, @id_comprador, @id_termino_pago_ventas, @id_metodo_pago_ventas, @id_termino_pago_compras, @id_metodo_pago_compras);
        DECLARE @id_contacto INT = CONVERT(INT, SCOPE_IDENTITY());
        DECLARE @id_cliente INT = NULL, @id_proveedor INT = NULL;
        DECLARE @identificacion NVARCHAR(50) = COALESCE(@cedula, @identificacion_fiscal);
        DECLARE @direccion NVARCHAR(300) = LEFT(CONCAT(@pais, N', ', @provincia, N', ', @canton, N', ', @distrito, N', ', @detalle_direccion), 300);
        DECLARE @dias_ventas INT = (SELECT dias_credito FROM dbo.TERMINO_PAGO WHERE id_termino_pago = @id_termino_pago_ventas AND id_empresa = @id_empresa);
        DECLARE @dias_compras INT = (SELECT dias_credito FROM dbo.TERMINO_PAGO WHERE id_termino_pago = @id_termino_pago_compras AND id_empresa = @id_empresa);
        IF @rol_contacto IN ('Cliente', 'Ambos')
        BEGIN
            INSERT dbo.CLIENTE(id_empresa, id_contacto, tipo_cliente, nombre_razon_social, identificacion, correo, telefono, direccion, dias_credito, id_estado)
            VALUES(@id_empresa, @id_contacto, 'Contacto', @nombre, @identificacion, @correo, @telefono, @direccion, @dias_ventas, @id_estado);
            SET @id_cliente = CONVERT(INT, SCOPE_IDENTITY());
        END;
        IF @rol_contacto IN ('Proveedor', 'Ambos')
        BEGIN
            INSERT dbo.PROVEEDOR(id_empresa, id_contacto, nombre_razon_social, identificacion, correo, telefono, direccion, dias_credito, id_estado)
            VALUES(@id_empresa, @id_contacto, @nombre, @identificacion, @correo, @telefono, @direccion, @dias_compras, @id_estado);
            SET @id_proveedor = CONVERT(INT, SCOPE_IDENTITY());
        END;
        DECLARE @nuevo VARCHAR(MAX) = (SELECT * FROM dbo.CONTACTO WHERE id_contacto = @id_contacto FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        INSERT dbo.BITACORA(id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES(@id_usuario, @id_empresa, 'CONTACTO', CONVERT(VARCHAR(100), @id_contacto), 'REGISTRAR', NULL, @nuevo);
        COMMIT TRANSACTION;
        SELECT @id_contacto AS IdContacto, @id_cliente AS IdCliente, @id_proveedor AS IdProveedor;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;