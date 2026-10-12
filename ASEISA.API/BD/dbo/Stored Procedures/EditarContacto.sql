CREATE   PROCEDURE dbo.EditarContacto
    @id_contacto INT,
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
    IF @@TRANCOUNT <> 0 THROW 51000, 'EditarContacto debe ejecutarse sin una transaccion externa.', 1;
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
        DECLARE @estado_actual INT, @rol_anterior VARCHAR(10), @anterior VARCHAR(MAX);
        SELECT @estado_actual = id_estado, @rol_anterior = rol_contacto
        FROM dbo.CONTACTO WITH (UPDLOCK, HOLDLOCK)
        WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa;
        IF @estado_actual IS NULL
            THROW 51015, 'El contacto no existe en la empresa indicada.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO WITH (HOLDLOCK) WHERE id_estado = @estado_actual AND LTRIM(RTRIM(nombre_estado)) = 'Activo')
            THROW 51016, 'El contacto esta inactivo. Debe reactivarlo antes de editar.', 1;
        IF @id_estado <> @estado_actual
            THROW 51017, 'Use la operacion de cambio de estado para activar o desactivar el contacto.', 1;
        SET @anterior = (SELECT * FROM dbo.CONTACTO WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
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
        WHERE id_empresa = @id_empresa AND id_contacto <> @id_contacto AND ((@cedula IS NOT NULL AND cedula = @cedula)
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
            THROW 51014, 'La identificacion existe en un cliente/proveedor anterior. Vincule ese registro antes de editar el contacto.', 1;
        UPDATE dbo.CONTACTO SET
            cedula = @cedula,
            identificacion_fiscal = @identificacion_fiscal,
            nombre = @nombre,
            correo = @correo,
            telefono = @telefono,
            pais = @pais,
            provincia = @provincia,
            canton = @canton,
            distrito = @distrito,
            detalle_direccion = @detalle_direccion,
            rol_contacto = @rol_contacto,
            id_estado = @id_estado,
            id_vendedor = @id_vendedor,
            id_comprador = @id_comprador,
            id_termino_pago_ventas = @id_termino_pago_ventas,
            id_metodo_pago_ventas = @id_metodo_pago_ventas,
            id_termino_pago_compras = @id_termino_pago_compras,
            id_metodo_pago_compras = @id_metodo_pago_compras,
            fecha_modificacion = SYSDATETIME()
        WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa;

        DECLARE @identificacion NVARCHAR(50) = COALESCE(@cedula, @identificacion_fiscal);
        DECLARE @direccion NVARCHAR(300) = LEFT(CONCAT(@pais, N', ', @provincia, N', ', @canton, N', ', @distrito, N', ', @detalle_direccion), 300);
        DECLARE @dias_ventas INT = (SELECT dias_credito FROM dbo.TERMINO_PAGO WHERE id_termino_pago = @id_termino_pago_ventas AND id_empresa = @id_empresa);
        DECLARE @dias_compras INT = (SELECT dias_credito FROM dbo.TERMINO_PAGO WHERE id_termino_pago = @id_termino_pago_compras AND id_empresa = @id_empresa);
        DECLARE @estado_inactivo INT;
        -- Si se retira un rol, conservar el registro comercial y sus documentos.
        IF (@rol_contacto = 'Proveedor' AND EXISTS (SELECT 1 FROM dbo.CLIENTE WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa))
            OR (@rol_contacto = 'Cliente' AND EXISTS (SELECT 1 FROM dbo.PROVEEDOR WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa))
        BEGIN
            SELECT TOP (1) @estado_inactivo = id_estado FROM dbo.ESTADO WITH (HOLDLOCK)
            WHERE LTRIM(RTRIM(nombre_estado)) = 'Inactivo' ORDER BY id_estado;
            IF @estado_inactivo IS NULL THROW 51018, 'Se requiere el estado Inactivo para retirar un rol comercial.', 1;
        END;
        IF @rol_contacto IN ('Cliente', 'Ambos')
            AND NOT EXISTS (SELECT 1 FROM dbo.CLIENTE WITH (UPDLOCK, HOLDLOCK) WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa)
        BEGIN
            INSERT dbo.CLIENTE(id_empresa, id_contacto, tipo_cliente, nombre_razon_social, identificacion, correo, telefono, direccion, dias_credito, id_estado)
            VALUES(@id_empresa, @id_contacto, 'Contacto', @nombre, @identificacion, @correo, @telefono, @direccion, @dias_ventas, @id_estado);
        END;
        UPDATE dbo.CLIENTE SET nombre_razon_social = @nombre, identificacion = @identificacion,
            correo = @correo, telefono = @telefono, direccion = @direccion,
            dias_credito = @dias_ventas,
            id_estado = CASE WHEN @rol_contacto IN ('Cliente', 'Ambos') THEN @id_estado ELSE @estado_inactivo END, fecha_modificacion = SYSDATETIME()
        WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa;
        IF @rol_contacto IN ('Proveedor', 'Ambos')
            AND NOT EXISTS (SELECT 1 FROM dbo.PROVEEDOR WITH (UPDLOCK, HOLDLOCK) WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa)
        BEGIN
            INSERT dbo.PROVEEDOR(id_empresa, id_contacto, nombre_razon_social, identificacion, correo, telefono, direccion, dias_credito, id_estado)
            VALUES(@id_empresa, @id_contacto, @nombre, @identificacion, @correo, @telefono, @direccion, @dias_compras, @id_estado);
        END;
        UPDATE dbo.PROVEEDOR SET nombre_razon_social = @nombre, identificacion = @identificacion,
            correo = @correo, telefono = @telefono, direccion = @direccion,
            dias_credito = @dias_compras,
            id_estado = CASE WHEN @rol_contacto IN ('Proveedor', 'Ambos') THEN @id_estado ELSE @estado_inactivo END
        WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa;
        DECLARE @nuevo VARCHAR(MAX) = (SELECT * FROM dbo.CONTACTO WHERE id_contacto = @id_contacto AND id_empresa = @id_empresa FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        INSERT dbo.BITACORA(id_usuario, id_empresa, tabla_afectada, registro_id, accion, valor_anterior, valor_nuevo)
        VALUES(@id_usuario, @id_empresa, 'CONTACTO', CONVERT(VARCHAR(100), @id_contacto), 'EDITAR', @anterior, @nuevo);
        COMMIT TRANSACTION;
        SELECT @id_contacto;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;