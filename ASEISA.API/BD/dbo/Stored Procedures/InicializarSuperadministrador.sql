CREATE PROCEDURE dbo.InicializarSuperadministrador
    @nombre VARCHAR(80),@primer_apellido VARCHAR(80),@segundo_apellido VARCHAR(80)=NULL,
    @correo VARCHAR(150),@nombre_usuario VARCHAR(100),@contrasena_hash VARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT=0 THROW 51300,'Inicializacion requiere transaccion exterior.',1;
    DECLARE @lock INT;
    EXEC @lock=sys.sp_getapplock @Resource=N'ASEISA:InicializarPlataforma',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=8000;
    IF @lock<0 THROW 51301,'Inicializacion temporalmente no disponible.',1;
    IF EXISTS(SELECT 1 FROM dbo.USUARIO WITH(UPDLOCK,HOLDLOCK) WHERE es_plataforma=1)
        OR EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU JOIN dbo.ROL R ON R.id_rol=RU.id_rol WHERE R.nombre='Superadministrador')
        THROW 51306,'Ya existe una cuenta de superadministracion; no se permite reinicializar.',1;
    IF NULLIF(LTRIM(RTRIM(@contrasena_hash)),'') IS NULL OR NULLIF(LTRIM(RTRIM(@nombre_usuario)),'') IS NULL
       OR NULLIF(LTRIM(RTRIM(@nombre)),'') IS NULL OR NULLIF(LTRIM(RTRIM(@primer_apellido)),'') IS NULL OR @correo NOT LIKE '%_@_%._%'
        THROW 51307,'Datos de inicializacion invalidos.',1;
    DECLARE @activo INT=(SELECT MIN(id_estado) FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado))='Activo'),@rol INT;
    IF @activo IS NULL THROW 51020,'Debe instalarse el catalogo de estados.',1;
    IF (SELECT COUNT(*) FROM dbo.ROL WHERE nombre='Superadministrador')>1 THROW 51307,'Catalogo de roles ambiguo.',1;
    SELECT @rol=id_rol FROM dbo.ROL WITH(UPDLOCK,HOLDLOCK) WHERE nombre='Superadministrador';
    IF @rol IS NULL BEGIN INSERT dbo.ROL(nombre,id_estado) VALUES('Superadministrador',@activo); SET @rol=CONVERT(INT,SCOPE_IDENTITY()); END;
    IF NOT EXISTS(SELECT 1 FROM dbo.ROL WHERE id_rol=@rol AND id_estado=@activo) THROW 51307,'Rol de plataforma inactivo.',1;
    INSERT dbo.USUARIO(id_empresa,es_plataforma,tipo_cliente,nombre,primer_apellido,segundo_apellido,correo,nombre_usuario,contrasena_hash,id_estado,estado_aprobacion)
        VALUES(NULL,1,NULL,@nombre,@primer_apellido,@segundo_apellido,@correo,@nombre_usuario,@contrasena_hash,@activo,'Aprobado');
    DECLARE @usuario INT=CONVERT(INT,SCOPE_IDENTITY());
    INSERT dbo.ROL_USUARIO(id_usuario,id_rol,id_estado) VALUES(@usuario,@rol,@activo);
    INSERT dbo.BITACORA(id_usuario,id_empresa,tabla_afectada,registro_id,accion)
        VALUES(@usuario,NULL,'USUARIO',CONVERT(VARCHAR(100),@usuario),'INICIALIZAR_SUPERADMINISTRADOR');
    SELECT @usuario;
END;