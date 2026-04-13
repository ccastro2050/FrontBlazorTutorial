# Paso 12: Login y Control de Acceso - Blazor Server

## Los 3 conceptos clave de seguridad

1. **Autenticacion** → ¿Quien eres? (login, BCrypt, JWT)
2. **Autorizacion** → ¿Que puedes hacer? (roles, rutas, permisos)
3. **Encriptacion** → ¿Como se protege? (BCrypt para contrasenas, Data Protection para sesion, HTTPS para transporte)

## Que se implemento

- **Login** con email y contrasena encriptada (BCrypt)
- **Control de acceso por roles**: cada usuario tiene roles, cada rol tiene rutas
- **Cambio de contrasena** con validacion de seguridad
- **Sesion persistente** con ProtectedSessionStorage (sobrevive F5)
- **Descubrimiento dinamico** de PKs y FKs (compatible Postgres + SqlServer)

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
