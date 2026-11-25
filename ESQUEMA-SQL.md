# Fuente autoritativa del esquema SQL

El esquema compartido pertenece exclusivamente a `ASEISA.API/BD/BD.sqlproj`.
Modificar tablas y procedimientos en ese directorio; Seguridad.API accede a la misma
base de datos y no tiene un esquema independiente.

`Seguridad/Seguridad.API/BD/BD.sqlproj` permanece en la solucion como proyecto
de navegacion. Sus 24 elementos `None` enlazan los archivos originales mediante
`Link`. No importa SSDT, no compila SQL ni genera DACPAC. Los destinos Build,
Rebuild, SqlBuild, Publish, Deploy y sus variantes fallan expresamente con
`ASEISA_SQL_REFERENCE_ONLY`. La solucion conserva el proyecto pero no selecciona
su compilacion ni despliegue automaticos.

## Comprobacion local y CI

Desde la raiz, con PowerShell 7:

```powershell
pwsh -NoProfile -File ./scripts/Comprobar-EsquemaSql.ps1
dotnet msbuild ./Seguridad/Seguridad.API/BD/BD.sqlproj -t:ValidateReferences
```

El comprobador verifica enlaces, archivos autoritativos incluidos, ausencia de
copias fisicas y DACPAC secundarios y bloqueo de los destinos de artefactos.
GitHub Actions ejecuta esa comprobacion para cambios SQL relacionados. No
requiere acceso a base de datos, credenciales ni SSDT.

Para probar el bloqueo sin publicar ni conectarse a una base:

```powershell
dotnet msbuild ./Seguridad/Seguridad.API/BD/BD.sqlproj -t:Build
```

Debe fallar con `ASEISA_SQL_REFERENCE_ONLY`; no se debe agregar una excepcion para
producir un DACPAC secundario.

## Modificaciones futuras

1. Editar el archivo original en ASEISA.API/BD. No copiarlo a Seguridad.
2. Si se agrega otro enlace de navegacion, actualizar de forma revisada el
   inventario esperado del comprobador (actualmente 24).
3. Ejecutar la comprobacion y las pruebas de contratos/API afectadas.
4. Compilar el proyecto SQL autoritativo con SSDT compatible cuando este disponible.
5. Cualquier publicacion requiere autorizacion separada y revision del plan,
   respaldo, reversion y del script PostDeploy. Este documento no autoriza publicar.

Las definiciones existentes, incluidas aprobacion, hashes y auditoria, no cambian
por usar enlaces. Permanecen pendientes las pruebas SQL en una base sintetica y
la comprobacion de permisos de BITACORA.

El bloqueo del proyecto no invalida DACPAC generados anteriormente ni controla
SqlPackage ejecutado directamente sobre artefactos ajenos. No utilizar paquetes
historicos de Seguridad. El comprobador rechaza archivos `*.dacpac` en su carpeta;
un artefacto local previo se conserva con extension `.dacpac.disabled`.
