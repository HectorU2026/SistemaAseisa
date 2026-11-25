CREATE PROCEDURE dbo.ObtenerNombreEmpresa
AS
BEGIN
    SET NOCOUNT ON;
    SELECT E.id_empresa IdEmpresa,E.nombre_comercial NombreComercial
    FROM dbo.EMPRESA E INNER JOIN dbo.ESTADO S ON S.id_estado=E.id_estado
    WHERE LTRIM(RTRIM(S.nombre_estado))='Activo';
END;