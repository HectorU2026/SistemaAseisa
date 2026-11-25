CREATE   PROCEDURE [dbo].[ObtenerSolicitudesClientesPendientes]
    @id_empresa INT,
    @id_usuario_accion INT
AS
BEGIN
    SET NOCOUNT ON;
    IF dbo.EsAdministradorActivo(@id_usuario_accion,@id_empresa)=0 THROW 51102,'Operacion no autorizada.',1;

    DECLARE @id_activo INT;

    -- 1. Obtener el estado Activo.
    SELECT @id_activo = id_estado
    FROM dbo.ESTADO
    WHERE LTRIM(RTRIM(nombre_estado)) = 'Activo';

    IF @id_activo IS NULL
        THROW 51101,
            'No se encontro el estado Activo.', 1;

    -- 2. Verificar que el usuario que consulta
    -- sea administrador activo de la empresa.
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.USUARIO U
        INNER JOIN dbo.ROL_USUARIO RU
            ON RU.id_usuario = U.id_usuario
        INNER JOIN dbo.ROL R
            ON R.id_rol = RU.id_rol
        WHERE U.id_usuario = @id_usuario_accion
          AND U.id_empresa = @id_empresa
          AND U.id_estado = @id_activo
          AND U.estado_aprobacion = 'Aprobado'
          AND RU.id_estado = @id_activo
          AND R.id_estado = @id_activo
          AND R.nombre = 'Administrador'
    )
    BEGIN
        THROW 51102,
            'No tiene permisos para consultar las solicitudes.', 1;
    END;

    -- 3. Una solicitud por usuario; EXISTS no multiplica las filas por rol.
    SELECT
        U.id_usuario AS IdUsuario,
        U.id_empresa AS IdEmpresa,
        U.nombre AS Nombre,
        U.primer_apellido AS PrimerApellido,
        U.segundo_apellido AS SegundoApellido,
        U.correo AS Correo,
        U.nombre_usuario AS NombreUsuario,
        U.tipo_cliente AS TipoCliente,
        U.id_estado AS IdEstado,
        U.estado_aprobacion AS EstadoAprobacion,
        U.fecha_creacion AS FechaCreacion,
        C.id_cliente AS IdCliente,
        C.nombre_razon_social AS NombreRazonSocial,
        C.identificacion AS Identificacion,
        C.telefono AS Telefono,
        C.direccion AS Direccion
    FROM dbo.USUARIO U
    LEFT JOIN dbo.CLIENTE C
        ON C.id_usuario = U.id_usuario
       AND C.id_empresa = U.id_empresa
    WHERE U.id_empresa = @id_empresa
      AND U.estado_aprobacion = 'Pendiente'
      AND EXISTS (
          SELECT 1
          FROM dbo.ROL_USUARIO RU
          INNER JOIN dbo.ROL R ON R.id_rol = RU.id_rol
          WHERE RU.id_usuario = U.id_usuario
            AND RU.id_estado = @id_activo
            AND R.id_estado = @id_activo
            AND R.nombre = 'Cliente'
      )
    ORDER BY U.fecha_creacion ASC, U.id_usuario ASC;
END;