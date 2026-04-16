# Etapa 3: Clarificación

> Según [Spec-Kit de GitHub](https://github.com/github/spec-kit): la clarificación
> (`/speckit.clarify`) resuelve ambigüedades ANTES de la planificación técnica,
> reduciendo retrabajos posteriores. "La IA te hace preguntas sobre lo que olvidaste.
> Te fuerza a pensar para no dejar agujeros."
>
> Referencia: [spec-driven.md](https://github.com/github/spec-kit/blob/main/spec-driven.md)

---

## 1. Preguntas resueltas sobre la arquitectura

### P: Por qué Blazor Server y no WebAssembly (WASM)?
**R:** Blazor Server ejecuta el código C# en el servidor y actualiza el navegador vía SignalR (WebSocket). El estudiante no necesita entender compilación a WebAssembly ni descargas de DLLs al cliente. Además, ProtectedSessionStorage (sesión encriptada) solo funciona en Server porque depende de Data Protection API del servidor. Con WASM, todo el código corre en el navegador y la sesión sería manipulable.

### P: Por qué no usar Entity Framework si es un proyecto .NET?
**R:** Porque el frontend no accede a la BD. Todo va por HTTP a la API REST genérica (ApiGenericaCsharp). Si usamos Entity Framework, el estudiante aprende ORM pero no aprende a consumir APIs, que es lo que necesita en el mundo real (microservicios, frontends desacoplados). Además, el mismo ejercicio se replica en Flask, Java y PHP — todos consumen la misma API.

### P: Por qué ProtectedSessionStorage y no localStorage?
**R:** ProtectedSessionStorage encripta los datos con Data Protection API antes de guardarlos en el sessionStorage del navegador. Si un atacante inspecciona el almacenamiento del navegador, solo ve texto encriptado. Con localStorage (sin encriptar), el token JWT y los roles serían visibles y manipulables desde la consola del navegador.

### P: Por qué OnAfterRenderAsync y no OnInitializedAsync para restaurar la sesión?
**R:** ProtectedSessionStorage requiere JavaScript (interop) para leer del sessionStorage del navegador. Blazor Server no tiene acceso a JavaScript durante OnInitializedAsync porque el componente aún no se ha renderizado. OnAfterRenderAsync se ejecuta DESPUÉS del primer render, cuando la conexión SignalR ya está establecida y JavaScript está disponible. Intentar leer ProtectedSessionStorage en OnInitializedAsync lanza una excepción.

### P: Por qué LocationChanged para verificar acceso y no un middleware?
**R:** Blazor Server no tiene middleware HTTP como Flask o ASP.NET MVC. Las páginas Razor se renderizan dentro de un layout (MainLayout), no como peticiones HTTP separadas. La navegación entre páginas ocurre por SignalR sin nuevas peticiones HTTP. Por eso MainLayout se suscribe al evento `NavigationManager.LocationChanged` y llama `VerificarAcceso()` en cada cambio de URL. Implementa `IDisposable` para limpiar la suscripción al destruir el componente.

### P: Qué pasa si la API no está corriendo?
**R:** Los servicios (`ApiService`, `AuthService`) tienen `try/catch` que capturan excepciones HTTP. Si la API no responde, se muestra un mensaje de error en pantalla sin que la app crashee. El login falla con "Error de conexión".

## 2. Preguntas resueltas sobre seguridad

### P: Si la sesión está en el navegador, el usuario puede manipularla?
**R:** No. ProtectedSessionStorage usa Data Protection API para encriptar los datos con claves que solo el servidor conoce. Si el usuario modifica un byte del valor encriptado, la desencriptación falla y la sesión se invalida. No puede inyectar roles ni rutas.

### P: Por qué guardar el JWT si Blazor ya tiene ProtectedSessionStorage?
**R:** Son dos capas diferentes:
- La **sesión** (ProtectedSessionStorage) protege las páginas del frontend (MainLayout verifica antes de cada navegación)
- El **JWT** protege los datos de la API (si tiene `[Authorize]`, rechaza sin token)

Sin JWT, alguien puede abrir Postman y hacer `DELETE /api/usuario/email/admin@test.com` sin haber hecho login.

### P: Qué pasa si el JWT expira durante la sesión?
**R:** La API responde 401. ApiService no maneja esto automáticamente — el usuario ve un error y debe hacer login de nuevo. En un sistema de producción se implementaría refresh token, pero está fuera del alcance del tutorial.

### P: Las contraseñas viajan en texto plano por HTTP?
**R:** Sí, en desarrollo local (http://localhost). En producción se usa HTTPS. La contraseña viaja en el body del POST (no en la URL) y solo el servidor la lee. La API inmediatamente la compara con el hash BCrypt — nunca la guarda en texto plano.

## 3. Preguntas resueltas sobre el CRUD

### P: Por qué ConsultasController en vez de los 5 GETs?
**R:** Eficiencia. Los 5 GETs traen tablas COMPLETAS y filtran en C#. ConsultasController ejecuta 1 SQL con JOINs y WHERE en la BD — solo viajan las filas del usuario. Si hay 1000 usuarios, los 5 GETs traen los 1000 para buscar 1. El SQL solo trae las filas del usuario logueado.

### P: Cómo sabe el formulario qué tipo de input usar para cada campo?
**R:** Las páginas Razor tienen los tipos hardcodeados por tabla (ej: `type="email"` para email, `type="number"` para precio). En el generador genérico (`frontGenericoFlask`), se descubre el tipo vía `/api/estructuras/basedatos` y se mapea: `varchar -> text`, `integer -> number`, `boolean -> checkbox`, etc.

### P: Qué pasa con los campos FK en los formularios?
**R:** Se renderizan como `<select>` cargados desde la API. Ejemplo: en factura, el campo `fkcodvendedor` se muestra como un dropdown con todos los vendedores. La página Razor carga la lista con `ApiService.ListarAsync("vendedor")` y la itera con `@foreach`.

## 4. Preguntas resueltas sobre el ciclo de vida Blazor

### P: Por qué @rendermode="InteractiveServer" en App.razor y no en cada página?
**R:** Si se pone `@rendermode` en cada página, las páginas que no lo tengan se renderizan en modo estático (Static SSR) y no pueden usar eventos (`@onclick`), ProtectedSessionStorage ni SignalR. Ponerlo en `<Routes>` dentro de App.razor hace que TODAS las páginas sean interactivas por defecto. Esto simplifica el tutorial — el estudiante no tiene que recordar agregar `@rendermode` en cada `.razor`.

### P: Por qué MainLayout implementa IDisposable?
**R:** MainLayout se suscribe al evento `_nav.LocationChanged` en `OnInitialized()`. Si el componente se destruye sin desuscribirse, el evento sigue referenciando al componente destruido (memory leak). `IDisposable.Dispose()` llama `_nav.LocationChanged -= OnLocationChanged` para limpiar. Blazor llama `Dispose()` automáticamente cuando el componente se destruye.

### P: Por qué el spinner en MainLayout mientras restaura sesión?
**R:** Sin el spinner, el usuario ve un flash del contenido de la página antes de ser redirigido a `/login`. Esto ocurre porque `OnAfterRenderAsync` se ejecuta DESPUÉS del primer render. La variable `_cargando = true` muestra un spinner en vez del contenido, y se cambia a `false` después de `Restaurar()`. Esto evita la experiencia visual de "parpadeo".

### P: Por qué AddScoped y no AddSingleton para los servicios?
**R:** `AddScoped` crea una instancia por circuito SignalR (equivalente a "por sesión de usuario"). `AddSingleton` crearía UNA instancia compartida entre todos los usuarios — el token JWT de un usuario se compartiría con otro. Con `AddScoped`, cada usuario tiene su propia instancia de AuthService y ApiService con su propio token.

## 5. Preguntas resueltas sobre el modelo de datos

### P: Por qué ACID y no BASE (eventual consistency)?
**R:** Este es un sistema transaccional (facturas, contraseñas, permisos). ACID garantiza que una factura se crea completa o no se crea. BASE es para sistemas distribuidos de alta escala (redes sociales, IoT) donde se acepta inconsistencia temporal. Un sistema de facturación NO puede tener inconsistencia temporal — el dinero no puede "eventualmente" cuadrar.

### P: Por qué usuario tiene email como PK y no un id numérico?
**R:** Porque el email es único y natural — es lo que el usuario escribe para hacer login. Usar un id numérico obligaría a hacer un JOIN extra para buscar por email. Además, simplifica las FKs en rol_usuario (`fkemail` es legible).

### P: Por qué productosporfactura tiene su propio campo "precio"?
**R:** Porque el precio del producto puede cambiar en el futuro. Si solo guardamos el FK al producto, al consultar una factura vieja mostraría el precio actual, no el que se cobró. Guardar el precio al momento de la venta es un patrón estándar de facturación.

### P: Por qué cliente tiene FK a persona Y a empresa?
**R:** Un cliente puede ser persona natural (fkcodpersona) O persona jurídica (fkcodempresa). `fkcodempresa` es nullable — si es persona natural, no tiene empresa. Esto cumple 3FN porque el nombre de la persona no se duplica en la tabla cliente.

### P: Por qué las tablas de seguridad (rol, ruta, rutarol) están separadas de las de negocio?
**R:** Principio de Single Responsibility (SOLID - S). Las tablas de negocio (producto, factura) manejan datos del dominio. Las tablas de seguridad (rol, ruta) manejan permisos. Si un día se cambia el sistema de permisos, no se tocan las tablas de negocio.

## 6. Preguntas resueltas sobre el trabajo colaborativo

### P: Qué pasa si dos estudiantes crean la misma página Razor?
**R:** Conflicto de merge en el archivo `.razor`. Se resuelve en la rama feature/ antes de mergear a main. Por eso cada estudiante tiene tablas asignadas (Paso0).

### P: Quién resuelve conflictos de merge?
**R:** El Estudiante 1 (admin/scrum master). Los otros hacen `git fetch origin` y mergean main a su rama antes de hacer PR.

## 7. Decisiones de diseño documentadas

| Decisión | Alternativa descartada | Razón |
|----------|----------------------|-------|
| Blazor Server (componentes server-side) | Blazor WASM (client-side) | Código C# en servidor, sesión segura, sin descarga de DLLs |
| Razor (componentes .razor) | React/Vue (SPA) | Mismo lenguaje C# en todo el proyecto |
| HttpClient (inyectado por DI) | RestSharp, Refit | Incluido en .NET, sin dependencias extra |
| ProtectedSessionStorage (encriptada) | localStorage / cookie | Encriptación con Data Protection, no manipulable |
| ConsultasController (1 SQL) | 5 GETs separados | Eficiencia, menos tráfico de red |
| Bootstrap incluido en wwwroot | CDN | Ya viene con la plantilla Blazor |
| Descubrimiento dinámico FK/PK | Hardcodear nombres | Funciona con cualquier BD |
| LocationChanged + VerificarAcceso | Middleware HTTP | Blazor navega por SignalR, no HTTP |
| OnAfterRenderAsync | OnInitializedAsync | ProtectedSessionStorage necesita JavaScript (post-render) |
| PostgreSQL | MySQL, SQLite | ACID completo, compatible con SqlServer |
| 3FN (normalización) | Desnormalizar para rendimiento | Integridad sobre velocidad en sistema transaccional |
| IDisposable en MainLayout | No limpiar suscripción | Evitar memory leaks del evento LocationChanged |

## 8. Principios de diseño aplicados (resumen)

| Principio | Categoría | Dónde aplica | Referencia |
|-----------|-----------|-------------|------------|
| SOLID - S (Single Responsibility) | OOP | Cada servicio/página tiene 1 responsabilidad | 01_constitucion.md |
| SOLID - O (Open/Closed) | OOP | Agregar CRUD = archivos nuevos, no modificar existentes | 01_constitucion.md |
| SOLID - D (Dependency Inversion) | OOP | Páginas dependen de ApiService, no de HttpClient directo | 01_constitucion.md |
| ACID | BD | PostgreSQL garantiza transacciones íntegras | 01_constitucion.md |
| 3FN (Normalización) | BD | Sin datos redundantes, FKs para relaciones | 02_especificacion.md |
| Componentes | Arquitectura | Services/ + Pages/ + Layout/ | 01_constitucion.md |
| Facade | Patrón | ApiService oculta complejidad HTTP | 01_constitucion.md |
| Strategy (fallback) | Patrón | ConsultasController o GETs separados según disponibilidad | 01_constitucion.md |
| Observer | Patrón | LocationChanged notifica a MainLayout en cada navegación | MainLayout.razor |
| DI (Dependency Injection) | Patrón | @inject inyecta servicios en componentes Razor | Program.cs |
