CREATE FUNCTION dbo.EsAdministradorActivo(@id_usuario INT, @id_empresa INT)
RETURNS BIT
AS
BEGIN
    DECLARE @permitido BIT = 0;
    IF EXISTS (
        SELECT 1 FROM dbo.USUARIO U
        INNER JOIN dbo.ESTADO EU ON EU.id_estado=U.id_estado
        INNER JOIN dbo.EMPRESA E ON E.id_empresa=U.id_empresa
        INNER JOIN dbo.ESTADO EE ON EE.id_estado=E.id_estado
        WHERE U.id_usuario=@id_usuario AND U.id_empresa=@id_empresa
          AND U.estado_aprobacion='Aprobado' AND LTRIM(RTRIM(EU.nombre_estado))='Activo'
          AND LTRIM(RTRIM(EE.nombre_estado))='Activo'
          AND EXISTS (SELECT 1 FROM dbo.ROL_USUARIO RU
              INNER JOIN dbo.ROL R ON R.id_rol=RU.id_rol
              INNER JOIN dbo.ESTADO EA ON EA.id_estado=RU.id_estado
              INNER JOIN dbo.ESTADO ER ON ER.id_estado=R.id_estado
              WHERE RU.id_usuario=U.id_usuario AND R.nombre='Administrador'
                AND LTRIM(RTRIM(EA.nombre_estado))='Activo' AND LTRIM(RTRIM(ER.nombre_estado))='Activo'))
        SET @permitido=1;
    RETURN @permitido;
END;