CREATE PROCEDURE dbo.ValidarCorreoActivo @correo VARCHAR(150)
AS
BEGIN
 SET NOCOUNT ON;
 SELECT U.correo,U.nombre Nombre FROM dbo.USUARIO U
 INNER JOIN dbo.ESTADO EU ON EU.id_estado=U.id_estado
 INNER JOIN dbo.EMPRESA E ON E.id_empresa=U.id_empresa
 INNER JOIN dbo.ESTADO EE ON EE.id_estado=E.id_estado
 WHERE U.correo=@correo AND U.estado_aprobacion='Aprobado'
 AND LTRIM(RTRIM(EU.nombre_estado))='Activo' AND LTRIM(RTRIM(EE.nombre_estado))='Activo';
END;