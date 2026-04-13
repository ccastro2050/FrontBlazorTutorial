# Paso 12: Login y Control de Acceso - Blazor Server

## Los 3 conceptos clave de seguridad

1. **Autenticacion** → ¿Quien eres? (login, BCrypt, JWT)
2. **Autorizacion** → ¿Que puedes hacer? (roles, rutas, permisos)
3. **Encriptacion** → ¿Como se protege? (BCrypt para contrasenas, Data Protection para sesion, HTTPS para transporte)

## Que se implemento

- **Login** con email y contrasena encriptada (BCrypt)
- **JWT** (JSON Web Token): token enviado en cada request para proteger la API
- **Control de acceso por roles**: cada usuario tiene roles, cada rol tiene rutas
- **Cambio de contrasena** con validacion de seguridad (minimo 6 chars, mayuscula, numero)
- **Recuperar contrasena** con envio de temporal por correo SMTP (Gmail)
- **Sesion persistente** con ProtectedSessionStorage (encriptada, sobrevive F5)
- **Descubrimiento dinamico** de PKs y FKs (compatible Postgres + SqlServer)
- **3 capas de seguridad**: BCrypt (BD) + JWT (API) + Sesion (frontend)

---

## Que se crea, que se modifica, que se agrega

### Archivos que se CREAN (nuevos)

| Archivo | Para que |
|---------|---------|
| `Services/AuthService.cs` | Toda la logica: login, roles, rutas, cambiar contrasena, descubrimiento dinamico |
| `Components/Pages/Login.razor` | Formulario de login (email + contrasena) |
| `Components/Pages/CambiarContrasena.razor` | Formulario para cambiar contrasena |
| `Components/Pages/RecuperarContrasena.razor` | Recuperar contrasena olvidada (envia email SMTP) |
| `Components/Pages/SinAcceso.razor` | Pagina error 403 (no tiene permiso para esa ruta) |
| `Components/Layout/EmptyLayout.razor` | Layout vacio para login (sin sidebar ni menu) |

### Archivos que se MODIFICAN (ya existian)

| Archivo | Que se agrega | Para que |
|---------|---------------|---------|
| `Program.cs` | `builder.Services.AddScoped<AuthService>();` | Registrar el servicio en Blazor (Dependency Injection) |
| `Components/App.razor` | `@rendermode="InteractiveServer"` en `<Routes>` | Hacer el layout interactivo (necesario para ProtectedSessionStorage) |
| `Components/Layout/MainLayout.razor` | `@inject AuthService`, `OnAfterRenderAsync`, boton logout | Verificar sesion al cargar, redirigir a /login, mostrar usuario y boton cerrar sesion |
| `appsettings.json` | Seccion `"Smtp"` con Host, Port, User, Pass, From | Configurar correo Gmail para recuperar contrasena |

### Tablas que se necesitan en la BD (5)

| Tabla | Para que |
|-------|---------|
| `usuario` | Almacenar credenciales (email + contrasena BCrypt) |
| `rol` | Definir tipos de usuario (Administrador, Vendedor, etc) |
| `rol_usuario` | Asignar roles a usuarios (un usuario puede tener varios roles) |
| `ruta` | Registrar las paginas del sistema (/producto, /cliente, etc) |
| `rutarol` | Definir que paginas puede acceder cada rol |

---

## Tablas necesarias en la base de datos

Son las mismas 5 tablas del tutorial Flask:

```sql
CREATE TABLE usuario (
    email VARCHAR(200) PRIMARY KEY,
    contrasena VARCHAR(200) NOT NULL
);

CREATE TABLE rol (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL
);

CREATE TABLE rol_usuario (
    id SERIAL PRIMARY KEY,
    fkemail VARCHAR(200) REFERENCES usuario(email),
    fkidrol INTEGER REFERENCES rol(id)
);

CREATE TABLE ruta (
    id SERIAL PRIMARY KEY,
    ruta VARCHAR(200) NOT NULL,
    descripcion TEXT DEFAULT ''
);

CREATE TABLE rutarol (
    id SERIAL PRIMARY KEY,
    fkidrol INTEGER REFERENCES rol(id),
    fkidruta INTEGER REFERENCES ruta(id)
);
```

---

## Diferencias con Flask

| Aspecto | Flask | Blazor |
|---------|-------|--------|
| Sesion | Cookie encriptada (server-side) | ProtectedSessionStorage (browser-side) |
| Middleware | `@app.before_request` intercepta cada request | `MainLayout.OnAfterRenderAsync` verifica al cargar |
| Templates | Jinja2 (HTML con `{{ }}`) | Razor components (HTML con `@`) |
| Interactividad | JavaScript + POST forms | SignalR (todo en C# sin JS) |
| Layout login | `{% extends base.html %}` | `@layout EmptyLayout` |
| Verificacion acceso | Middleware automatico en cada request | `TieneAcceso()` llamado en MainLayout |

---

## Flujo de autenticacion (paso a paso con diagrama)

```
Usuario abre http://localhost:5003
         |
         v
MainLayout.razor (OnAfterRenderAsync)
         |
         +-- _auth.Restaurar() -> intenta leer sesion del navegador
         |     |
         |     +-- ProtectedSessionStorage.GetAsync("usuario")
         |           |
         |           +-- SI hay sesion -> mostrar pagina + nombre + boton logout
         |           |
         |           +-- NO hay sesion -> nav.NavigateTo("/login")
         |
         v
Login.razor
         |
         +-- Usuario escribe email + contrasena
         |
         +-- Click "Iniciar sesion" -> DoLogin()
         |     |
         |     v
         |   AuthService.Login(email, contrasena)
         |     |
         |     +-- PASO 1: PrecargarEstructura() + PostJson("autenticacion/token")
         |     |     |                                    |
         |     |     |                                    v
         |     |     |                  API C#: POST /api/autenticacion/token
         |     |     |                                    |
         |     |     |                  BCrypt.Verify(contrasena, hashBD)
         |     |     |                                    |
         |     |     |                  <- OK (token JWT) o ERROR (401)
         |     |     |
         |     |     +-- Cachea PKs y FKs de TODAS las tablas en UNA llamada
         |     |
         |     +-- PASO 2: CargarDatosUsuario() + CargarRoles()  <- EN PARALELO
         |     |     |                              |
         |     |     |                              +-- ObtenerFK("rol_usuario","usuario") -> "fkemail"
         |     |     |                              +-- ObtenerFK("rol_usuario","rol") -> "fkidrol"
         |     |     |                              +-- GET /api/rol_usuario + GET /api/rol
         |     |     |                              +-- Filtrar: roles de este email
         |     |     |
         |     |     +-- GET /api/usuario -> buscar nombre del usuario
         |     |
         |     +-- PASO 3: CargarRutasPermitidas()
         |     |     |
         |     |     +-- GET /api/rutarol + GET /api/rol + GET /api/ruta  <- 3 EN PARALELO
         |     |     +-- Filtrar: rutas asignadas a los roles del usuario
         |     |
         |     +-- PASO 4: Guardar en ProtectedSessionStorage
         |           |
         |           +-- session.SetAsync("usuario", "ccastro@correo.itm.edu.co")
         |           +-- session.SetAsync("roles", "Administrador,Profesor,...")
         |           +-- session.SetAsync("rutas_permitidas", "/facultad,/asignatura,...")
         |
         v
nav.NavigateTo("/") -> vuelve a MainLayout -> _auth.Restaurar() -> AHORA SI hay sesion
```

## Como funciona la proteccion de rutas

```csharp
// MainLayout.razor - OnAfterRenderAsync
// Se ejecuta DESPUES del primer render (necesario para ProtectedSessionStorage)

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await _auth.Restaurar();        // Lee sesion del navegador
        
        if (!_auth.EstaAutenticado)      // No hay sesion?
            _nav.NavigateTo("/login");   // -> Ir a login
            
        if (_auth.DebeCambiarContrasena) // Debe cambiar?
            _nav.NavigateTo("/cambiar-contrasena"); // -> Forzar cambio
            
        StateHasChanged();               // Re-renderizar con datos de auth
    }
}
```

**Por que OnAfterRenderAsync y no OnInitialized?**
Porque ProtectedSessionStorage necesita JavaScript del navegador, y JavaScript
solo esta disponible DESPUES del primer render. Si se llama antes, da error.

---

## JWT: que es, para que y como se usa

### Que es JWT?

JWT (JSON Web Token) es un token que la API genera al hacer login exitoso.
Es un string largo con 3 partes separadas por puntos: Header.Payload.Signature

```
eyJhbGciOiJIUzI1NiIs...   <-- se ve asi en Session Storage como "token"
```

### Para que sirve?

Si la API tiene `[Authorize]` en un controller, SOLO acepta peticiones
que traigan el token en el header HTTP:

```
GET /api/producto HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

Sin ese header -> la API responde 401 Unauthorized (rechaza la peticion).

### Como funciona en este proyecto?

```
1. Login: POST /api/autenticacion/token
   -> API verifica BCrypt -> genera JWT -> devuelve en respuesta

2. AuthService captura el token:
   -> Token = respuesta["token"]
   -> Guarda en ProtectedSessionStorage("token", Token)

3. Cada peticion de ApiService:
   -> AgregarTokenJwt()  <-- agrega header Authorization
   -> GET /api/producto  (con "Bearer eyJhbG..." en el header)

4. API recibe el header:
   -> Verifica firma JWT con clave secreta
   -> Valido y no expiro -> permite la operacion
   -> Invalido o expiro -> 401 Unauthorized
```

### Donde esta la clave secreta?

En la API C# (`appsettings.json`):
```json
"Jwt": {
    "Key": "MySuperSecretKey1234567890...",
    "DuracionMinutos": 60
}
```
El frontend NUNCA conoce la clave. Solo la API puede generar y verificar tokens.

### JWT vs Sesion: que protege que?

| Concepto | Donde se verifica | Que protege |
|----------|------------------|-------------|
| Sesion (ProtectedSessionStorage) | Frontend (Blazor) | Acceso a las PAGINAS |
| JWT (Authorization header) | Backend (API C#) | Acceso a los DATOS |

Ambos son necesarios:
- Sin sesion -> usuario ve login (no puede navegar)
- Sin JWT -> usuario ve la pagina pero no puede cargar datos (401)
- Con ambos -> usuario ve la pagina Y puede operar con datos

### Que valor agrega JWT? Por que es necesario?

Sin JWT, la API esta **completamente abierta**. Cualquier persona que conozca
la URL puede consumirla directamente desde Postman, curl o su propio codigo:

```
SIN JWT (API abierta - INSEGURO):

  Cualquiera con Postman:
    GET http://localhost:5035/api/usuario
    -> Ve TODOS los usuarios con sus hashes BCrypt

    DELETE http://localhost:5035/api/usuario/email/admin@mail.com
    -> Borra al administrador

    PUT http://localhost:5035/api/usuario/email/admin@mail.com
    -> Modifica cualquier dato

  La sesion de Blazor NO protege esto — solo protege las paginas del frontend.
  La API sigue abierta aunque el usuario no haya hecho login en Blazor.
```

```
CON JWT (API protegida - SEGURO):

  Postman sin token:
    GET http://localhost:5035/api/usuario
    -> 401 Unauthorized (no puede ver nada)

    DELETE http://localhost:5035/api/usuario/email/admin@mail.com
    -> 401 Unauthorized (no puede borrar nada)

  Postman CON token (obtenido via login):
    GET http://localhost:5035/api/usuario
    Headers: Authorization: Bearer eyJhbG...
    -> 200 OK (datos devueltos)

  Solo quien hizo login y tiene un token valido puede operar.
```

**Resumen**:

| Capa | Que protege | De quien |
|------|------------|----------|
| **Sesion** (Blazor) | Las PAGINAS del frontend | Usuarios no logueados en el navegador |
| **JWT** (API) | Los DATOS del backend | Cualquiera que intente consumir la API directamente |
| **BCrypt** (BD) | Las CONTRASENAS en la base de datos | Alguien que acceda a la BD directamente |

Las 3 capas juntas forman la seguridad completa:
- BCrypt protege la BD (si la hackean, no ven contrasenas)
- JWT protege la API (si conocen la URL, no pueden operar sin token)
- Sesion protege el frontend (si abren el navegador, no ven paginas sin login)

### IMPORTANTE: JWT solo protege si el controller tiene [Authorize]

El JWT existe y se envia, pero **solo funciona** si el controller de la API C#
tiene el atributo `[Authorize]`. Si no lo tiene, el endpoint queda abierto
aunque el token exista.

```
Actualmente en la API generica:

  [ApiController]                        <-- NO tiene [Authorize]
  [Route("api/[controller]")]
  public class EntidadesController       <-- ABIERTO (cualquiera puede consumir)

Para proteger:

  [Authorize]                            <-- AGREGAR ESTA LINEA
  [ApiController]
  [Route("api/[controller]")]
  public class EntidadesController       <-- PROTEGIDO (requiere JWT)
```

### Donde se pone [Authorize] en la API C#?

En los controllers de la API generica. Los archivos estan en:
```
ApiGenericaCsharp/Controllers/
  EntidadesController.cs          <-- CRUD generico (listar, crear, actualizar, eliminar)
  AutenticacionController.cs      <-- Login (este NO debe tener [Authorize])
  EstructurasController.cs        <-- Estructura BD (puede o no tener [Authorize])
  ConsultasController.cs          <-- Consultas SQL
```

### Opciones de proteccion

```csharp
// OPCION 1: Proteger TODO el controller (todos los endpoints)
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EntidadesController : ControllerBase { ... }

// OPCION 2: Proteger solo algunos metodos
[ApiController]
[Route("api/[controller]")]
public class EntidadesController : ControllerBase
{
    [HttpGet]                     // <-- SIN [Authorize] = publico (cualquiera puede listar)
    public async Task Listar() { ... }

    [Authorize]                   // <-- CON [Authorize] = protegido (solo con token)
    [HttpPost]
    public async Task Crear() { ... }

    [Authorize]
    [HttpPut]
    public async Task Actualizar() { ... }

    [Authorize]
    [HttpDelete]
    public async Task Eliminar() { ... }
}

// OPCION 3: Proteger todo EXCEPTO login
// AutenticacionController NO debe tener [Authorize] porque
// es el endpoint que GENERA el token — si lo protegemos,
// nadie puede obtener un token (paradoja).

[ApiController]                          // SIN [Authorize]
[Route("api/autenticacion")]
public class AutenticacionController     // Login SIEMPRE abierto
```

### Que pasa si activo [Authorize] en la API?

```
ANTES (sin [Authorize]):
  Postman: GET /api/usuario -> 200 OK (devuelve datos) ← ABIERTO

DESPUES (con [Authorize]):
  Postman sin token: GET /api/usuario -> 401 Unauthorized ← BLOQUEADO
  Postman con token: GET /api/usuario + Authorization: Bearer eyJ... -> 200 OK
  Blazor con login: GET /api/usuario + Authorization: Bearer eyJ... -> 200 OK
  Blazor sin login: GET /api/usuario -> 401 Unauthorized ← BLOQUEADO
```

### Resumen: que protege que y donde se configura

| Capa | Donde se configura | Efecto |
|------|-------------------|--------|
| `[Authorize]` en controller | API C# (`Controllers/*.cs`) | Requiere JWT para acceder |
| JWT token en header | Frontend (`ApiService.cs`) | Envia el token en cada peticion |
| ProtectedSessionStorage | Frontend (`AuthService.cs`) | Guarda el token encriptado en el navegador |
| BCrypt | API C# (automatico con `?camposEncriptar=`) | Contrasenas irreversibles en BD |

---

## Que es BCrypt

BCrypt es un algoritmo de encriptacion de contrasenas **irreversible**.

**Sin BCrypt**: `contrasena = "MiClave123"` (texto plano, inseguro)
**Con BCrypt**: `contrasena = "$2a$12$LJ3m4ys1Z..."` (hash, imposible revertir)

La API generica C# maneja BCrypt automaticamente:
- Al crear/actualizar con `?camposEncriptar=contrasena`, encripta con BCrypt
- Al autenticar con `/api/autenticacion/token`, verifica con BCrypt

---

## Que es ProtectedSessionStorage

Es un mecanismo de Blazor Server para guardar datos en el navegador de forma encriptada.

```csharp
// Guardar
await _session.SetAsync("usuario", "juan@mail.com");

// Leer
var result = await _session.GetAsync<string>("usuario");
if (result.Success) { /* result.Value = "juan@mail.com" */ }

// Borrar
await _session.DeleteAsync("usuario");
```

- Se encripta automaticamente (el usuario no puede leer/modificar los datos)
- Persiste mientras el tab del navegador este abierto
- Se pierde al cerrar el tab (no es persistente como localStorage)
- Solo esta disponible despues del primer render (requiere JavaScript)

---

## Session Storage: que es, para que y como llegan los datos

### Que es Session Storage?

Es un espacio de almacenamiento del **navegador** (no del servidor).
Se ve en F12 -> Application -> Session Storage -> http://localhost:5003

```
Key                 Value
----------------    ---------------------------
usuario             CfDJ8JdZdyLIT3BLn0Ti8m0Lk...
nombre_usuario      CfDJ8JdZdyLIT3BLn0Ti8m0Lk...
roles               CfDJ8JdZdyLIT3BLn0Ti8m0Lk...
rutas_permitidas    CfDJ8JdZdyLIT3BLn0Ti8m0Lk...
```

Los **keys** se ven en texto plano pero los **values** estan encriptados.
Nadie puede leer que `usuario = carloscastro5033@correo.itm.edu.co` mirando el valor.

### Como llegan los datos ahi?

```
AuthService.Login() (despues de verificar BCrypt)
        |
        v
await _session.SetAsync("usuario", "carloscastro5033@correo.itm.edu.co");
await _session.SetAsync("roles", "Administrador,Profesor,...");
await _session.SetAsync("rutas_permitidas", "/facultad,/asignatura,...");
        |
        v
Blazor internamente:
  1. Toma el valor "carloscastro5033@correo.itm.edu.co"
  2. Lo encripta con Data Protection API -> "CfDJ8JdZdyLIT3BLn0Ti8m0Lk..."
  3. Llama JavaScript: sessionStorage.setItem("usuario", "CfDJ8...")
  4. El navegador guarda el valor encriptado
```

### Como se lee de vuelta?

```
Al refrescar (F5) o navegar a otra pagina:

MainLayout.OnAfterRenderAsync()
        |
        v
await _auth.Restaurar()
        |
        v
var result = await _session.GetAsync<string>("usuario");
        |
        +-- JavaScript: sessionStorage.getItem("usuario") -> "CfDJ8..."
        +-- Blazor desencripta con Data Protection API
        +-- result.Value = "carloscastro5033@correo.itm.edu.co"
```

### Por que ProtectedSessionStorage y no SessionStorage normal?

| Aspecto | SessionStorage (JS) | ProtectedSessionStorage (Blazor) |
|---------|-------------------|--------------------------------|
| Valores | Texto plano | Encriptados |
| Manipulable | Si (F12 -> editar) | No (si lo cambian, no desencripta) |
| Seguridad | Baja | Alta |

Si fuera texto plano, alguien podria abrir F12 y cambiar `roles` a `"Administrador"`
y tener acceso a todo. Con Protected, el valor encriptado no se puede manipular.

### Session Storage vs Local Storage

| | Session Storage | Local Storage |
|--|----------------|---------------|
| Persiste F5 (refrescar) | Si | Si |
| Persiste cerrar tab | **No** (se borra) | Si (queda) |
| Compartido entre tabs | No | Si |

Blazor usa Session Storage porque al cerrar el tab la sesion se pierde
automaticamente (mas seguro).

---

## Descubrimiento dinamico de PKs y FKs

El AuthService NO hardcodea nombres de columnas. Los descubre consultando la API.

### Como funciona?

```
UNA sola llamada: GET /api/estructuras/basedatos

La API responde con TODAS las tablas, columnas y FKs:
{
  "tablas": [
    {
      "table_name": "rol_usuario",
      "columnas": [
        {"column_name": "fkemail", "is_primary_key": "YES"},
        {"column_name": "fkidrol", "is_primary_key": "YES"}
      ],
      "foreign_keys": [
        {"column_name": "fkemail", "foreign_table_name": "usuario"},
        {"column_name": "fkidrol", "foreign_table_name": "rol"}
      ]
    }
  ]
}

El AuthService extrae y cachea en _cache:
  "rol_usuario->usuario" = "fkemail"     (FK hacia usuario)
  "rol_usuario->rol" = "fkidrol"         (FK hacia rol)
  "pk_usuario" = "email"                 (PK de usuario)
  "pk_rol" = "id"                        (PK de rol)
```

### Por que es importante?

Asi funciona con CUALQUIER base de datos sin importar como se llamen las columnas.
Si en otra BD el FK se llama `id_usuario` en vez de `fkemail`, el sistema lo descubre
automaticamente. No hay que cambiar codigo.

### Compatibilidad

- **PostgreSQL**: devuelve `foreign_table_name` directo en `foreign_keys`
- **SqlServer**: a veces no devuelve bien `foreign_table_name`, entonces
  busca como fallback en `fk_constraint_name` (ej: `fk_rolusuario_usuario`)

---

## Optimizaciones del login

| Optimizacion | Que hace | Por que |
|---|---|---|
| `PrecargarEstructura()` | 1 sola llamada para TODA la BD | En vez de 5 llamadas (una por tabla) |
| `Task.WhenAll` | Llamadas HTTP en paralelo | Login mas rapido (no secuencial) |
| `_cache` | Guarda PKs/FKs en memoria | No repite consultas a la API |
| `ProtectedSessionStorage` | Persiste sesion al refrescar (F5) | No pide login de nuevo |

---

## Las 5 tablas y su relacion (diagrama)

```
usuario --< rol_usuario >-- rol --< rutarol >-- ruta
(quien)    (tiene roles)  (que rol) (puede ver) (que pagina)

Ejemplo concreto:
  carloscastro5033@correo.itm.edu.co tiene rol "Administrador" (id=1)
  Rol "Administrador" tiene acceso a /facultad, /asignatura, /estudiante
  -> carloscastro5033 puede acceder a esas 3 paginas
  -> Si intenta /workflow -> "Acceso Denegado" (403)
  -> Si no hay rutas configuradas -> permite todo (sistema nuevo)
```

---

## Como funciona Cambiar Contrasena

### Cuando se activa?

Dos casos:
1. **Voluntario**: el usuario hace clic en "Cambiar contrasena"
2. **Forzado**: el sistema genera una contrasena temporal (recuperar) y obliga
   al usuario a cambiarla antes de poder usar la aplicacion

### Flujo completo

```
1. Usuario hace login con contrasena temporal
        |
        v
2. AuthService detecta: DebeCambiarContrasena = true
   (porque el campo debe_cambiar_contrasena es true en la BD
    o porque el email esta marcado en _emailsDebeCambiar)
        |
        v
3. Login.razor redirige automaticamente:
   -> nav.NavigateTo("/cambiar-contrasena")
        |
        v
4. MainLayout TAMBIEN bloquea (doble proteccion):
   -> if (_auth.DebeCambiarContrasena) nav.NavigateTo("/cambiar-contrasena")
   -> El usuario NO puede ir a ninguna otra pagina
   -> Si intenta ir a /facultad -> lo devuelve a /cambiar-contrasena
        |
        v
5. CambiarContrasena.razor muestra formulario:
   -> Campo: nueva contrasena
   -> Campo: confirmar contrasena
        |
        v
6. Validacion en el frontend (antes de enviar):
   -> Las dos coinciden?         Si no -> "Las contrasenas no coinciden."
   -> Minimo 6 caracteres?       Si no -> "Minimo 6 caracteres."
   -> Tiene al menos 1 mayuscula? Si no -> "Debe incluir una mayuscula."
   -> Tiene al menos 1 numero?   Si no -> "Debe incluir un numero."
        |
        v
7. AuthService.CambiarContrasena(nueva):
   -> PUT /api/usuario/{pk}/{email}?camposEncriptar=contrasena
   -> Body: {"contrasena": "NuevaContrasena123"}
        |
        v
8. La API C# recibe la peticion:
   -> Ve ?camposEncriptar=contrasena en la URL
   -> Ejecuta: BCrypt.HashPassword("NuevaContrasena123")
   -> Resultado: "$2a$12$xK9mN..."  (hash irreversible)
   -> Guarda el HASH en la BD (nunca el texto plano)
   -> La contrasena anterior desaparece para siempre
        |
        v
9. Si la API responde OK:
   -> DebeCambiarContrasena = false
   -> Redirect a / (pagina de inicio)
   -> El usuario puede navegar normalmente
```

### Que pasa con la contrasena anterior?

Se **sobreescribe** en la BD. No hay historial de contrasenas.
La anterior deja de existir — solo el nuevo hash BCrypt queda guardado.

### Por que se fuerza el cambio?

Cuando se recupera contrasena:
1. El sistema genera una temporal aleatoria (ej: "Ak7xPm2q")
2. La guarda con BCrypt en la BD
3. La envia por correo (o la muestra en pantalla)
4. Marca el email para forzar cambio

La temporal es **insegura** porque:
- Se envio por correo (puede ser interceptado)
- Se mostro en pantalla (alguien pudo verla)
- Es generada aleatoriamente (el usuario no la eligio)

Por eso se OBLIGA a crear una contrasena propia antes de usar el sistema.

---

## Configurar Gmail para SMTP (recuperacion de contrasena)

La recuperacion de contrasena genera una temporal, la guarda con BCrypt,
y la envia por correo SMTP. Si SMTP no esta configurado, muestra la
temporal en pantalla (para desarrollo).

### Paso 1: Crear una cuenta de Gmail (si no tiene una para el proyecto)

1. Ir a https://accounts.google.com/signup
2. Crear una cuenta nueva (ej: `mi.proyecto.2026@gmail.com`)
3. Completar el registro con nombre, fecha de nacimiento, etc.
4. Agregar un numero de telefono (lo va a necesitar en el paso 2)

### Paso 2: Activar la verificacion en 2 pasos

Google NO permite crear App Passwords sin verificacion en 2 pasos.
Es un requisito obligatorio. Siga estos pasos:

1. Abrir el navegador e ir a: https://myaccount.google.com/security
2. Iniciar sesion con la cuenta de Gmail del paso 1
3. Bajar en la pagina hasta la seccion **"Como inicias sesion en Google"**
4. Buscar **"Verificacion en dos pasos"** (dira "desactivada")
5. Hacer clic en **"Verificacion en dos pasos"**
6. Se abre una pagina nueva. Hacer clic en el boton **"Activar verificacion en dos pasos"**
7. Google le pedira confirmar con su telefono:
   - Seleccionar el pais (+57 Colombia)
   - Escribir su numero de celular
   - Elegir "Mensaje de texto" o "Llamada"
   - Hacer clic en "Enviar"
8. Escribir el codigo de 6 digitos que le llego al celular
9. Hacer clic en **"Activar"**
10. Verificar que ahora dice **"Activada"** en la seccion de seguridad

### Paso 3: Crear una App Password (contrasena de aplicacion)

Una App Password es una contrasena especial de 16 caracteres que
solo sirve para aplicaciones externas. La contrasena normal de Gmail
NO funciona para SMTP.

1. Ir a: https://myaccount.google.com/apppasswords
   - Si no le deja entrar, es porque el paso 2 no se completo
   - Vuelva al paso 2 y verifique que la verificacion en 2 pasos esta "Activada"
2. En **"Nombre de la app"** escribir: `BlazorLogin` (o cualquier nombre)
3. Hacer clic en **"Crear"**
4. Google muestra una contrasena de 16 caracteres, algo como: `pcsa qfto hhjf sadv`
5. **Copiarla inmediatamente** — solo se muestra UNA vez
6. Si la pierde, puede crear otra (y la anterior deja de funcionar)

### Paso 4: Configurar appsettings.json

Abrir `appsettings.json` del proyecto y poner los datos:

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "User": "mi.proyecto.2026@gmail.com",
    "Pass": "pcsa qfto hhjf sadv",
    "From": "mi.proyecto.2026@gmail.com"
  }
}
```

- **User**: la cuenta de Gmail del paso 1
- **Pass**: la App Password del paso 3 (NO la contrasena normal de Gmail)
- **From**: misma cuenta (es el remitente del correo)

### Paso 5: Probar

1. Ejecutar la aplicacion Blazor
2. Ir a `/recuperar-contrasena`
3. Escribir un email que exista en la tabla `usuario`
4. Si todo esta bien, llega un correo con la contrasena temporal
5. Si SMTP no esta configurado, muestra la temporal en pantalla

### Si algo no funciona

| Problema | Solucion |
|----------|----------|
| "App passwords not available" | El paso 2 no se completo. Verificar que dice "Activada" |
| "Username and Password not accepted" | Usar la App Password del paso 3, NO la contrasena de Gmail |
| "SMTP no configurado" | Llenar User y Pass en appsettings.json |
| "Connection refused" | Verificar que el firewall no bloquee el puerto 587 |
| No llega el correo | Revisar la carpeta de **spam** del destinatario |
| Perdio la App Password | Ir al paso 3 y crear una nueva |

### Alternativas a Gmail

| Proveedor | Host | Puerto | Notas |
|-----------|------|--------|-------|
| Gmail | smtp.gmail.com | 587 | Requiere App Password (pasos 2 y 3) |
| Outlook/Hotmail | smtp-mail.outlook.com | 587 | Contrasena normal funciona |
| Yahoo | smtp.mail.yahoo.com | 587 | Requiere App Password |
| Mailtrap (pruebas) | sandbox.smtp.mailtrap.io | 587 | Gratis para desarrollo, no envia correos reales |

---

## Probar el login

1. Asegurese de que la API C# este corriendo (puerto 5035)
2. Cree un usuario con contrasena encriptada:
   ```
   POST http://localhost:5035/api/usuario?camposEncriptar=contrasena
   Body: {"email": "admin@test.com", "contrasena": "Admin123"}
   ```
3. Cree un rol y asignelo:
   ```
   POST http://localhost:5035/api/rol
   Body: {"nombre": "Administrador"}

   POST http://localhost:5035/api/rol_usuario
   Body: {"fkemail": "admin@test.com", "fkidrol": 1}
   ```
4. Ejecute: `dotnet run`
5. Abra el navegador -> redirige a `/login`
6. Ingrese credenciales -> deberia entrar
