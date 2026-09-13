CREATE DATABASE ASEISA_BD;
USE ASEISA_BD;

CREATE TABLE ESTADO (
    id_estado INT IDENTITY(1,1) NOT NULL,
    nombre_estado VARCHAR(100) NOT NULL,
    CONSTRAINT PK_ESTADO PRIMARY KEY (id_estado)
);
GO

CREATE TABLE EMPRESA (
    id_empresa INT IDENTITY(1,1) NOT NULL,
    razon_social VARCHAR(200) NOT NULL,
    nombre_comercial VARCHAR(200) NULL,
    cedula_juridica VARCHAR(30) NOT NULL,
    correo VARCHAR(150) NULL,
    telefono VARCHAR(30) NULL,
    direccion VARCHAR(300) NULL,
    moneda_base VARCHAR(10) NOT NULL,
    id_estado INT NOT NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    fecha_modificacion DATETIME2 NULL,
    CONSTRAINT PK_EMPRESA PRIMARY KEY (id_empresa),
    CONSTRAINT FK_EMPRESA_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE USUARIO (
    id_usuario INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    tipo_cliente VARCHAR(50) NULL,
    nombre VARCHAR(100) NOT NULL,
    primer_apellido VARCHAR(100) NOT NULL,
    segundo_apellido VARCHAR(100) NULL,
    correo VARCHAR(150) NOT NULL,
    nombre_usuario VARCHAR(100) NOT NULL,
    contrasena_hash VARCHAR(500) NOT NULL,
    ultimo_acceso DATETIME2 NULL,
    id_estado INT NOT NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    fecha_modificacion DATETIME2 NULL,
    CONSTRAINT PK_USUARIO PRIMARY KEY (id_usuario),
    CONSTRAINT UQ_USUARIO_CORREO UNIQUE (correo),
    CONSTRAINT UQ_USUARIO_NOMBRE_USUARIO UNIQUE (nombre_usuario),
    CONSTRAINT FK_USUARIO_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_USUARIO_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE ROL (
    id_rol INT IDENTITY(1,1) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    id_estado INT NOT NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_ROL PRIMARY KEY (id_rol),
    CONSTRAINT FK_ROL_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE PERMISO (
    id_permiso INT IDENTITY(1,1) NOT NULL,
    modulo VARCHAR(100) NOT NULL,
    accion VARCHAR(100) NOT NULL,
    pantalla VARCHAR(150) NULL,
    descripcion VARCHAR(300) NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_PERMISO PRIMARY KEY (id_permiso),
    CONSTRAINT FK_PERMISO_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE ROL_USUARIO (
    id_usuario INT NOT NULL,
    id_rol INT NOT NULL,
    id_estado INT NOT NULL,
    fecha_asignacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_ROL_USUARIO PRIMARY KEY (id_usuario, id_rol),
    CONSTRAINT FK_ROL_USUARIO_USUARIO FOREIGN KEY (id_usuario) REFERENCES USUARIO(id_usuario),
    CONSTRAINT FK_ROL_USUARIO_ROL FOREIGN KEY (id_rol) REFERENCES ROL(id_rol),
    CONSTRAINT FK_ROL_USUARIO_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE ROL_PERMISO (
    id_rol INT NOT NULL,
    id_permiso INT NOT NULL,
    id_estado INT NOT NULL,
    fecha_asignacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_ROL_PERMISO PRIMARY KEY (id_rol, id_permiso),
    CONSTRAINT FK_ROL_PERMISO_ROL FOREIGN KEY (id_rol) REFERENCES ROL(id_rol),
    CONSTRAINT FK_ROL_PERMISO_PERMISO FOREIGN KEY (id_permiso) REFERENCES PERMISO(id_permiso),
    CONSTRAINT FK_ROL_PERMISO_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE BITACORA (
    id_bitacora INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    id_empresa INT NOT NULL,
    tabla_afectada VARCHAR(150) NOT NULL,
    registro_id VARCHAR(100) NULL,
    accion VARCHAR(100) NOT NULL,
    valor_anterior VARCHAR(MAX) NULL,
    valor_nuevo VARCHAR(MAX) NULL,
    fecha_accion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_BITACORA PRIMARY KEY (id_bitacora),
    CONSTRAINT FK_BITACORA_USUARIO FOREIGN KEY (id_usuario) REFERENCES USUARIO(id_usuario),
    CONSTRAINT FK_BITACORA_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa)
);
GO

CREATE TABLE CLIENTE (
    id_cliente INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    tipo_cliente VARCHAR(50) NOT NULL,
    nombre_razon_social VARCHAR(200) NOT NULL,
    identificacion VARCHAR(50) NOT NULL,
    correo VARCHAR(150) NULL,
    telefono VARCHAR(30) NULL,
    direccion VARCHAR(300) NULL,
    limite_credito DECIMAL(18,2) NULL,
    dias_credito INT NULL,
    id_estado INT NOT NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    fecha_modificacion DATETIME2 NULL,
    CONSTRAINT PK_CLIENTE PRIMARY KEY (id_cliente),
    CONSTRAINT FK_CLIENTE_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_CLIENTE_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE PRODUCTO (
    id_producto INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    codigo VARCHAR(50) NOT NULL,
    nombre VARCHAR(150) NOT NULL,
    descripcion VARCHAR(300) NULL,
    tipo_producto VARCHAR(50) NOT NULL,
    unidad_medida VARCHAR(50) NULL,
    costo_promedio DECIMAL(18,4) NULL,
    precio_venta DECIMAL(18,2) NOT NULL,
    inventariable BIT NOT NULL DEFAULT 1,
    id_estado INT NOT NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    fecha_modificacion DATETIME2 NULL,
    CONSTRAINT PK_PRODUCTO PRIMARY KEY (id_producto),
    CONSTRAINT FK_PRODUCTO_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_PRODUCTO_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE COTIZACION (
    id_cotizacion INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    id_cliente INT NOT NULL,
    numero_cotizacion VARCHAR(50) NOT NULL,
    fecha_emision DATETIME2 NOT NULL,
    fecha_vencimiento DATETIME2 NULL,
    moneda VARCHAR(10) NULL,
    tipo_cambio DECIMAL(18,6) NULL,
    subtotal DECIMAL(18,2) NOT NULL,
    impuesto DECIMAL(18,2) NOT NULL,
    descuento DECIMAL(18,2) NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_COTIZACION PRIMARY KEY (id_cotizacion),
    CONSTRAINT FK_COTIZACION_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_COTIZACION_CLIENTE FOREIGN KEY (id_cliente) REFERENCES CLIENTE(id_cliente),
    CONSTRAINT FK_COTIZACION_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE DETALLE_COTIZACION (
    id_detalle_cotizacion INT IDENTITY(1,1) NOT NULL,
    id_cotizacion INT NOT NULL,
    id_producto INT NOT NULL,
    cantidad DECIMAL(18,4) NOT NULL,
    precio_unitario DECIMAL(18,2) NOT NULL,
    porcentaje_descuento DECIMAL(8,4) NULL,
    porcentaje_impuesto DECIMAL(8,4) NULL,
    subtotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_DETALLE_COTIZACION PRIMARY KEY (id_detalle_cotizacion),
    CONSTRAINT FK_DETALLE_COTIZACION_COTIZACION FOREIGN KEY (id_cotizacion) REFERENCES COTIZACION(id_cotizacion),
    CONSTRAINT FK_DETALLE_COTIZACION_PRODUCTO FOREIGN KEY (id_producto) REFERENCES PRODUCTO(id_producto)
);
GO

CREATE TABLE VENTAS (
    id_venta INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    id_cliente INT NOT NULL,
    id_cotizacion INT NULL,
    id_usuario INT NOT NULL,
    numero_comprobante VARCHAR(50) NULL,
    tipo_comprobante VARCHAR(50) NULL,
    fecha_emision DATETIME2 NOT NULL,
    condicion_venta VARCHAR(50) NULL,
    moneda VARCHAR(10) NULL,
    tipo_cambio DECIMAL(18,6) NULL,
    subtotal DECIMAL(18,2) NOT NULL,
    impuesto DECIMAL(18,2) NOT NULL,
    descuento DECIMAL(18,2) NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_VENTAS PRIMARY KEY (id_venta),
    CONSTRAINT FK_VENTAS_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_VENTAS_CLIENTE FOREIGN KEY (id_cliente) REFERENCES CLIENTE(id_cliente),
    CONSTRAINT FK_VENTAS_COTIZACION FOREIGN KEY (id_cotizacion) REFERENCES COTIZACION(id_cotizacion),
    CONSTRAINT FK_VENTAS_USUARIO FOREIGN KEY (id_usuario) REFERENCES USUARIO(id_usuario),
    CONSTRAINT FK_VENTAS_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE DETALLE_VENTA (
    id_detalle_venta INT IDENTITY(1,1) NOT NULL,
    id_venta INT NOT NULL,
    id_producto INT NOT NULL,
    cantidad DECIMAL(18,4) NOT NULL,
    precio_unitario DECIMAL(18,2) NOT NULL,
    porcentaje_descuento DECIMAL(8,4) NULL,
    porcentaje_impuesto DECIMAL(8,4) NULL,
    subtotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_DETALLE_VENTA PRIMARY KEY (id_detalle_venta),
    CONSTRAINT FK_DETALLE_VENTA_VENTAS FOREIGN KEY (id_venta) REFERENCES VENTAS(id_venta),
    CONSTRAINT FK_DETALLE_VENTA_PRODUCTO FOREIGN KEY (id_producto) REFERENCES PRODUCTO(id_producto)
);
GO

CREATE TABLE FACTURA (
    id_factura INT IDENTITY(1,1) NOT NULL,
    id_venta INT NOT NULL,
    id_empresa INT NOT NULL,
    id_usuario INT NOT NULL,
    id_estado INT NOT NULL,
    numero_factura VARCHAR(50) NOT NULL,
    fecha_emision DATETIME2 NOT NULL,
    tipo_factura VARCHAR(50) NULL,
    moneda VARCHAR(10) NULL,
    tipo_cambio DECIMAL(18,6) NULL,
    subtotal DECIMAL(18,2) NOT NULL,
    impuesto DECIMAL(18,2) NOT NULL,
    descuento DECIMAL(18,2) NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    saldo_pendiente DECIMAL(18,2) NOT NULL,
    motivo_anulacion VARCHAR(500) NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    fecha_modificacion DATETIME2 NULL,
    CONSTRAINT PK_FACTURA PRIMARY KEY (id_factura),
    CONSTRAINT FK_FACTURA_VENTAS FOREIGN KEY (id_venta) REFERENCES VENTAS(id_venta),
    CONSTRAINT FK_FACTURA_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_FACTURA_USUARIO FOREIGN KEY (id_usuario) REFERENCES USUARIO(id_usuario),
    CONSTRAINT FK_FACTURA_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE DETALLE_FACTURA (
    id_factura INT NOT NULL,
    id_producto INT NOT NULL,
    cantidad DECIMAL(18,4) NOT NULL,
    precio_unitario DECIMAL(18,2) NOT NULL,
    porcentaje_descuento DECIMAL(8,4) NULL,
    porcentaje_impuesto DECIMAL(8,4) NULL,
    subtotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_DETALLE_FACTURA PRIMARY KEY (id_factura, id_producto),
    CONSTRAINT FK_DETALLE_FACTURA_FACTURA FOREIGN KEY (id_factura) REFERENCES FACTURA(id_factura),
    CONSTRAINT FK_DETALLE_FACTURA_PRODUCTO FOREIGN KEY (id_producto) REFERENCES PRODUCTO(id_producto)
);
GO

CREATE TABLE NOTA_CREDITO (
    id_nota_credito INT IDENTITY(1,1) NOT NULL,
    id_factura INT NOT NULL,
    id_usuario INT NOT NULL,
    numero_nota_credito VARCHAR(50) NOT NULL,
    fecha_emision DATETIME2 NOT NULL,
    motivo VARCHAR(500) NOT NULL,
    monto DECIMAL(18,2) NOT NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    id_estado INT NOT NULL,
    CONSTRAINT PK_NOTA_CREDITO PRIMARY KEY (id_nota_credito),
    CONSTRAINT FK_NOTA_CREDITO_FACTURA FOREIGN KEY (id_factura) REFERENCES FACTURA(id_factura),
    CONSTRAINT FK_NOTA_CREDITO_USUARIO FOREIGN KEY (id_usuario) REFERENCES USUARIO(id_usuario),
    CONSTRAINT FK_NOTA_CREDITO_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE BODEGA (
    id_bodega INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    codigo VARCHAR(50) NOT NULL,
    nombre VARCHAR(150) NOT NULL,
    ubicacion VARCHAR(300) NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_BODEGA PRIMARY KEY (id_bodega),
    CONSTRAINT FK_BODEGA_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_BODEGA_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE EXISTENCIA (
    id_existencia INT IDENTITY(1,1) NOT NULL,
    id_producto INT NOT NULL,
    id_bodega INT NOT NULL,
    cantidad_disponible DECIMAL(18,4) NOT NULL DEFAULT 0,
    cantidad_reservada DECIMAL(18,4) NOT NULL DEFAULT 0,
    costo_promedio DECIMAL(18,4) NULL,
    fecha_actualizacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_EXISTENCIA PRIMARY KEY (id_existencia),
    CONSTRAINT UQ_EXISTENCIA_PRODUCTO_BODEGA UNIQUE (id_producto, id_bodega),
    CONSTRAINT FK_EXISTENCIA_PRODUCTO FOREIGN KEY (id_producto) REFERENCES PRODUCTO(id_producto),
    CONSTRAINT FK_EXISTENCIA_BODEGA FOREIGN KEY (id_bodega) REFERENCES BODEGA(id_bodega)
);
GO

CREATE TABLE MOVIMIENTO_INVENTARIO (
    id_movimiento_inventario INT IDENTITY(1,1) NOT NULL,
    id_producto INT NOT NULL,
    id_bodega INT NOT NULL,
    tipo_movimiento VARCHAR(50) NOT NULL,
    cantidad DECIMAL(18,4) NOT NULL,
    costo_unitario DECIMAL(18,4) NULL,
    fecha_movimiento DATETIME2 NOT NULL,
    documento_origen VARCHAR(100) NULL,
    observacion VARCHAR(500) NULL,
    id_usuario INT NOT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_MOVIMIENTO_INVENTARIO PRIMARY KEY (id_movimiento_inventario),
    CONSTRAINT FK_MOVIMIENTO_INVENTARIO_PRODUCTO FOREIGN KEY (id_producto) REFERENCES PRODUCTO(id_producto),
    CONSTRAINT FK_MOVIMIENTO_INVENTARIO_BODEGA FOREIGN KEY (id_bodega) REFERENCES BODEGA(id_bodega),
    CONSTRAINT FK_MOVIMIENTO_INVENTARIO_USUARIO FOREIGN KEY (id_usuario) REFERENCES USUARIO(id_usuario),
    CONSTRAINT FK_MOVIMIENTO_INVENTARIO_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE PROVEEDOR (
    id_proveedor INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    nombre_razon_social VARCHAR(200) NOT NULL,
    identificacion VARCHAR(50) NOT NULL,
    correo VARCHAR(150) NULL,
    telefono VARCHAR(30) NULL,
    direccion VARCHAR(300) NULL,
    dias_credito INT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_PROVEEDOR PRIMARY KEY (id_proveedor),
    CONSTRAINT FK_PROVEEDOR_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_PROVEEDOR_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE ORDEN_COMPRA (
    id_orden_compra INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    id_proveedor INT NOT NULL,
    id_usuario_solicitante INT NOT NULL,
    numero_orden VARCHAR(50) NOT NULL,
    fecha_emision DATETIME2 NOT NULL,
    fecha_requerida DATETIME2 NULL,
    subtotal DECIMAL(18,2) NOT NULL,
    impuesto DECIMAL(18,2) NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_ORDEN_COMPRA PRIMARY KEY (id_orden_compra),
    CONSTRAINT FK_ORDEN_COMPRA_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_ORDEN_COMPRA_PROVEEDOR FOREIGN KEY (id_proveedor) REFERENCES PROVEEDOR(id_proveedor),
    CONSTRAINT FK_ORDEN_COMPRA_USUARIO FOREIGN KEY (id_usuario_solicitante) REFERENCES USUARIO(id_usuario),
    CONSTRAINT FK_ORDEN_COMPRA_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE DETALLE_ORDEN_COMPRA (
    id_orden_compra INT NOT NULL,
    id_producto INT NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(18,2) NOT NULL,
    subtotal DECIMAL(18,2) NULL,
    CONSTRAINT PK_DETALLE_ORDEN_COMPRA PRIMARY KEY (id_orden_compra, id_producto),
    CONSTRAINT FK_DETALLE_ORDEN_COMPRA_ORDEN FOREIGN KEY (id_orden_compra) REFERENCES ORDEN_COMPRA(id_orden_compra),
    CONSTRAINT FK_DETALLE_ORDEN_COMPRA_PRODUCTO FOREIGN KEY (id_producto) REFERENCES PRODUCTO(id_producto)
);
GO

CREATE TABLE CUENTA_POR_PAGAR (
    id_cuenta_pagar INT IDENTITY(1,1) NOT NULL,
    id_orden_compra INT NOT NULL,
    id_proveedor INT NOT NULL,
    fecha_emision DATETIME2 NOT NULL,
    fecha_vencimiento DATETIME2 NOT NULL,
    monto_original DECIMAL(18,2) NOT NULL,
    saldo_pendiente DECIMAL(18,2) NOT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_CUENTA_POR_PAGAR PRIMARY KEY (id_cuenta_pagar),
    CONSTRAINT FK_CUENTA_POR_PAGAR_ORDEN FOREIGN KEY (id_orden_compra) REFERENCES ORDEN_COMPRA(id_orden_compra),
    CONSTRAINT FK_CUENTA_POR_PAGAR_PROVEEDOR FOREIGN KEY (id_proveedor) REFERENCES PROVEEDOR(id_proveedor),
    CONSTRAINT FK_CUENTA_POR_PAGAR_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE CUENTA_CONTABLE (
    id_cuenta_contable INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    codigo VARCHAR(50) NOT NULL,
    nombre VARCHAR(150) NOT NULL,
    tipo_cuenta VARCHAR(50) NOT NULL,
    naturaleza VARCHAR(20) NOT NULL,
    permite_movimientos BIT NOT NULL DEFAULT 1,
    nivel INT NOT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_CUENTA_CONTABLE PRIMARY KEY (id_cuenta_contable),
    CONSTRAINT FK_CUENTA_CONTABLE_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_CUENTA_CONTABLE_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE CUENTA_BANCARIA (
    id_cuenta_bancaria INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    numero_cuenta VARCHAR(100) NOT NULL,
    tipo_cuenta VARCHAR(50) NOT NULL,
    moneda VARCHAR(10) NOT NULL,
    saldo_actual DECIMAL(18,2) NOT NULL DEFAULT 0,
    id_cuenta_contable INT NOT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_CUENTA_BANCARIA PRIMARY KEY (id_cuenta_bancaria),
    CONSTRAINT FK_CUENTA_BANCARIA_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_CUENTA_BANCARIA_CUENTA_CONTABLE FOREIGN KEY (id_cuenta_contable) REFERENCES CUENTA_CONTABLE(id_cuenta_contable),
    CONSTRAINT FK_CUENTA_BANCARIA_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE PERIODO_CONTABLE (
    id_periodo INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    anio INT NOT NULL,
    mes INT NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    fecha_cierre DATETIME2 NULL,
    id_usuario_cierre INT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_PERIODO_CONTABLE PRIMARY KEY (id_periodo),
    CONSTRAINT FK_PERIODO_CONTABLE_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_PERIODO_CONTABLE_USUARIO FOREIGN KEY (id_usuario_cierre) REFERENCES USUARIO(id_usuario),
    CONSTRAINT FK_PERIODO_CONTABLE_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE ASIENTO_CONTABLE (
    id_asiento INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    numero_asiento VARCHAR(50) NOT NULL,
    fecha_asiento DATE NOT NULL,
    descripcion VARCHAR(500) NULL,
    tipo_origen VARCHAR(50) NULL,
    total_debe DECIMAL(18,2) NOT NULL,
    total_haber DECIMAL(18,2) NOT NULL,
    id_estado INT NOT NULL,
    id_usuario INT NOT NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_ASIENTO_CONTABLE PRIMARY KEY (id_asiento),
    CONSTRAINT FK_ASIENTO_CONTABLE_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_ASIENTO_CONTABLE_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado),
    CONSTRAINT FK_ASIENTO_CONTABLE_USUARIO FOREIGN KEY (id_usuario) REFERENCES USUARIO(id_usuario)
);
GO

CREATE TABLE DISTRIBUCION_ANALITICA (
    id_distribucion_analitica INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    id_proveedor INT NOT NULL,
    id_producto INT NOT NULL,
    descripcion VARCHAR(500) NULL,
    fecha DATETIME2 NOT NULL,
    importe DECIMAL(18,2) NOT NULL,
    id_estado INT NOT NULL,
    referencia VARCHAR(150) NULL,
    unidad VARCHAR(100) NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    fecha_modificacion DATETIME2 NULL,
    CONSTRAINT PK_DISTRIBUCION_ANALITICA PRIMARY KEY (id_distribucion_analitica),
    CONSTRAINT FK_DISTRIBUCION_ANALITICA_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_DISTRIBUCION_ANALITICA_PROVEEDOR FOREIGN KEY (id_proveedor) REFERENCES PROVEEDOR(id_proveedor),
    CONSTRAINT FK_DISTRIBUCION_ANALITICA_PRODUCTO FOREIGN KEY (id_producto) REFERENCES PRODUCTO(id_producto),
    CONSTRAINT FK_DISTRIBUCION_ANALITICA_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE DETALLE_ASIENTO (
    id_asiento INT NOT NULL,
    id_cuenta_contable INT NOT NULL,
    id_distribucion_analitica INT NULL,
    descripcion VARCHAR(500) NULL,
    debe DECIMAL(18,2) NOT NULL DEFAULT 0,
    haber DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT PK_DETALLE_ASIENTO PRIMARY KEY (id_asiento, id_cuenta_contable),
    CONSTRAINT FK_DETALLE_ASIENTO_ASIENTO FOREIGN KEY (id_asiento) REFERENCES ASIENTO_CONTABLE(id_asiento),
    CONSTRAINT FK_DETALLE_ASIENTO_CUENTA FOREIGN KEY (id_cuenta_contable) REFERENCES CUENTA_CONTABLE(id_cuenta_contable),
    CONSTRAINT FK_DETALLE_ASIENTO_DISTRIBUCION FOREIGN KEY (id_distribucion_analitica) REFERENCES DISTRIBUCION_ANALITICA(id_distribucion_analitica)
);
GO

CREATE TABLE CUENTA_POR_COBRAR (
    id_cuenta_cobrar INT IDENTITY(1,1) NOT NULL,
    id_venta INT NOT NULL,
    id_cliente INT NOT NULL,
    fecha_emision DATETIME2 NOT NULL,
    fecha_vencimiento DATETIME2 NOT NULL,
    monto_original DECIMAL(18,2) NOT NULL,
    saldo_pendiente DECIMAL(18,2) NOT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_CUENTA_POR_COBRAR PRIMARY KEY (id_cuenta_cobrar),
    CONSTRAINT FK_CUENTA_POR_COBRAR_VENTA FOREIGN KEY (id_venta) REFERENCES VENTAS(id_venta),
    CONSTRAINT FK_CUENTA_POR_COBRAR_CLIENTE FOREIGN KEY (id_cliente) REFERENCES CLIENTE(id_cliente),
    CONSTRAINT FK_CUENTA_POR_COBRAR_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE PAGO_CLIENTE (
    id_pago_cliente INT IDENTITY(1,1) NOT NULL,
    id_cuenta_cobrar INT NOT NULL,
    id_cuenta_bancaria INT NULL,
    fecha_pago DATETIME2 NOT NULL,
    monto DECIMAL(18,2) NOT NULL,
    metodo_pago VARCHAR(50) NOT NULL,
    referencia VARCHAR(150) NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_PAGO_CLIENTE PRIMARY KEY (id_pago_cliente),
    CONSTRAINT FK_PAGO_CLIENTE_CUENTA_COBRAR FOREIGN KEY (id_cuenta_cobrar) REFERENCES CUENTA_POR_COBRAR(id_cuenta_cobrar),
    CONSTRAINT FK_PAGO_CLIENTE_CUENTA_BANCARIA FOREIGN KEY (id_cuenta_bancaria) REFERENCES CUENTA_BANCARIA(id_cuenta_bancaria),
    CONSTRAINT FK_PAGO_CLIENTE_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE PAGO_PROVEEDOR (
    id_pago_proveedor INT IDENTITY(1,1) NOT NULL,
    id_cuenta_pagar INT NOT NULL,
    id_cuenta_bancaria INT NOT NULL,
    fecha_pago DATETIME2 NOT NULL,
    monto DECIMAL(18,2) NOT NULL,
    metodo_pago VARCHAR(50) NULL,
    referencia VARCHAR(150) NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_PAGO_PROVEEDOR PRIMARY KEY (id_pago_proveedor),
    CONSTRAINT FK_PAGO_PROVEEDOR_CUENTA_PAGAR FOREIGN KEY (id_cuenta_pagar) REFERENCES CUENTA_POR_PAGAR(id_cuenta_pagar),
    CONSTRAINT FK_PAGO_PROVEEDOR_CUENTA_BANCARIA FOREIGN KEY (id_cuenta_bancaria) REFERENCES CUENTA_BANCARIA(id_cuenta_bancaria),
    CONSTRAINT FK_PAGO_PROVEEDOR_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE MOVIMIENTO_BANCARIO (
    id_movimiento_bancario INT IDENTITY(1,1) NOT NULL,
    id_cuenta_bancaria INT NOT NULL,
    fecha_movimiento DATE NOT NULL,
    descripcion VARCHAR(500) NULL,
    referencia VARCHAR(150) NULL,
    tipo_movimiento VARCHAR(50) NOT NULL,
    monto DECIMAL(18,2) NOT NULL,
    origen VARCHAR(100) NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_MOVIMIENTO_BANCARIO PRIMARY KEY (id_movimiento_bancario),
    CONSTRAINT FK_MOVIMIENTO_BANCARIO_CUENTA FOREIGN KEY (id_cuenta_bancaria) REFERENCES CUENTA_BANCARIA(id_cuenta_bancaria),
    CONSTRAINT FK_MOVIMIENTO_BANCARIO_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE CONCILIACION_BANCARIA (
    id_conciliacion INT IDENTITY(1,1) NOT NULL,
    id_cuenta_bancaria INT NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    saldo_banco DECIMAL(18,2) NOT NULL,
    saldo_libros VARCHAR(100) NULL,
    diferencia DECIMAL(18,2) NOT NULL,
    procesada_ia BIT NOT NULL DEFAULT 0,
    id_usuario INT NOT NULL,
    conciliado_auto BIT NOT NULL DEFAULT 0,
    observacion VARCHAR(500) NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_CONCILIACION_BANCARIA PRIMARY KEY (id_conciliacion),
    CONSTRAINT FK_CONCILIACION_BANCARIA_CUENTA FOREIGN KEY (id_cuenta_bancaria) REFERENCES CUENTA_BANCARIA(id_cuenta_bancaria),
    CONSTRAINT FK_CONCILIACION_BANCARIA_USUARIO FOREIGN KEY (id_usuario) REFERENCES USUARIO(id_usuario),
    CONSTRAINT FK_CONCILIACION_BANCARIA_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE DETALLE_CONCILIACION (
    id_detalle_conciliacion INT IDENTITY(1,1) NOT NULL,
    id_conciliacion INT NOT NULL,
    id_movimiento_bancario INT NOT NULL,
    id_asiento INT NULL,
    nivel_coincidencia DECIMAL(8,4) NULL,
    conciliado_auto BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_DETALLE_CONCILIACION PRIMARY KEY (id_detalle_conciliacion),
    CONSTRAINT FK_DETALLE_CONCILIACION_CONCILIACION FOREIGN KEY (id_conciliacion) REFERENCES CONCILIACION_BANCARIA(id_conciliacion),
    CONSTRAINT FK_DETALLE_CONCILIACION_MOVIMIENTO FOREIGN KEY (id_movimiento_bancario) REFERENCES MOVIMIENTO_BANCARIO(id_movimiento_bancario),
    CONSTRAINT FK_DETALLE_CONCILIACION_ASIENTO FOREIGN KEY (id_asiento) REFERENCES ASIENTO_CONTABLE(id_asiento)
);
GO

CREATE TABLE ACTIVO_FIJO (
    id_activo_fijo INT IDENTITY(1,1) NOT NULL,
    id_empresa INT NOT NULL,
    codigo VARCHAR(50) NOT NULL,
    nombre VARCHAR(150) NOT NULL,
    descripcion VARCHAR(500) NULL,
    fecha_adquisicion DATE NOT NULL,
    costo_adquisicion DECIMAL(18,2) NOT NULL,
    valor_residual DECIMAL(18,2) NOT NULL,
    vida_util_meses INT NOT NULL,
    metodo_depreciacion VARCHAR(100) NOT NULL,
    id_cuenta_activo INT NOT NULL,
    id_cuenta_depreciacion INT NOT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_ACTIVO_FIJO PRIMARY KEY (id_activo_fijo),
    CONSTRAINT FK_ACTIVO_FIJO_EMPRESA FOREIGN KEY (id_empresa) REFERENCES EMPRESA(id_empresa),
    CONSTRAINT FK_ACTIVO_FIJO_CUENTA_ACTIVO FOREIGN KEY (id_cuenta_activo) REFERENCES CUENTA_CONTABLE(id_cuenta_contable),
    CONSTRAINT FK_ACTIVO_FIJO_CUENTA_DEPRECIACION FOREIGN KEY (id_cuenta_depreciacion) REFERENCES CUENTA_CONTABLE(id_cuenta_contable),
    CONSTRAINT FK_ACTIVO_FIJO_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO

CREATE TABLE DEPRECIACION (
    id_depreciacion INT IDENTITY(1,1) NOT NULL,
    id_activo_fijo INT NOT NULL,
    id_asiento INT NULL,
    periodo VARCHAR(20) NOT NULL,
    monto_despreciacion DECIMAL(18,2) NOT NULL,
    depreciacion_acumulada DECIMAL(18,2) NOT NULL,
    valor_en_libros DECIMAL(18,2) NOT NULL,
    fecha_calculo DATETIME2 NOT NULL,
    id_estado INT NOT NULL,
    CONSTRAINT PK_DEPRECIACION PRIMARY KEY (id_depreciacion),
    CONSTRAINT FK_DEPRECIACION_ACTIVO FOREIGN KEY (id_activo_fijo) REFERENCES ACTIVO_FIJO(id_activo_fijo),
    CONSTRAINT FK_DEPRECIACION_ASIENTO FOREIGN KEY (id_asiento) REFERENCES ASIENTO_CONTABLE(id_asiento),
    CONSTRAINT FK_DEPRECIACION_ESTADO FOREIGN KEY (id_estado) REFERENCES ESTADO(id_estado)
);
GO