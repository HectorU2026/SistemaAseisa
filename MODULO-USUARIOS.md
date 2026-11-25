# Modulo de usuarios ASEISA — cierre local

Estado al 18 de noviembre de 2026: implementacion local y compilaciones correctas; integracion real pendiente. No se modificaron ni publicaron bases, no se ejecutaron SSDT ni aplicaciones bloqueadas, no se enviaron correos reales y no se hicieron commit/push.

## Funcionalidades completas en codigo

Se conservan las consultas y paginas administrativas, registro publico, CONF-006 por invitacion, CONF-010 por enlace, edicion auditada, aprobacion/rechazo, estados y administracion multirrol. Agregar/retirar roles no afecta otras asignaciones. Ambas API revalidan identidad, actividad, aprobacion, empresa y roles contra SQL; Web sincroniza todos los roles de su cookie. Una sesion revocada produce 401 y cierre de cookie; una indisponibilidad produce 503 sin autorizar con permisos antiguos.

### Revocacion por cambio de contrasena

- USUARIO.version_credencial es un contador no negativo, inicialmente cero. Login emite VersionCredencial en el JWT.
- Cada API exige una unica version valida y la compara con SQL en el componente central de revalidacion. Una version anterior o ausente produce 401, aunque el JWT no haya vencido. Web recibe ese 401 desde api/Sesion y elimina la cookie.
- Consumir un enlace incrementa la version, cambia el hash, consume todos los enlaces anteriores, anula correos pendientes y registra la auditoria en una sola transaccion. Una operacion fallida revierte todo.
- Los enlaces estan vinculados tambien a la version con la que fueron emitidos. Un cambio posterior vuelve inutilizables los enlaces anteriores aun si otro consumidor interno actualizo la contrasena.
- La migracion de un hash antiguo al formato actual usa MigrarHashAutenticacion: compara el hash y version leidos antes de actualizar. No incrementa la version porque no cambia el secreto, y no puede sobrescribir un restablecimiento concurrente.
- El procedimiento historico CambiarContrasena incrementa la version si se usa para un cambio real; ya no se usa para migrar hashes. El restablecimiento administrativo directo por hash sigue deshabilitado.

Los JWT existentes sin VersionCredencial necesitan nuevo inicio de sesion al desplegar esta version. No hay fallback que acepte su vigencia anterior. Las solicitudes ya autorizadas y en curso no se cancelan retroactivamente.

### Bandeja persistente de correo

- Todas las invitaciones, restablecimientos administrativos y recuperaciones publicas guardan solicitud y CORREO_ENLACE en la misma transaccion SQL. En el alta administrativa tambien se incluye la creacion del usuario. La peticion HTTP no depende de SMTP.
- SOLICITUD_CONTRASENA conserva solo el resumen SHA256 del token. CORREO_ENLACE guarda un sobre AES-256-GCM autenticado, con nonce aleatorio y vinculado al resumen de la solicitud. No persiste el enlace en claro ni la clave de cifrado en SQL.
- DespachadorEnlaces consulta SQL. Reserva el trabajo durante cinco minutos; un proceso reiniciado puede recuperar una reserva vencida. Usa un identificador de reserva para impedir que un despachador antiguo confirme el trabajo de otro.
- Hasta cinco intentos, con demoras de 30, 60, 120 y 300 segundos. Despues queda Fallido. Si el correo esta vencido, consumido, reemplazado o su usuario/empresa/version dejaron de ser validos, queda Anulado. Se borra el sobre cifrado al enviar o terminar.
- Se mantienen el orden de bloqueos usuario -> solicitud -> correo y las validaciones de vigencia antes del despacho. Una modificacion posterior al inicio de SMTP puede volver inutilizable el enlace; el consumo siempre vuelve a validar SQL.
- Entrega al menos una vez: SMTP no participa de una transaccion SQL. Si SMTP acepta y el proceso cae antes de confirmar, puede repetirse el correo. El enlace sigue siendo de un solo uso y no permite un segundo cambio de contrasena.
- EnvioProgramado confirma persistencia; no afirma entrega. Se conserva EnlaceEnviado por compatibilidad, sin marcarlo true desde una peticion que no ha enviado correo. La cabecera de alta X-Invitacion-Estado usa Programado/Pendiente y Web muestra mensajes coherentes.
- No se registran tokens, sobres cifrados, hashes, contrasenas, credenciales ni respuestas SMTP en logs propios o BITACORA. El enlace mantiene el secreto en fragmento, fuera del GET y Referer; el formulario usa no-store/no-referrer.

### Uso unico y concurrencia

La emision serializa por usuario e invalida solicitudes anteriores. El consumo bloquea usuario y solicitud, compara version/empresa/correo/aprobacion/actividad y vencimiento UTC, y confirma consumo junto al nuevo hash y auditoria. Dos solicitudes concurrentes con el mismo enlace no pueden confirmar dos cambios. Esta propiedad se reviso estaticamente; falta demostrarla con SQL Server en un entorno autorizado.

## Configuracion antes del despliegue

| Aplicacion | Requisito |
|---|---|
| ASEISA.API | Servicios__SeguridadUrl: URL HTTPS confiable de Seguridad.API incluyendo /api. Bearer del usuario, validacion TLS y sin redirecciones. |
| Seguridad.API | App__BaseUrl: URL HTTPS completa del formulario /Seguridad/Cambiar_Contrasena, sin query ni fragmento. SMTP de pruebas o produccion segun el entorno autorizado. |
| Seguridad.API | CorreoEnlaces__ClaveCifrado: secreto Base64 de 32 bytes aleatorios. Mantenerlo estable y protegido fuera del repositorio y de SQL, mediante el almacen de secretos/configuracion del entorno. |
| Seguridad.API | CorreoEnlaces__EnviosHabilitados: false por defecto. Habilitar explicitamente solo tras comprobar base, SMTP y destinatarios autorizados. Deshabilitado no descarta los trabajos persistidos. |
| Ambas API | JWT y conexiones compatibles. Para integrar, usar exclusivamente la base sintetica autorizada con configuracion temporal. |
| Web | ApiEndPoints y ApiEndPointsSeguridad existentes. No se editaron appsettings ni User Secrets. |

Todas las instancias despachadoras deben tener la misma clave. Respaldar la clave por separado y proteger su acceso. Detener temporalmente emision y despacho antes de rotarla. No reemplazarla mientras existan sobres pendientes: drenar/anular los envios y emitir nuevos enlaces antes de rotar. Perderla vuelve irrecuperables los sobres; no permite recuperar contrasenas ni tokens desde sus hashes.

El timeout SMTP debe ser menor que los cinco minutos de reserva. Se puede supervisar la bandeja consultando unicamente IDs, estado, intentos y fechas; no exportar sobres ni datos personales para diagnosticar. Los reenvios administrativos crean una solicitud nueva y anulan la anterior.

## SQL pendiente, sin ejecutar

Fuente unica: ASEISA.API/BD. El secundario mantiene 24 referencias None/Link y no genera DACPAC.

Cambios de este cierre:
- Columna version_credencial y constraints/default en USUARIO y version de emision en SOLICITUD_CONTRASENA.
- Tabla CORREO_ENLACE, FK, unicidad por solicitud, constraints e indice de despacho.
- Nuevos MigrarHashAutenticacion, TomarCorreoEnlace y FinalizarCorreoEnlace.
- ObtenerUsuario y ObtenerContextoUsuarioAutorizado devuelven la version.
- CrearSolicitudContrasena persiste tambien el sobre y el trabajo de correo.
- ConsumirSolicitudContrasena valida la version e incrementa el contador; anula los correos anteriores.
- CambiarContrasena incrementa la version para cambios reales.

Tambien siguen pendientes los cambios previos del modulo: ObtenerContextoUsuarioAutorizado, EsAdministradorActivo, SOLICITUD_CONTRASENA, registro/invitacion, roles independientes, edicion/auditoria y consultas administrativas. No publicar solamente los archivos del cierre sobre un esquema que carezca de esas dependencias.

## Validaciones de este cierre

- ASEISA.API, Seguridad.API y ASEISA.Web compilan en Debug sin errores. Persisten advertencias existentes de paquetes/nulabilidad y System.Data.SqlClient obsoleto.
- 41 pruebas aisladas de ASEISA aprobadas, incluyendo version ausente/ambigua/invalida, JWT anterior rechazado, JWT nuevo permitido y cookies ante 401/503.
- El ejecutor de Seguridad compila pero Windows bloqueo su ejecucion con 0x800711C7. No se reintento ni se alteraron opciones para eludirlo. Sus casos de AES-GCM, persistencia/despacho simulados estan pendientes de ejecucion; la migracion concurrente requiere SQL real.
- 153 SQL analizados con TSql160Parser: sin errores sintacticos. No equivale a SSDT ni a verificacion semantica.
- Comprobacion de enlaces SQL: 23 validos, sin definiciones duplicadas ni artefactos secundarios.

No se ejecutaron pruebas integrales, concurrencia real, SQL ni SMTP. Los resultados historicos de 82 pruebas del bloque anterior no validan por si solos este cierre.

## Pasos concretos para desplegar y probar (pendientes de autorizacion)

1. Preparar un entorno autorizado donde puedan compilarse SSDT y ejecutarse las aplicaciones sin alterar controles de Windows. Conservar respaldo y estrategia de recuperacion del esquema/datos y de la clave de cifrado.
2. Generar Sql160 temporal desde todas las fuentes actuales, comprobar hash/procedencia y modelo. No usar el DACPAC antiguo ni publicar Sql170 sobre SQL Server 2022.
3. Reconfirmar JACP-DESKTOP130/Aseisa_Pruebas/ID6 y fixture. Generar DeployReport y script con ScriptDatabaseOptions=False, AllowIncompatiblePlatform=False, BlockOnPossibleDataLoss=True y sin eliminaciones automaticas. Revisar cambios ajenos al modulo/preexistentes, defaults y PostDeploy antes de solicitar publicacion.
4. Con aprobacion separada, instalar el esquema completo en la base aislada y comprobar objetos, columnas, FK, permisos de los procedimientos nuevos/BITACORA y que la fixture no haya cambiado.
5. Configurar las aplicaciones solo para esa base; suministrar la clave protegida, JWT compatibles y URL HTTPS. Mantener envios deshabilitados mientras se comprueban el destino y los destinatarios. Desplegar ambas API y Web coordinadamente y exigir nuevo login para JWT sin version.
6. Probar JWT antes/despues de cambio/restablecimiento, cookies, roles y empresa. Probar dos consumos simultaneos del mismo enlace: exactamente uno debe cambiar hash/version y auditar; probar tambien expiracion, reemplazo, cambio de correo/empresa/estado/version y migracion de hash concurrente.
7. Con SMTP de pruebas autorizado, habilitar envios y probar reinicio antes/despues de reserva, error SMTP, caida entre envio/confirmacion, dos despachadores, maximo de intentos, cancelacion y borrado del sobre terminal. Probar clave incorrecta/sobre alterado sin filtrar detalles. Aceptar posible correo duplicado, nunca doble consumo.
8. Verificar alta/invitacion, aprobacion, gestion multirrol y paginas Razor con datos ficticios; comprobar respuestas publicas genericas, auditoria sin secretos y limpieza exclusivamente de datos sintéticos autorizados.

No quedan decisiones funcionales abiertas dentro del cierre solicitado. Quedan configuracion operativa, autorizacion de despliegue y pruebas reales. La revision de autorizacion en modulos ajenos identificados en la entrega anterior queda fuera de este cierre.
## Mejoras finales de experiencia y validación — 24 de noviembre de 2026

Este bloque conserva JWT, revalidación, sesiones, invitaciones y auditoría existentes. No implementa SES-008 ni define una matriz de módulos por rol. Las comprobaciones manuales de correo y contraseña comunicadas por el usuario corresponden a la versión anterior; no prueban integralmente estas modificaciones.

### Comportamiento actualizado

- Listado con búsqueda por nombre/correo/usuario, filtros por estado, aprobación y rol, navegación de pendientes, etiquetas, roles ordenados y páginas de 15 elementos. Filtrado y paginación en Web sobre la consulta existente; para volúmenes altos convendrá paginación SQL posterior. La vista de pendientes usa el listado completo para conservar los roles en sus filtros; el endpoint de solicitudes permanece disponible.
- Creación y detalle conservan `_Layout` y el menú lateral. Reutilizan `contactos.css`, botones de Contactos, formularios y modales Bootstrap. `usuarios.css` agrega únicamente ajustes del módulo.
- El nombre de usuario se genera en el servidor con la regla pública existente (inicial + primer apellido sin tildes + últimos cuatro dígitos de identificación). Web y ambas API enlazan una sola fuente `Compartido/Usuarios/ReglasRegistroUsuario.cs`. No cambia al editar datos; Web lo muestra de sólo lectura, la API rechaza diferencias y SQL nunca lo actualiza. Una colisión del nombre generado se rechaza; no se agregó un sufijo ni otra regla automática.
- Registro público muestra confirmación explícita de aprobación pendiente al regresar al login.
- El login usa `ObtenerCandidatoAutenticacion`, contrato exclusivamente interno con hash. Verifica primero la contraseña y sólo entonces informa pendiente, rechazado o inactivo. Contraseña incorrecta/correo inexistente conservan respuesta genérica. No emite JWT ni audita un login exitoso para cuentas no elegibles. `ObtenerUsuario` y la consulta propia conservan sus filtros y contratos seguros. La respuesta de login agrega `CodigoEstado` opcional sin retirar campos existentes.
- Alta administrativa admite `IdsRoles` (uno o varios). Se conserva `IdRol` como alternativa para consumidores anteriores. La colección, si tiene elementos, es autoritativa. SQL valida todos los roles activos, impide Superadministrador y asigna todos en la transacción existente junto con CLIENTE, cuando corresponde, y la invitación persistente. No hay contraseñas elegidas por el administrador ni hashes del navegador.
- Alta administrativa exige tipo e identificación para generar la identidad, incluso cuando no se asigna Cliente; para persona jurídica exige razón social. Nombres/apellidos tienen máximo 80, correo 150, teléfono 30, dirección 300 e identificación 50 caracteres. Los consumidores administrativos anteriores deben enviar esos datos. Se conservan los valores enviados ante errores y se muestran validaciones por campo. La unicidad se mantiene en SQL.
- Retirar el último rol activo se impide en Web/API/SQL. SQL mantiene el bloqueo `UPDLOCK,HOLDLOCK` del usuario durante la comprobación y retirada; dos retiradas del mismo usuario se serializan. Sólo cuentan asignaciones y roles del catálogo activos. Los errores 51210/51211 son funcionales controlados, sin mensajes SQL públicos.
- El detalle ofrece aprobar/rechazar sólo para pendientes; activar o desactivar, según corresponda, para aprobados; no ofrece transiciones nuevas para rechazados. El envío de enlace conserva la elegibilidad aprobada/activa y las validaciones de empresa del servidor. Los cambios de acceso y roles usan confirmación. Los mensajes distinguen agregar/retirar, activar/desactivar y programar un envío de confirmar su entrega.

### Configuración necesaria (sin valores reales)

Usar **Administrar secretos de usuario** en el proyecto API correspondiente de Visual Studio. Los secretos de ASEISA.API y Seguridad.API son almacenes distintos. Web no tiene actualmente UserSecretsId: no agregar valores sensibles a su appsettings; utilizar variables de entorno o el almacén de configuración autorizado para el entorno. No se han modificado estos almacenes ni archivos de configuración.

| Aplicación | Clave | Valor requerido |
|---|---|---|
| ASEISA.API | `Servicios:SeguridadUrl` | URL HTTPS de Seguridad.API con `/api`, sin query ni fragmento. Se reenvía Bearer validado; TLS debe ser confiable y no se permiten redirecciones del cliente interno. |
| Seguridad.API | `CorreoEnlaces:ClaveCifrado` | Secreto Base64 de 32 bytes aleatorios; estable y respaldado fuera del repositorio/SQL. No rotarlo con trabajos pendientes. |
| Seguridad.API | `CorreoEnlaces:EnviosHabilitados` | `true` sólo en entorno y destinatarios autorizados; predeterminado deshabilitado. |
| Seguridad.API | `Email:Remitente` | Cuenta Gmail remitente autenticada por el componente SMTP actual. El destinatario puede ser de otro proveedor. |
| Seguridad.API | `Email:Nombre` | Nombre visible del remitente. |
| Seguridad.API | `Email:Password` | Contraseña de aplicación de la cuenta remitente, guardada como secreto; nunca contraseña del destinatario. |
| Seguridad.API | `App:BaseUrl` | URL HTTPS completa de Web `/Seguridad/Cambiar_Contrasena`, sin query ni fragmento. Invitación y recuperación reutilizan este formulario; el token se agrega en el fragmento. |
| Ambas API | `Token` y conexión existente | Mismos emisor/audiencia/clave y algoritmo autorizado; conexión exclusivamente al destino aprobado. No confundir conexión de desarrollo con producción. |
| Web | `ApiEndPoints:UrlBase`, `ApiEndPointsSeguridad` | URL y métodos HTTPS existentes de ambas API, coherentes con los puertos del entorno. |

El componente de correo actual utiliza `smtp.gmail.com:587` con STARTTLS; agregar claves Host/Port a configuración no cambia ese código. Los certificados locales HTTPS deben ser confiables para las tres aplicaciones. No desactivar la validación TLS. Las variables de entorno usan doble guion bajo, por ejemplo `CorreoEnlaces__EnviosHabilitados`, y tienen precedencia sobre archivos/secretos según el proveedor .NET; verificar también launchSettings del perfil utilizado.

El trabajador persistente ya registrado en Seguridad.API procesa CORREO_ENLACE; habilitar envíos no confirma entrega. Revisar sólo IDs, estado, intentos y fechas. No consultar/exportar token_cifrado, hash_token, contrasena_hash ni contenido de enlaces para diagnosticar.

### SQL y despliegue pendiente de este bloque

1. Nuevo `dbo.ObtenerCandidatoAutenticacion` en el proyecto autoritativo y enlace `None` en Seguridad.API (24 enlaces en total).
2. `dbo.RegistroUsuario`: parámetro opcional `@roles_json`, validaciones equivalentes de registro y alta multirrol transaccional. La llamada pública sigue forzando Cliente.
3. `dbo.EditarUsuario`: nombre de usuario inmutable, error 51210.
4. `dbo.GestionarRolUsuario`: restricción del último rol, error 51211 y comprobación bajo bloqueo por usuario.

No se modificaron `ObtenerUsuario` ni `CambiarEstadoUsuario` en este bloque. **No desplegar las aplicaciones nuevas antes del SQL correspondiente**: el login necesita el procedimiento nuevo y el alta administrativa envía `@roles_json`. Generar y revisar el plan de despliegue completo en un entorno autorizado antes de publicarlo. No se ejecutó SQL ni SSDT y no se modificó ninguna base de datos.

### Verificación y pruebas pendientes

- Compilaciones Debug de ambas API, Web y ejecutores de pruebas: consultar el informe de entrega para resultados finales. La compilación no valida Dapper/SQL en ejecución.
- 56 casos aislados ASEISA aprobados, sin red ni SQL: regresiones JWT/cookies y 15 casos de generación de identidad, identificación, roles y validación de contratos Web/API.
- Se agregaron diez casos aislados del login a Seguridad (aprobado, multirrol, pendiente, rechazado, inactivo, empresa inactiva, sin roles, contraseña incorrecta y correo inexistente). Sólo se compilan: no se ejecuta el ensamblado bloqueado previamente por Smart App Control.
- Parser estático TSql160 y comprobación de enlaces no sustituyen SSDT, permisos ni ejecución real.
- Pendiente en base sintética autorizada: alta con uno/varios roles, Cliente combinado, correo/usuario duplicado y rollback completo ante rol inválido; integridad CLIENTE/ROL_USUARIO; invitación única y auditoría existente.
- Pendiente: dos retiradas concurrentes de roles de un mismo usuario dejan al menos uno activo; retirada repetida y roles de catálogo/asignaciones inactivos; permisos y empresa ajena; solicitud directa que intente cambiar nombre de usuario.
- Pendiente: login real para cada estado con contraseña correcta/incorrecta; ningún estado revelado con contraseña incorrecta; JWT multirrol, versión y auditoría única. Confirmar que la consulta propia siga sin hash.
- Pendiente visual/manual: escritorio y móvil, menú lateral, filtros combinados, paginación, conservar campos tras error, modales (incluido agregar rol), textos de confirmación pública y acciones por estado. No se inició Web contra una base desconocida para revisar el diseño.
- No se enviaron correos, no se publicaron procedimientos, no se cambiaron configuraciones permanentes y no se realizó commit/push.
## Ajustes finales: botones y autoadministración

- Los botones de usuarios reutilizan las clases existentes con separación de 8 px, transiciones de 180 ms, bordes visibles, foco de teclado y variantes destructivas. Se respeta movimiento reducido y se excluyen controles deshabilitados. El alcance CSS sigue limitado a `.usuarios-page`, incluidos sus modales.
- El listado identifica la cuenta autenticada por `NameIdentifier`, muestra «Tu cuenta» y no ofrece Administrar. Todos los handlers del detalle rechazan el propio ID antes de consultar las API y regresan al listado con un aviso.
- `ImpedirAutoadministracionAttribute`, compartido por los controladores administrativos de ambas API, devuelve 403 y `AUTOADMINISTRACION_NO_PERMITIDA` antes de ejecutar acciones cuyo `idUsuario` sea el ID autenticado. Cubre consulta administrativa, edición, estado, roles, aprobación y restablecimiento; no se aplica al controlador de información personal ni a los flujos públicos/personales de contraseña. La revalidación JWT y las comprobaciones de empresa/permiso permanecen vigentes.
- No hay SQL nuevo o modificado para este ajuste; no requiere publicación adicional de esquema.
- Validación: tres aplicaciones compiladas sin errores hacia carpetas temporales porque los binarios habituales estaban en uso por Visual Studio. No se cerraron esos procesos ni se ejecutaron las salidas temporales. Persisten advertencias previas.
- 67 pruebas aisladas ASEISA aprobadas, incluidas 11 del filtro administrativo que verifican que no se ejecute la acción rechazada. Sin SQL, red ni correos. Pendiente comprobar manualmente hover/foco/modales y el recorrido real Web/API tras reiniciar y recompilar las aplicaciones en Visual Studio.
## Coherencia del registro administrativo y público

- Empresa administrativa: se conserva la procedencia exclusiva del claim Empresa revalidado en Seguridad.API. Ni UsuarioRequest, RegistroAdministrativoRequest ni FormularioUsuario incluyen IdEmpresa. Una propiedad adicional enviada por el cliente no modifica la empresa de la operación. No se agregó selección de empresa.
- El catálogo administrativo agrega NombreEmpresa; después de validar al administrador mediante ObtenerCatalogosAdministracionUsuarios, reutiliza ObtenerEmpresaPorId con el ID autenticado y devuelve únicamente el nombre comercial (o razón social si no existe). Web lo muestra readonly y sin atributo name. No confundirlo con la selección de empresa que sí corresponde al registro público. Verificar permiso de ejecución de ObtenerEmpresaPorId para la identidad SQL de ASEISA.API.
- registro-campos.js concentra máscaras y comprobaciones de identificación, teléfono y correo. registro.js público y usuarios-formulario.js administrativo reutilizan esas funciones. No hay nuevos tipos de identificación: el público actual sólo ofrece Persona física y Persona jurídica.
- Cédula física: 1-2345-6789; jurídica: 3-101-123456. La máscara coloca guiones, elimina caracteres ajenos a 0–9 y limita a 9/10 dígitos. El cambio de tipo limpia la identificación. El backend acepta formato compacto o guiones correctamente colocados, rechaza caracteres/prefijos/longitudes incorrectos y normaliza a dígitos sin guiones al enviar a Dapper, como el SQL existente del registro público. No se cambia el almacenamiento ni la definición SQL.
- Se reutilizan atributos de validación de correo y teléfono en Web y ambas API: correo con dominio y punto, máximo 150; teléfono opcional de ocho dígitos, compacto o 1234-5678, normalizado con guion. Cierra la diferencia entre la validación JavaScript pública de ocho dígitos y el antiguo PhoneAttribute más permisivo del servidor.
- Se alinean nombres/apellidos de alta y edición a 80 caracteres; no se inventa una restricción de sólo letras que el registro público no tenía. Dirección máxima 250, coherente con el HTML público existente; razón social máxima 200. La edición conserva exclusivamente los campos personales que ya eran editables, sin habilitar identificación, empresa ni nombre de usuario.
- La interfaz mantiene mensajes por campo y valores enviados ante errores; no se modificaron comprobaciones de duplicados. Los cambios a Dapper se limitan a los datos personales/catálogos. No se modificó generación de credenciales, invitaciones, sesiones, auditoría, asignación multirrol ni aprobación.
- Validación: 79 casos aislados .NET aprobados y 14 JavaScript sin SQL/red/correos; tres aplicaciones compiladas sin errores en salidas temporales. Persisten advertencias previas. JavaScript revisado además con node --check. No se ejecutaron API ni el runner Seguridad previamente bloqueado.
- Pendiente manual en entorno autorizado: nombre de empresa correcto tras GET y POST inválido; intentar IdEmpresa ajena en solicitudes a ambas API y comprobar empresa real del alta; pegar identificaciones y cambiar tipo; comprobar mensajes y edición; duplicados e integridad SQL; invitación sin cambios. No hay cambios SQL de este ajuste pendientes de publicación. No se modificaron configuraciones, bases de datos ni políticas y no hubo commit/push. SES-008 queda excluido.