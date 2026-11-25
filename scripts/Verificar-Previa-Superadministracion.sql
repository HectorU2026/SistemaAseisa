-- Previa de solo lectura. Revisar en un entorno autorizado; no ejecutar automaticamente.
-- Compatible con el esquema anterior: no requiere es_plataforma.
-- No muestra correos, nombres, hashes, claves ni tokens.
SELECT U.id_usuario IdUsuario,U.id_empresa IdEmpresa,RU.id_rol IdRol,RU.id_estado IdEstadoAsignacion
FROM dbo.USUARIO U JOIN dbo.ROL_USUARIO RU ON RU.id_usuario=U.id_usuario JOIN dbo.ROL R ON R.id_rol=RU.id_rol
WHERE R.nombre='Superadministrador';
-- Cualquier fila anterior requiere decidir manualmente la migracion. No transferir identidades automaticamente.
SELECT nombre Rol,COUNT(*) Cantidad FROM dbo.ROL GROUP BY nombre HAVING COUNT(*)>1;
SELECT LTRIM(RTRIM(nombre_estado)) Estado,COUNT(*) Cantidad FROM dbo.ESTADO GROUP BY LTRIM(RTRIM(nombre_estado)) HAVING COUNT(*)>1;
SELECT REPLACE(cedula_juridica,'-','') CedulaJuridica,COUNT(*) Cantidad FROM dbo.EMPRESA GROUP BY REPLACE(cedula_juridica,'-','') HAVING COUNT(*)>1;
