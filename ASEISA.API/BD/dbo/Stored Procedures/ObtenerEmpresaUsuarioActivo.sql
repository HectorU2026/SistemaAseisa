CREATE   PROCEDURE dbo.ObtenerEmpresaUsuarioActivo
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.id_empresa
    FROM dbo.USUARIO u
    INNER JOIN dbo.ESTADO e
        ON e.id_estado = u.id_estado
    WHERE u.id_usuario = @id_usuario
      AND LTRIM(RTRIM(e.nombre_estado)) = 'Activo';
END;