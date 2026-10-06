CREATE   PROCEDURE ObtenerNombreEmpresa
AS 
BEGIN
	SELECT id_empresa AS IdEmpresa, 
	nombre_comercial AS NombreComercial
	FROM EMPRESA
	WHERE id_estado = 1
END;