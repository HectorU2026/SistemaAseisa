CREATE   PROCEDURE dbo.ObtenerContactos
    @id_empresa INT,
    @busqueda NVARCHAR(200) = NULL,
    @rol_contacto VARCHAR(10) = NULL,
    @id_estado INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @busqueda = NULLIF(LTRIM(RTRIM(@busqueda)), N'');
    SET @rol_contacto = NULLIF(LTRIM(RTRIM(@rol_contacto)), '');
    IF @rol_contacto IS NOT NULL AND @rol_contacto NOT IN ('Cliente','Proveedor','Ambos')
        THROW 51002, 'Seleccione un rol de contacto valido.', 1;
    SELECT c.id_contacto AS IdContacto,
        c.id_empresa AS IdEmpresa,
        c.cedula AS Cedula,
        c.identificacion_fiscal AS IdentificacionFiscal,
        c.nombre AS Nombre,
        c.correo AS Correo,
        c.telefono AS Telefono,
        c.pais AS Pais,
        c.provincia AS Provincia,
        c.canton AS Canton,
        c.distrito AS Distrito,
        c.detalle_direccion AS DetalleDireccion,
        c.rol_contacto AS RolContacto,
        c.id_estado AS IdEstado,
        c.id_vendedor AS IdVendedor,
        c.id_comprador AS IdComprador,
        c.id_termino_pago_ventas AS IdTerminoPagoVentas,
        c.id_metodo_pago_ventas AS IdMetodoPagoVentas,
        c.id_termino_pago_compras AS IdTerminoPagoCompras,
        c.id_metodo_pago_compras AS IdMetodoPagoCompras,
        c.fecha_creacion AS FechaRegistro,
        c.fecha_modificacion AS FechaModificacion,
        e.nombre_estado AS Estado,
        cl.id_cliente AS IdCliente, pr.id_proveedor AS IdProveedor,
        tv.nombre AS TerminoPagoVentas, mv.nombre AS MetodoPagoVentas,
        tc.nombre AS TerminoPagoCompras, mc.nombre AS MetodoPagoCompras
    FROM dbo.CONTACTO c
    INNER JOIN dbo.ESTADO e ON e.id_estado = c.id_estado
    LEFT JOIN dbo.CLIENTE cl ON cl.id_contacto = c.id_contacto AND cl.id_empresa = c.id_empresa
    LEFT JOIN dbo.PROVEEDOR pr ON pr.id_contacto = c.id_contacto AND pr.id_empresa = c.id_empresa
    LEFT JOIN dbo.TERMINO_PAGO tv ON tv.id_termino_pago = c.id_termino_pago_ventas AND tv.id_empresa = c.id_empresa
    LEFT JOIN dbo.METODO_PAGO mv ON mv.id_metodo_pago = c.id_metodo_pago_ventas AND mv.id_empresa = c.id_empresa
    LEFT JOIN dbo.TERMINO_PAGO tc ON tc.id_termino_pago = c.id_termino_pago_compras AND tc.id_empresa = c.id_empresa
    LEFT JOIN dbo.METODO_PAGO mc ON mc.id_metodo_pago = c.id_metodo_pago_compras AND mc.id_empresa = c.id_empresa
    WHERE c.id_empresa = @id_empresa
      AND (@busqueda IS NULL OR CHARINDEX(@busqueda, c.nombre) > 0 OR CHARINDEX(@busqueda, c.cedula) > 0 OR CHARINDEX(@busqueda, c.identificacion_fiscal) > 0)
      AND (@rol_contacto IS NULL OR c.rol_contacto = @rol_contacto)
      AND (@id_estado IS NULL OR c.id_estado = @id_estado)
    ORDER BY c.nombre, c.id_contacto;
END;