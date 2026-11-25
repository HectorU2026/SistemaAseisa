CREATE PROCEDURE dbo.GestionarAdministradorPlataforma @actor INT,@empresa INT,@usuario INT,@operacion VARCHAR(20),
    @nombre VARCHAR(80)=NULL,@primer_apellido VARCHAR(80)=NULL,@segundo_apellido VARCHAR(80)=NULL,@correo VARCHAR(150)=NULL,@activo BIT=NULL
AS
BEGIN
    SET NOCOUNT ON;SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF dbo.EsSuperadministradorActivo(@actor)=0 THROW 51025,'Operacion de plataforma no autorizada.',1;
        EXEC dbo.ProtegerAdministradorEmpresa @empresa;
        IF @actor=@usuario OR NOT EXISTS(SELECT 1 FROM dbo.USUARIO U WITH(UPDLOCK,HOLDLOCK) WHERE U.id_usuario=@usuario AND U.id_empresa=@empresa AND U.es_plataforma=0
            AND EXISTS(SELECT 1 FROM dbo.ROL_USUARIO RU JOIN dbo.ROL R ON R.id_rol=RU.id_rol JOIN dbo.ESTADO E ON E.id_estado=RU.id_estado JOIN dbo.ESTADO ER ON ER.id_estado=R.id_estado
                WHERE RU.id_usuario=U.id_usuario AND R.nombre='Administrador' AND LTRIM(RTRIM(E.nombre_estado))='Activo' AND LTRIM(RTRIM(ER.nombre_estado))='Activo'))
            THROW 51015,'Administrador no disponible en esta empresa.',1;
        IF @operacion='Editar'
        BEGIN
            IF NULLIF(LTRIM(RTRIM(@nombre)),'') IS NULL OR NULLIF(LTRIM(RTRIM(@primer_apellido)),'') IS NULL OR @correo NOT LIKE '%_@_%._%' THROW 51307,'Datos personales invalidos.',1;
            UPDATE dbo.USUARIO SET nombre=@nombre,primer_apellido=@primer_apellido,segundo_apellido=@segundo_apellido,correo=@correo,fecha_modificacion=SYSDATETIME() WHERE id_usuario=@usuario;
        END
        ELSE IF @operacion='Estado' AND @activo IS NOT NULL
        BEGIN
            IF EXISTS(SELECT 1 FROM dbo.USUARIO WHERE id_usuario=@usuario AND estado_aprobacion<>'Aprobado') THROW 51024,'Cuenta no aprobada.',1;
            DECLARE @estado INT=(SELECT MIN(id_estado) FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado))=CASE WHEN @activo=1 THEN 'Activo' ELSE 'Inactivo' END);
            IF @estado IS NULL THROW 51020,'Catalogo de estados incompleto.',1;
            UPDATE dbo.USUARIO SET id_estado=@estado,fecha_modificacion=SYSDATETIME() WHERE id_usuario=@usuario AND id_estado<>@estado;
        END
        ELSE THROW 51307,'Operacion invalida.',1;
        INSERT dbo.BITACORA(id_usuario,id_empresa,id_empresa_objetivo,tabla_afectada,registro_id,accion)
            VALUES(@actor,NULL,@empresa,'USUARIO',CONVERT(VARCHAR(100),@usuario),CASE WHEN @operacion='Editar' THEN 'EDITAR_ADMINISTRADOR' WHEN @activo=1 THEN 'ACTIVAR_ADMINISTRADOR' ELSE 'DESACTIVAR_ADMINISTRADOR' END);
        COMMIT;SELECT @usuario;
    END TRY
    BEGIN CATCH IF XACT_STATE()<>0 ROLLBACK;THROW;END CATCH;
END;