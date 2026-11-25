CREATE PROCEDURE dbo.ConsultarPlataforma @actor INT,@empresa INT=NULL AS
BEGIN
    SET NOCOUNT ON;
    IF dbo.EsSuperadministradorActivo(@actor)=0 THROW 51025,'Operacion de plataforma no autorizada.',1;
    IF @empresa IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.EMPRESA WHERE id_empresa=@empresa) THROW 51015,'Empresa no disponible.',1;
    SELECT E.id_empresa IdEmpresa,E.razon_social RazonSocial,E.nombre_comercial NombreComercial,E.cedula_juridica CedulaJuridica,
        E.correo Correo,E.telefono Telefono,E.direccion Direccion,E.moneda_base MonedaBase,S.nombre_estado Estado,
        (SELECT COUNT(*) FROM dbo.USUARIO U WHERE U.id_empresa=E.id_empresa AND dbo.EsAdministradorActivo(U.id_usuario,E.id_empresa)=1) AdministradoresHabilitados
    FROM dbo.EMPRESA E JOIN dbo.ESTADO S ON S.id_estado=E.id_estado WHERE @empresa IS NULL OR E.id_empresa=@empresa ORDER BY E.razon_social;
    SELECT U.id_usuario IdUsuario,U.id_empresa IdEmpresa,U.nombre Nombre,U.primer_apellido PrimerApellido,U.segundo_apellido SegundoApellido,
        U.correo Correo,U.nombre_usuario NombreUsuario,U.estado_aprobacion EstadoAprobacion,S.nombre_estado Estado
    FROM dbo.USUARIO U JOIN dbo.ESTADO S ON S.id_estado=U.id_estado
    WHERE (@empresa IS NULL OR U.id_empresa=@empresa) AND U.es_plataforma=0
        AND EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU JOIN dbo.ROL R ON R.id_rol=RU.id_rol JOIN dbo.ESTADO EA ON EA.id_estado=RU.id_estado JOIN dbo.ESTADO ER ON ER.id_estado=R.id_estado
        WHERE RU.id_usuario=U.id_usuario AND R.nombre='Administrador' AND LTRIM(RTRIM(EA.nombre_estado))='Activo' AND LTRIM(RTRIM(ER.nombre_estado))='Activo') ORDER BY U.nombre;
    SELECT TOP(200) id_bitacora IdBitacora,id_usuario IdUsuario,id_empresa_objetivo IdEmpresaObjetivo,tabla_afectada Tabla,registro_id Registro,accion Accion,fecha_accion Fecha
    FROM dbo.BITACORA WHERE id_empresa IS NULL AND (@empresa IS NULL OR id_empresa_objetivo=@empresa) ORDER BY id_bitacora DESC;
END;