CREATE   PROCEDURE [dbo].[RegistroUsuario]
    @id_empresa           INT,
    @tipo_cliente         VARCHAR(50) = NULL,
    @nombre               VARCHAR(100),
    @primer_apellido      VARCHAR(100),
    @segundo_apellido     VARCHAR(100) = NULL,
    @correo               VARCHAR(150),
    @nombre_usuario       VARCHAR(100),
    @contrasena_hash      VARCHAR(500),
    @id_estado            INT,
    @telefono             VARCHAR(30) = NULL,
    @direccion            VARCHAR(300) = NULL,
    @identificacion       VARCHAR(50) = NULL,
    @nombre_razon_social  VARCHAR(200) = NULL,
    @id_rol               INT,
    @registro_publico     BIT = 0,
    @id_usuario_accion    INT = NULL,
    @roles_json           NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @contrasena_hash IS NULL OR LTRIM(RTRIM(@contrasena_hash)) = ''
        THROW 51021, 'Debe proporcionar un hash de contrasena no vacio.', 1;
    IF @registro_publico IS NULL
        THROW 51039, 'Debe indicar el tipo de registro.', 1;

    IF NULLIF(LTRIM(RTRIM(@nombre)),'') IS NULL OR LEN(@nombre)>80
       OR NULLIF(LTRIM(RTRIM(@primer_apellido)),'') IS NULL OR LEN(@primer_apellido)>80
       OR LEN(@segundo_apellido)>80 OR @correo NOT LIKE '%_@_%._%'
       OR NULLIF(LTRIM(RTRIM(@nombre_usuario)),'') IS NULL
        THROW 51212,'Revise los datos personales del registro.',1;
    SET @identificacion=REPLACE(LTRIM(RTRIM(@identificacion)),'-','');
    IF @tipo_cliente IS NULL OR @tipo_cliente NOT IN('Persona física','Persona jurídica')
       OR @identificacion IS NULL OR @identificacion LIKE '%[^0-9]%'
       OR (@tipo_cliente='Persona física' AND (LEN(@identificacion)<>9 OR LEFT(@identificacion,1) NOT LIKE '[1-9]'))
       OR (@tipo_cliente='Persona jurídica' AND (LEN(@identificacion)<>10 OR LEFT(@identificacion,1)<>'3' OR NULLIF(LTRIM(RTRIM(@nombre_razon_social)),'') IS NULL))
        THROW 51213,'Revise el tipo, identificacion y razon social.',1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE
            @nombre_rol VARCHAR(100),
            @id_rol_cliente INT,
            @id_estado_activo INT,
            @id_estado_inactivo INT,
            @estado_aprobacion VARCHAR(20),
            @estado_usuario INT,
            @nuevo_id INT;

        -- 1. Obtener estados.
        SELECT @id_estado_activo = id_estado
        FROM dbo.ESTADO
        WHERE LTRIM(RTRIM(nombre_estado)) = 'Activo';

        SELECT @id_estado_inactivo = id_estado
        FROM dbo.ESTADO
        WHERE LTRIM(RTRIM(nombre_estado)) = 'Inactivo';

        IF @id_estado_activo IS NULL
           OR @id_estado_inactivo IS NULL
            THROW 51020,
                'No se encontraron los estados requeridos.', 1;

        -- 2. Verificar empresa activa.
        IF NOT EXISTS (
            SELECT 1
            FROM dbo.EMPRESA
            WHERE id_empresa = @id_empresa
              AND id_estado = @id_estado_activo
        )
            THROW 51030,
                'La empresa no existe o esta inactiva.', 1;

        -- 3. El registro publico resuelve Cliente desde el catalogo.
        IF @registro_publico = 1
        BEGIN
            IF (SELECT COUNT(*) FROM dbo.ROL
                WHERE nombre = 'Cliente' AND id_estado = @id_estado_activo) <> 1
                THROW 51034, 'Debe existir un unico rol Cliente activo.', 1;

            SELECT @id_rol_cliente = id_rol
            FROM dbo.ROL
            WHERE nombre = 'Cliente' AND id_estado = @id_estado_activo;

            IF @id_rol IS NOT NULL AND @id_rol <> @id_rol_cliente
                THROW 51034, 'El registro publico solo permite clientes.', 1;

            SET @id_rol = @id_rol_cliente;
        END;

        DECLARE @roles TABLE(id_rol INT PRIMARY KEY);
        IF @registro_publico=1 OR @roles_json IS NULL
            INSERT @roles VALUES(@id_rol);
        ELSE
        BEGIN
            IF ISJSON(@roles_json)<>1 OR LEFT(LTRIM(@roles_json),1)<>'['
                THROW 51214,'Seleccione al menos un rol valido, sin duplicados.',1;
            IF NOT EXISTS(SELECT 1 FROM OPENJSON(@roles_json))
               OR EXISTS(SELECT 1 FROM OPENJSON(@roles_json) WHERE type<>2 OR TRY_CONVERT(INT,value) IS NULL OR TRY_CONVERT(INT,value)<=0)
               OR EXISTS(SELECT TRY_CONVERT(INT,value) FROM OPENJSON(@roles_json) GROUP BY TRY_CONVERT(INT,value) HAVING COUNT(*)>1)
                THROW 51214,'Seleccione al menos un rol valido, sin duplicados.',1;
            INSERT @roles SELECT CONVERT(INT,value) FROM OPENJSON(@roles_json);
        END;
        IF EXISTS(SELECT 1 FROM @roles S LEFT JOIN dbo.ROL R ON R.id_rol=S.id_rol
            WHERE R.id_rol IS NULL OR R.id_estado<>@id_estado_activo OR R.nombre='Superadministrador')
            THROW 51031,'El rol no existe o no esta permitido.',1;
        -- Cliente es representativo si se selecciono, para conservar su registro asociado.
        SELECT TOP(1) @id_rol=R.id_rol FROM @roles S INNER JOIN dbo.ROL R ON R.id_rol=S.id_rol
            ORDER BY CASE WHEN R.nombre='Cliente' THEN 0 ELSE 1 END,R.id_rol;

        SELECT @nombre_rol = nombre
        FROM dbo.ROL
        WHERE id_rol = @id_rol
          AND id_estado = @id_estado_activo;

        IF @nombre_rol IS NULL
            THROW 51031, 'El rol no existe o esta inactivo.', 1;

        IF @nombre_rol='Cliente' AND NULLIF(LTRIM(RTRIM(@identificacion)),'') IS NULL
            THROW 51040,'Indique la identificacion del cliente.',1;
        IF @nombre_rol='Cliente' AND @nombre_razon_social IS NULL AND LEN(CONCAT(@nombre,' ',@primer_apellido))>200
            THROW 51040,'Indique un nombre de cliente de hasta 200 caracteres.',1;
        -- 4. Verificar datos duplicados.
        IF EXISTS (
            SELECT 1
            FROM dbo.USUARIO
            WHERE correo = @correo
        )
            THROW 51032,
                'El correo ya esta registrado.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.USUARIO
            WHERE nombre_usuario = @nombre_usuario
        )
            THROW 51033,
                'El nombre de usuario ya esta registrado.', 1;

        -- 5. Determinar tipo de registro.
        IF @registro_publico = 1
        BEGIN
            IF @nombre_rol <> 'Cliente'
                THROW 51034,
                    'El registro publico solo permite clientes.', 1;

            SET @estado_aprobacion = 'Pendiente';
            SET @estado_usuario = @id_estado_inactivo;
        END
        ELSE
        BEGIN
            -- El registro administrativo requiere un actor.
            IF @id_usuario_accion IS NULL
                THROW 51035,
                    'Debe indicar el administrador responsable.', 1;

            IF dbo.EsSuperadministradorActivo(@id_usuario_accion)=1 AND EXISTS(SELECT 1 FROM @roles S JOIN dbo.ROL R ON R.id_rol=S.id_rol WHERE R.nombre<>'Administrador')
                THROW 51025,'La plataforma solo puede registrar administradores de empresas.',1;
            -- Verificar administrador activo de la empresa.
            IF dbo.EsSuperadministradorActivo(@id_usuario_accion)=0 AND NOT EXISTS (
                SELECT 1
                FROM dbo.USUARIO U
                INNER JOIN dbo.ROL_USUARIO RU
                    ON RU.id_usuario = U.id_usuario
                INNER JOIN dbo.ROL R
                    ON R.id_rol = RU.id_rol
                WHERE U.id_usuario = @id_usuario_accion
                  AND U.id_empresa = @id_empresa
                  AND U.id_estado = @id_estado_activo
                  AND U.estado_aprobacion = 'Aprobado'
                  AND RU.id_estado = @id_estado_activo
                  AND R.id_estado = @id_estado_activo
                  AND R.nombre = 'Administrador'
            )
                THROW 51036,
                    'No tiene permisos para registrar usuarios en esta empresa.', 1;

            -- Impedir asignar el rol global desde este proceso.
            IF @nombre_rol = 'Superadministrador'
                THROW 51037,
                    'No se permite registrar superadministradores.', 1;

            IF @id_estado IS NULL OR @id_estado NOT IN (
                @id_estado_activo,
                @id_estado_inactivo
            )
                THROW 51038,
                    'El estado indicado no es valido.', 1;

            SET @estado_aprobacion = 'Aprobado';
            SET @estado_usuario = @id_estado;
        END;

        -- 6. Registrar usuario.
        INSERT INTO dbo.USUARIO
        (
            id_empresa,
            tipo_cliente,
            nombre,
            primer_apellido,
            segundo_apellido,
            correo,
            nombre_usuario,
            contrasena_hash,
            id_estado,
            estado_aprobacion
        )
        VALUES
        (
            @id_empresa,
            @tipo_cliente,
            @nombre,
            @primer_apellido,
            @segundo_apellido,
            @correo,
            @nombre_usuario,
            @contrasena_hash,
            @estado_usuario,
            @estado_aprobacion
        );

        SET @nuevo_id = CAST(SCOPE_IDENTITY() AS INT);

        -- Todas las asignaciones iniciales se guardan en la misma transaccion.
        INSERT dbo.ROL_USUARIO(id_usuario,id_rol,id_estado)
            SELECT @nuevo_id,id_rol,@id_estado_activo FROM @roles;
        -- 8. Crear CLIENTE únicamente cuando corresponda.
        IF @nombre_rol = 'Cliente'
        BEGIN
            INSERT INTO dbo.CLIENTE
            (
                id_empresa,
                id_usuario,
                tipo_cliente,
                nombre_razon_social,
                identificacion,
                correo,
                telefono,
                direccion,
                id_estado
            )
            VALUES
            (
                @id_empresa,
                @nuevo_id,
                ISNULL(@tipo_cliente, 'Persona física'),
                ISNULL(
                    @nombre_razon_social,
                    CONCAT(@nombre, ' ', @primer_apellido)
                ),
                ISNULL(@identificacion, @correo),
                @correo,
                @telefono,
                @direccion,
                @estado_usuario
            );
        END;

        IF @registro_publico=0
            INSERT dbo.BITACORA(id_usuario,id_empresa,id_empresa_objetivo,tabla_afectada,registro_id,accion)
                VALUES(@id_usuario_accion,CASE WHEN dbo.EsSuperadministradorActivo(@id_usuario_accion)=1 THEN NULL ELSE @id_empresa END,CASE WHEN dbo.EsSuperadministradorActivo(@id_usuario_accion)=1 THEN @id_empresa ELSE NULL END,'USUARIO',CONVERT(VARCHAR(100),@nuevo_id),'REGISTRAR_USUARIO');
        COMMIT TRANSACTION;

        SELECT @nuevo_id AS id_usuario;

    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO
