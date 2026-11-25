# Superadministración de ASEISA

## Estado y alcance

Implementación local en ASEISA.API, Seguridad.API, ASEISA.Web y el proyecto autoritativo ASEISA.API/BD. No se ha publicado SQL, modificado datos, creado la primera cuenta ni enviado correos. No se implementó SES-008 ni un catálogo de permisos de módulos empresariales.

Las tres aplicaciones y los dos ejecutores de pruebas compilan. Se ejecutaron 90 casos aislados de ASEISA y 17 JavaScript; no equivalen a pruebas integrales. El ejecutor Seguridad sólo se compila por su bloqueo previo de Smart App Control. No se modificaron políticas ni se intentó eludir ese bloqueo.

## Modelo de identidad y dependencias

USUARIO.id_empresa pasa a nullable y se agrega es_plataforma BIT NOT NULL DEFAULT 0. CK_USUARIO_Ambito exige empresa para identidades ordinarias y empresa nula para plataforma. Los usuarios existentes conservan su empresa, roles y versión; no se trasladan ni convierten cuentas automáticamente. No se crean empresas ficticias ni se utiliza cero como empresa de plataforma.

El rol Superadministrador se resuelve por nombre en el catálogo; sólo la utilidad manual puede crear la primera cuenta y el rol si falta. Deben existir los estados Activo/Inactivo y un catálogo no ambiguo. Las cuentas de plataforma sólo pueden tener ese rol, incluso considerando asignaciones ordinarias inactivas. Triggers protegen asignaciones y cambios de nombre/estado del rol. No existe interfaz ni endpoint para crear superadministradores adicionales o desactivar la propia cuenta de plataforma.

Se revisaron USUARIO, ROL_USUARIO, ROL, EMPRESA, BITACORA, SOLICITUD_CONTRASENA y CORREO_ENLACE, DA/Flujo de contexto en ambas API, login, JWT, sesión HTTP, cookie y consumidores administrativos. Las tablas comerciales continúan sin cambios. Las consultas administrativas ordinarias siguen utilizando empresa obligatoria y sus catálogos excluyen Superadministrador.

## Autenticación, autorización y sesiones

Se conserva firma, algoritmo permitido, emisor, audiencia, expiración y VersionCredencial. Un JWT de plataforma contiene Ambito=Plataforma y NO contiene Empresa. El ámbito del JWT no otorga permisos: SQL vuelve a comprobar identidad aprobada/activa y rol exclusivo. Una identidad ordinaria sin empresa, plataforma con empresa, roles mezclados o versión anterior se rechaza. Se reconstruyen los claims sin conservar roles antiguos y no se agrega caché entre solicitudes.

ObtenerContextoUsuarioAutorizado acepta empresa nula sólo con una cuenta de plataforma vigente. ObtenerUsuario y ObtenerCandidatoAutenticacion admiten la identidad de plataforma a través de UsuarioVigente/EsSuperadministradorActivo. El contrato con hash sigue siendo interno. /api/Sesion devuelve IdEmpresa nullable para plataforma; Web reconstruye cookie y ámbito, mantiene 401 frente a revocación y 503 frente a indisponibilidad sin autorizar con claims antiguos.

Los nuevos controladores /api/Plataforma de ambas API exigen Superadministrador y vuelven a comprobarlo en SQL. Un middleware impide que una identidad de plataforma utilice otros endpoints empresariales, salvo /api/Sesion. Web bloquea sus páginas empresariales y dirige el login y /Index al panel de plataforma. El menú ordinario no se rediseña: sólo se sustituye para el superadministrador.

La protección nueva no equivale a una auditoría general de endpoints anónimos preexistentes de otros módulos: ese endurecimiento y SES-008 siguen fuera de este alcance. No se agregó una política global que cambie los contratos públicos existentes.

## Empresas y administradores

Panel: total de empresas, activas, inactivas, administradores registrados y empresas sin administradores habilitados. Listados con búsqueda/filtro, detalle, alta, edición y activación/desactivación. No hay eliminación física. Al desactivar una empresa, UsuarioVigente impide login y la siguiente revalidación de JWT/cookie revoca el acceso de sus usuarios.

La empresa objetivo siempre se valida en SQL. La cédula jurídica se normaliza sin guiones y su duplicidad se comprueba con bloqueos de rango; nombres, correo, teléfono, dirección y moneda respetan los tamaños del modelo. Se reutilizan las reglas de usuarios y las máscaras para formularios equivalentes.

La sección de administradores sólo administra usuarios con asignación Administrador activa y catálogo activo. Incluye cuentas con usuario inactivo para poder habilitarlas. El alta selecciona una empresa activa y resuelve en servidor el rol Administrador y estado Activo; el navegador no elige otro rol ni una contraseña. Se reutiliza CredencialesFlujo y su transacción de alta, solicitud y outbox. Activación/desactivación y edición nunca cambian la empresa ni regeneran el nombre de usuario.

ProtegerAdministradorEmpresa toma un applock transaccional por empresa. CambiarEstadoUsuario, GestionarRolUsuario y GestionarAdministradorPlataforma lo toman antes de modificar identidades. Los triggers de USUARIO, ROL_USUARIO y ROL vuelven a verificar el último administrador habilitado de empresas activas; incluyen protección para sentencias directas y multirregistro. Retirar un rol no modifica los demás. La restricción devuelve error funcional 51303. Los bloqueos pueden producir espera o rollback por deadlock para operaciones directas que no respeten el orden; no se reintenta ciegamente una escritura.

Una empresa nueva puede empezar sin administradores para permitir incorporarle el primero. No se obliga a inventar una cuenta al crearla. Reactivar una empresa sin administradores no otorga acceso: el panel lo señala y permite incorporar uno. Una empresa activa que ya tiene un administrador habilitado no puede perder el último mediante las operaciones protegidas. Las empresas inactivas conservan cuentas y relaciones.

## Auditoría

BITACORA.id_empresa es nullable. Los eventos de plataforma usan NULL y guardan la empresa afectada en id_empresa_objetivo, con FK; las consultas empresariales por id_empresa no los incluyen. ValidarAmbitoBitacora impide mezclar ámbitos del actor. La pantalla de plataforma consulta sólo eventos de plataforma y muestra los últimos 200, con IDs, acción y fecha. No devuelve valores de contraseñas, hashes, tokens, sobres cifrados ni datos de operaciones empresariales.

Se conserva la auditoría de alta, invitación, login, contraseña, roles y estados. Las operaciones nuevas de empresas y administradores registran al actor de plataforma. No se duplicó la auditoría del registro público.

## Invitaciones, recuperación y credenciales

SOLICITUD_CONTRASENA admite empresa nula para cuentas de plataforma. Emisión, reserva del correo y consumo comparan empresas nulas correctamente, revalidan UsuarioVigente, estado, correo y versión. La recuperación pública permite una cuenta de plataforma elegible, con la respuesta genérica habitual. El restablecimiento administrativo desde plataforma sólo admite administradores de la empresa objetivo; la inicialización es la única emisión administrativa para la propia cuenta nueva de plataforma.

Se conservan vencimiento de 30 minutos, un solo uso, hash del token, sobre AES-GCM, cola SQL, reservas/reintentos y consumo transaccional. Cambiar la contraseña incrementa la versión también para plataforma. La credencial inicial es aleatoria, desconocida y hasheada mediante ContrasenasSeguras.Crear; no se entrega ni registra. El primer acceso requiere establecer una contraseña mediante el enlace. La inicialización y el alta no envían SMTP en la petición.

## Configuración

No se agregaron secretos nuevos ni archivos de configuración permanentes.

- Ambas API: conexión al destino autorizado y JWT compatibles. Usar el almacén de secretos/variables del entorno, sin copiar contraseñas al repositorio.
- ASEISA.API: Servicios:SeguridadUrl con URL HTTPS de Seguridad.API incluyendo /api. Se reenvía el Bearer; TLS se valida y el cliente interno no sigue redirecciones.
- Seguridad.API: CorreoEnlaces:ClaveCifrado (Base64 de 32 bytes, estable y respaldada), App:BaseUrl (URL HTTPS completa de Web /Seguridad/Cambiar_Contrasena), Email:Remitente, Email:Nombre y Email:Password en secretos. CorreoEnlaces:EnviosHabilitados sigue deshabilitado por defecto y sólo debe habilitarse con autorización de destinatarios/entorno. Remitente Gmail con contraseña de aplicación según el componente SMTP actual.
- Web: ApiEndPoints:UrlBase y ApiEndPointsSeguridad existentes. Certificados locales confiables; no omitir la validación TLS.
- Verificar permisos EXECUTE de los procedimientos nuevos y permisos requeridos por el repositorio actual. La identidad usada sólo para la inicialización debe ser operativa/manual y restringirse después de utilizarla; no otorgar permisos de inicialización a consumidores Web.

## Inicializar la primera cuenta (NO ejecutado)

Requiere antes: esquema instalado bajo autorización separada, preflight sin cuentas heredadas de superadministración, estados requeridos, configuración anterior y conexión efectiva confirmada. Hacer respaldo y revisar el plan de despliegue antes. La utility no publica tablas ni instala catálogos de estados.

Desde la raíz del repositorio, en PowerShell, con configuración Development y secretos del proyecto Seguridad.API apuntando exclusivamente al destino aprobado:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet build .\Seguridad\Seguridad.API\API\API.csproj -c Debug
dotnet run --project .\Seguridad\Seguridad.API\API\API.csproj --no-build --no-restore --no-launch-profile -- --inicializar-superadministrador
```

Si Windows bloquea cualquier ensamblado, detenerse y usar un entorno autorizado; no cambiar políticas ni generar ejecutables alternativos. Estos comandos son instrucciones para un operador, no se ejecutaron en esta implementación.

La utilidad pide destino exacto `instancia/base`, nombre, primer apellido, segundo apellido opcional, correo y cédula física; valida con PersonaPlataforma. El operador escribe INICIALIZAR para confirmar. La DA consulta SERVERPROPERTY('ServerName') y DB_NAME() y exige coincidencia con el destino confirmado antes de abrir la transacción de escritura. No debe confirmarse una base por suposición ni reutilizar valores de otro ambiente.

Se crea una cuenta aprobada, activa, sin empresa y con credencial aleatoria desconocida; se asigna exclusivamente Superadministrador, se registra el evento y se persiste su invitación en la misma transacción externa. Un applock global y comprobación de todas las cuentas/asignaciones de superadministración impiden inicializaciones concurrentes o repetidas, incluso con cuentas inactivas. Si falla la invitación, toda la creación revierte. No se inicia servidor HTTP ni trabajador SMTP en este modo.

Después de la inicialización, arrancar Seguridad.API normalmente en el destino aprobado para que el trabajador procese la cola, sólo cuando el envío esté autorizado. El destinatario establece su contraseña. No volver a ejecutar la inicialización como mecanismo de recuperación; utilizar la recuperación existente.

## SQL incremental y compatibilidad

Única fuente: ASEISA.API/BD. Seguridad.API/BD permanece con 29 enlaces None; no produce DACPAC.

Cambios de tablas: USUARIO (empresa nullable, es_plataforma y CHECK), BITACORA (empresa nullable y empresa objetivo), SOLICITUD_CONTRASENA (empresa nullable). No se reescriben asignaciones existentes ni se altera la tabla EMPRESA.

Funciones nuevas: EsSuperadministradorActivo y UsuarioVigente.
Procedimientos nuevos: InicializarSuperadministrador, ConsultarPlataforma, GuardarEmpresaPlataforma, CambiarEstadoEmpresaPlataforma, GestionarAdministradorPlataforma, ValidarAdministradorPlataforma y ProtegerAdministradorEmpresa.
Triggers nuevos: ProtegerUsuarioPlataforma, ProtegerRolesUsuario, ProtegerRolPlataforma y ValidarAmbitoBitacora.
Procedimientos adaptados: ObtenerContextoUsuarioAutorizado, ObtenerUsuario, ObtenerCandidatoAutenticacion, RegistroUsuario, CrearSolicitudContrasena, ConsumirSolicitudContrasena, TomarCorreoEnlace, CambiarEstadoUsuario y GestionarRolUsuario. Se conserva la asignación de @estado_nuevo de CambiarEstadoUsuario.

Compilación temporal real de SQL Server 2022, sin modificar permanentemente DSP:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\ASEISA.API\BD\BD.sqlproj /t:Rebuild /p:Configuration=Debug /p:VisualStudioVersion=18.0 /p:DSP=Microsoft.Data.Tools.Schema.Sql.Sql160DatabaseSchemaProvider /p:OutputPath=C:\Temp\ASEISA-Superadministracion-Sql160\
```

Ajustar sólo la ruta de MSBuild si la instalación autorizada de Visual Studio es otra. No ejecutar Publish. El proyecto conserva su plataforma predeterminada Sql170 y la referencia a master correspondiente; no utilizar ese DACPAC predeterminado contra SQL Server 2022. La compilación de esta entrega se realizó realmente con SSDT/Sql160 y se empaquetó PostDeploy sin ejecutarlo.

Antes de cualquier publicación:
1. Ejecutar bajo autorización el script de sólo lectura scripts/Verificar-Previa-Superadministracion.sql. Si existen asignaciones heredadas de Superadministrador con empresa, detener la migración de esas cuentas: requieren decisión y plan manual; no se convierten automáticamente. La inicialización se negará a crear otra cuenta mientras existan.
2. Revisar catálogos duplicados, datos inconsistentes y cédulas de empresas duplicadas. No corregir datos reales sin autorización.
3. Generar DeployReport/script del esquema completo, con compatibilidad y protección de pérdida de datos activas. Revisar también los cambios SQL previos del repositorio; no publicar sólo un subconjunto que omita dependencias.
4. Revisar ALTER de columnas, CHECK/FK y triggers, permisos, PostDeploy, respaldo y reversión. Tras crear cuentas con empresa nula, no se puede volver ciegamente a NOT NULL: una reversión requiere un plan de datos aprobado.
5. Publicar sólo en una base aislada bajo autorización separada y desplegar ambas API/Web coordinadamente. JWT empresariales existentes conservan sus reglas; cuentas de plataforma nuevas sólo funcionan con todo este esquema instalado.

## Validaciones y pruebas pendientes

Realizado: compilación de las tres aplicaciones sin errores (con advertencias previas), compilación de ambos ejecutores, 90 pruebas aisladas ASEISA, 17 JavaScript y comprobación de 29 enlaces. Casos nuevos: JWT de plataforma sin empresa, exclusividad, rechazo de identidades ordinarias sin empresa, empresa prohibida en plataforma, rol revocado, versión anterior y cookies de plataforma. Los consumidores protegidos no ejecutan la operación ante autorización denegada.

Pendiente de entorno autorizado y datos sintéticos:
- Inicialización única y dos inicializaciones concurrentes; fallo entre creación/invitación debe revertir todo. Cuenta inactiva previa también debe bloquear reinicialización.
- Verificar datos y CHECK, exclusividad de roles y triggers con sentencias simples/multirregistro.
- Registrar empresa, alta de primer administrador y posteriores, duplicados y validación de objetivos ajenos.
- Dos desactivaciones o retiradas simultáneas del rol Administrador: al menos uno debe permanecer habilitado en empresa activa. Probar operaciones ordinarias y de plataforma, cambios de catálogo y rollback por deadlock.
- Login inicial sólo tras establecer contraseña, JWT/cookie de plataforma, sesión revocada tras cambio, enlaces concurrentes/expirados/reemplazados y reinicio de la cola. SMTP únicamente con destinatarios ficticios autorizados.
- Desactivación de empresa: usuarios afectados reciben 401 en la siguiente revalidación; otra empresa continúa operando. Reactivación no altera roles ni contraseñas.
- Rechazo de usuarios ordinarios en endpoints y páginas de plataforma; rechazo de plataforma en operaciones empresariales autenticadas; comprobar que auditorías de plataforma no aparezcan en consultas empresariales.
- Recorrido visual de escritorio/móvil, filtros, formularios, modales, errores y accesos directos. No se inició Web contra una conexión desconocida para probar el diseño.

No se afirma validación integral de SQL, API, concurrencia o SMTP. La decisión sobre posibles cuentas heredadas de superadministración queda pendiente del preflight autorizado; el resto de componentes está preparado sin migrar datos.