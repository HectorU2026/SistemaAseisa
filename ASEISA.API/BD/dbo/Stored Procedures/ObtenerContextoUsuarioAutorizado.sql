CREATE PROCEDURE dbo.ObtenerContextoUsuarioAutorizado
    @id_usuario INT,
    @id_empresa INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Una sola consulta y una fila por identidad vigente, incluso sin roles activos.
    -- Los permisos no se obtienen de los claims del token ni de estados numericos fijos.
    SELECT U.version_credencial AS VersionCredencial, U.id_usuario AS IdUsuario,
           U.id_empresa AS IdEmpresa,
           U.nombre_usuario AS NombreUsuario,
           U.correo AS Correo,
           (SELECT R.id_rol AS IdRol, R.nombre AS NombreRol
            FROM dbo.ROL R
            INNER JOIN dbo.ESTADO ER ON ER.id_estado = R.id_estado
            WHERE LTRIM(RTRIM(ER.nombre_estado)) = 'Activo'
              AND EXISTS (
                  SELECT 1
                  FROM dbo.ROL_USUARIO RU
                  INNER JOIN dbo.ESTADO EA ON EA.id_estado = RU.id_estado
                  WHERE RU.id_usuario = U.id_usuario AND RU.id_rol = R.id_rol
                    AND LTRIM(RTRIM(EA.nombre_estado)) = 'Activo')
            ORDER BY R.id_rol
            FOR JSON PATH) AS RolesJson
    FROM dbo.USUARIO U
    INNER JOIN dbo.ESTADO EU ON EU.id_estado = U.id_estado
    LEFT JOIN dbo.EMPRESA E ON E.id_empresa = U.id_empresa
    LEFT JOIN dbo.ESTADO EE ON EE.id_estado = E.id_estado
    WHERE U.id_usuario = @id_usuario
      AND (U.id_empresa = @id_empresa OR (U.id_empresa IS NULL AND @id_empresa IS NULL))
      AND U.estado_aprobacion = 'Aprobado'
      AND LTRIM(RTRIM(EU.nombre_estado)) = 'Activo'
      AND dbo.UsuarioVigente(U.id_usuario)=1;
END;