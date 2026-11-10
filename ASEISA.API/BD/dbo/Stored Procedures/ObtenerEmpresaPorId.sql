CREATE PROCEDURE dbo.ObtenerEmpresaPorId
    @id_empresa INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.id_empresa       AS IdEmpresa,
        e.razon_social     AS RazonSocial,
        e.nombre_comercial AS NombreComercial,
        e.cedula_juridica  AS CedulaJuridica,
        e.correo           AS Correo,
        e.telefono         AS Telefono,
        e.direccion        AS Direccion,
        e.moneda_base      AS MonedaBase,
        e.id_estado        AS IdEstado
    FROM dbo.EMPRESA e
    WHERE e.id_empresa = @id_empresa;
END;
