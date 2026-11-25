CREATE PROCEDURE dbo.GuardarEmpresaPlataforma @actor INT,@empresa INT=NULL,@razon_social VARCHAR(200),@nombre_comercial VARCHAR(200)=NULL,
    @cedula_juridica VARCHAR(30),@correo VARCHAR(150)=NULL,@telefono VARCHAR(30)=NULL,@direccion VARCHAR(300)=NULL,@moneda_base VARCHAR(10),@activo BIT=NULL
AS
BEGIN
    SET NOCOUNT ON;SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF dbo.EsSuperadministradorActivo(@actor)=0 THROW 51025,'Operacion de plataforma no autorizada.',1;
        IF NULLIF(LTRIM(RTRIM(@razon_social)),'') IS NULL OR NULLIF(LTRIM(RTRIM(@moneda_base)),'') IS NULL THROW 51307,'Datos de empresa invalidos.',1;
        SET @cedula_juridica=REPLACE(LTRIM(RTRIM(@cedula_juridica)),'-','');
        IF LEN(@cedula_juridica)<>10 OR @cedula_juridica LIKE '%[^0-9]%' OR LEFT(@cedula_juridica,1)<>'3' THROW 51307,'Cedula juridica invalida.',1;
        IF EXISTS(SELECT 1 FROM dbo.EMPRESA WITH(UPDLOCK,HOLDLOCK) WHERE REPLACE(cedula_juridica,'-','')=@cedula_juridica AND (@empresa IS NULL OR id_empresa<>@empresa)) THROW 51308,'La cedula juridica ya esta registrada.',1;
        DECLARE @estado INT=(SELECT MIN(id_estado) FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado))=CASE WHEN ISNULL(@activo,1)=1 THEN 'Activo' ELSE 'Inactivo' END);
        IF @estado IS NULL THROW 51020,'Catalogo de estados incompleto.',1;
        IF @empresa IS NULL
        BEGIN
            INSERT dbo.EMPRESA(razon_social,nombre_comercial,cedula_juridica,correo,telefono,direccion,moneda_base,id_estado)
                VALUES(@razon_social,@nombre_comercial,@cedula_juridica,@correo,@telefono,@direccion,@moneda_base,@estado);
            SET @empresa=CONVERT(INT,SCOPE_IDENTITY());
        END
        ELSE
        BEGIN
            IF NOT EXISTS(SELECT 1 FROM dbo.EMPRESA WITH(UPDLOCK,HOLDLOCK) WHERE id_empresa=@empresa) THROW 51015,'Empresa no disponible.',1;
            UPDATE dbo.EMPRESA SET razon_social=@razon_social,nombre_comercial=@nombre_comercial,cedula_juridica=@cedula_juridica,
                correo=@correo,telefono=@telefono,direccion=@direccion,moneda_base=@moneda_base,
                id_estado=CASE WHEN @activo IS NULL THEN id_estado ELSE @estado END,fecha_modificacion=SYSDATETIME() WHERE id_empresa=@empresa;
        END;
        INSERT dbo.BITACORA(id_usuario,id_empresa,id_empresa_objetivo,tabla_afectada,registro_id,accion)
            VALUES(@actor,NULL,@empresa,'EMPRESA',CONVERT(VARCHAR(100),@empresa),'GUARDAR_EMPRESA');
        COMMIT;SELECT @empresa;
    END TRY
    BEGIN CATCH IF XACT_STATE()<>0 ROLLBACK;THROW;END CATCH;
END;