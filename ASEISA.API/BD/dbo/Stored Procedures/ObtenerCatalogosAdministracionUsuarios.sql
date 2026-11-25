CREATE PROCEDURE dbo.ObtenerCatalogosAdministracionUsuarios @id_empresa INT,@id_usuario_accion INT
AS
BEGIN
    SET NOCOUNT ON;
    IF dbo.EsAdministradorActivo(@id_usuario_accion,@id_empresa)=0 THROW 51025,'Operacion no autorizada.',1;
    SELECT R.id_rol Id,R.nombre Nombre FROM dbo.ROL R INNER JOIN dbo.ESTADO E ON E.id_estado=R.id_estado
        WHERE LTRIM(RTRIM(E.nombre_estado))='Activo' AND R.nombre<>'Superadministrador' ORDER BY R.nombre;
    SELECT id_estado Id,LTRIM(RTRIM(nombre_estado)) Nombre FROM dbo.ESTADO WHERE LTRIM(RTRIM(nombre_estado)) IN ('Activo','Inactivo');
END;