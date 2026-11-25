CREATE PROCEDURE dbo.GestionarRolUsuario
    @id_empresa INT,@id_usuario INT,@id_rol INT,@id_usuario_accion INT,@agregar BIT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.ProtegerAdministradorEmpresa @id_empresa;
        IF @agregar IS NULL OR dbo.EsAdministradorActivo(@id_usuario_accion,@id_empresa)=0
            THROW 51025,'Operacion no autorizada.',1;
        IF @id_usuario=@id_usuario_accion THROW 51027,'No puede modificar sus propios roles.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.USUARIO WITH(UPDLOCK,HOLDLOCK)
            WHERE id_usuario=@id_usuario AND id_empresa=@id_empresa AND estado_aprobacion='Aprobado')
            THROW 51015,'Usuario no disponible.',1;
        IF EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU INNER JOIN dbo.ROL R ON R.id_rol=RU.id_rol
            WHERE RU.id_usuario=@id_usuario AND R.nombre='Superadministrador') THROW 51025,'Operacion no autorizada.',1;
        IF NOT EXISTS(SELECT 1 FROM dbo.ROL R INNER JOIN dbo.ESTADO E ON E.id_estado=R.id_estado
            WHERE R.id_rol=@id_rol AND R.nombre<>'Superadministrador' AND LTRIM(RTRIM(E.nombre_estado))='Activo')
            THROW 51022,'Rol no disponible.',1;
        DECLARE @activo INT,@inactivo INT,@cambios INT=0;
        SELECT @activo=id_estado FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado))='Activo';
        SELECT @inactivo=id_estado FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado))='Inactivo';
        IF @activo IS NULL OR @inactivo IS NULL THROW 51020,'Catalogo de estados incompleto.',1;
        IF @agregar=1
        BEGIN
            IF NOT EXISTS(SELECT 1 FROM dbo.ROL_USUARIO WHERE id_usuario=@id_usuario AND id_rol=@id_rol AND id_estado=@activo)
            BEGIN
                UPDATE dbo.ROL_USUARIO SET id_estado=@activo,fecha_asignacion=SYSDATETIME() WHERE id_usuario=@id_usuario AND id_rol=@id_rol;
                IF @@ROWCOUNT=0 INSERT dbo.ROL_USUARIO(id_usuario,id_rol,id_estado) VALUES(@id_usuario,@id_rol,@activo);
                SET @cambios=1;
            END;
        END
        ELSE
        BEGIN
            -- El bloqueo de USUARIO serializa las operaciones sobre sus roles.
            IF EXISTS(SELECT 1 FROM dbo.ROL_USUARIO WHERE id_usuario=@id_usuario AND id_rol=@id_rol AND id_estado=@activo)
               AND NOT EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU WITH(UPDLOCK,HOLDLOCK)
                   INNER JOIN dbo.ROL R WITH(HOLDLOCK) ON R.id_rol=RU.id_rol
                   INNER JOIN dbo.ESTADO E ON E.id_estado=R.id_estado
                   WHERE RU.id_usuario=@id_usuario AND RU.id_rol<>@id_rol AND RU.id_estado=@activo
                     AND LTRIM(RTRIM(E.nombre_estado))='Activo')
                THROW 51211,'No es posible retirar este rol. El usuario debe conservar al menos un rol activo.',1;
            UPDATE dbo.ROL_USUARIO SET id_estado=@inactivo WHERE id_usuario=@id_usuario AND id_rol=@id_rol AND id_estado=@activo;
            SET @cambios=@@ROWCOUNT;
        END;
        IF @cambios>0 INSERT dbo.BITACORA(id_usuario,id_empresa,tabla_afectada,registro_id,accion,valor_nuevo)
            VALUES(@id_usuario_accion,@id_empresa,'ROL_USUARIO',CONVERT(VARCHAR(100),@id_usuario),
                CASE WHEN @agregar=1 THEN 'AGREGAR_ROL' ELSE 'RETIRAR_ROL' END,
                (SELECT @id_rol IdRol,@agregar Activo FOR JSON PATH,WITHOUT_ARRAY_WRAPPER));
        COMMIT; SELECT @id_usuario;
    END TRY
    BEGIN CATCH IF XACT_STATE()<>0 ROLLBACK; THROW; END CATCH;
END;