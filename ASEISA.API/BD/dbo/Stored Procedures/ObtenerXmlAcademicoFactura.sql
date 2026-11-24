CREATE PROCEDURE dbo.ObtenerXmlAcademicoFactura
    @id_empresa INT,
    @id_factura INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        x.id_factura         AS IdFactura,
        x.id_empresa         AS IdEmpresa,
        x.xml_firmado        AS XmlFirmado,
        x.huella_contenido   AS HuellaContenido,
        x.algoritmo_firma    AS AlgoritmoFirma,
        x.fecha_generacion   AS FechaGeneracion,
        x.aviso              AS Aviso
    FROM dbo.FACTURA_XML_ACADEMICO x
    INNER JOIN dbo.FACTURA f
        ON f.id_factura = x.id_factura
       AND f.id_empresa = x.id_empresa
    INNER JOIN dbo.ESTADO_FACTURA ef
        ON ef.id_estado_factura = f.id_estado_factura
    WHERE x.id_factura = @id_factura
      AND x.id_empresa = @id_empresa
      AND f.id_empresa = @id_empresa
      AND ef.nombre_estado IN ('Emitida', 'Pagada');
END;
