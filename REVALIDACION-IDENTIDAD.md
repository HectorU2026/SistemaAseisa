> Registro historico de 1A.1. El estado actual y 1A.2 se documentan en MODULO-USUARIOS.md.

# Revalidacion de identidad y permisos — 1A.1

Las dos API validan criptograficamente el JWT y luego consultan `dbo.ObtenerContextoUsuarioAutorizado` en `RevalidacionJwtEvents.TokenValidated`. El resultado reemplaza completamente el principal: IdUsuario, Empresa, NombreUsuario, Correo y todos los roles vigentes. No se conservan roles ni identidades auxiliares del token anterior. Un rol agregado es efectivo en la siguiente solicitud; uno retirado deja de autorizarla.

El procedimiento existe solo en ASEISA.API/BD y el proyecto SQL secundario lo muestra como None/Link. Devuelve una fila con RolesJson (incluido []), sin credenciales. Verifica usuario activo/aprobado, pertenencia a empresa activa, asignacion activa y rol activo mediante ESTADO, sin IDs fijos. EXISTS evita duplicados de asignaciones.

Cada API tiene sus propios contratos DA/Flujo y usa su repositorio Dapper actual. ContextoUsuarioFlujo y los eventos se registran scoped: una consulta por pareja usuario/empresa en la solicitud, sin cache entre solicitudes. Se conserva la autorizacion especifica de controladores y procedimientos. No hay politica global nueva.

## Respuestas

- 401: JWT invalido o identidad/empresa no vigente, inexistente o inconsistente.
- 403: identidad vigente sin el rol exigido por la operacion.
- 503 generico: fallo tecnico de consulta o contrato interno invalido. El middleware no ejecuta la operacion protegida; el log 1002 no incluye excepciones, SQL, parametros ni tokens.
- Cero roles conserva una identidad autenticada, pero no satisface atributos con roles. Los controles adicionales de endpoints existentes siguen aplicando.
- Los endpoints anonimos existentes y los eventos de auditoria no se modifican.

## Dependencia de instalacion (pendiente, no autorizada en esta entrega)

No arrancar estas versiones para uso operativo antes de instalar el procedimiento en el destino aprobado. Si falta el procedimiento, las solicitudes protegidas con JWT valido devolveran 503. No hay fallback a permisos antiguos.

La compilacion SSDT realizada utiliza la plataforma Sql170 actual del proyecto y NO es un artefacto para publicar directamente sobre SQL Server 2022. Para ese destino se requiere preparar y validar Sql160 como en las fases anteriores. Esta entrega no publica DACPAC, ejecuta SQL ni modifica ninguna base.

## Pruebas aisladas

Desde la raiz, ejecutar en un entorno autorizado:

```powershell
dotnet build tests/RevalidacionIdentidad/ASEISA/RevalidacionIdentidad.ASEISA.csproj -c Debug
dotnet run --project tests/RevalidacionIdentidad/ASEISA/RevalidacionIdentidad.ASEISA.csproj -c Debug --no-build --no-restore
dotnet build tests/RevalidacionIdentidad/Seguridad/RevalidacionIdentidad.Seguridad.csproj -c Debug
dotnet run --project tests/RevalidacionIdentidad/Seguridad/RevalidacionIdentidad.Seguridad.csproj -c Debug --no-build --no-restore
./scripts/Comprobar-EsquemaSql.ps1
```

Los dos ejecutores enlazan los archivos de producto correspondientes y utilizan las versiones JwtBearer/JWT de cada API. No contienen servidor HTTP, configuracion de conexion ni acceso SQL. Usan middleware real de autenticacion/autorizacion con un DA falso. Prueban firma, algoritmo, emisor, audiencia, expiracion, claims ambiguos, empresa incorrecta, roles retirados/cero/multiples, errores 401/403/503, no ejecucion del endpoint rechazado y ausencia de cache entre solicitudes.

Resultado local: 30 escenarios por API, 60 aprobados. Compilaciones Debug: ASEISA.API y Seguridad.API sin errores. Advertencias existentes de nulabilidad/paquetes y System.Data.SqlClient obsoleto; el DA nuevo de Seguridad conserva ese proveedor por compatibilidad. SSDT: sin errores, diez SQL71502 preexistentes sobre sys.sp_getapplock. Referencias SQL: 15 validas.

Las revocaciones se simulan mediante ausencia/cambio del contexto entregado por el DA falso. No sustituyen pruebas del procedimiento, mapeo Dapper y concurrencia en una base sintetica. Estas pruebas quedan pendientes de autorizacion. Si Windows bloquea un ejecutor, detenerse sin renombrarlo ni cambiar politicas.

## Limites y siguiente entrega

- La revocacion se comprueba al autenticar cada solicitud; no cancela operaciones ya autorizadas en curso. Se mantienen las comprobaciones transaccionales de negocio.
- Controladores anonimos permanecen anonimos. Esto no es una auditoria ni correccion de permisos de todos los modulos.
- Los controladores revisados consumen HttpContext.User; no se encontro autorizacion mediante decodificacion manual del bearer. Los helpers existentes que reconsultan empresa o validan recursos conservan sus controles.
- ObtenerInformacionUsuario conserva ruta, verbo y contrato, ademas de sus validaciones propias. No se agrega un endpoint de sesion en 1A.1.
- ASEISA.Web no se modifica. En 1A.2: consulta autenticada de sesion, sincronizar todos los roles de la cookie, rechazar sesiones revocadas y definir el tratamiento de 401/403/503 sin ampliar la vigencia del JWT.
- CONF-006, CONF-010, asignaciones de roles, ObtenerUsuario, RegistroUsuario y CambiarEstadoUsuario permanecen intactos respecto del inicio de esta entrega.