# Paso 12: Login y Control de Acceso - Blazor Server

## Que se implemento

- **Login** con email y contrasena encriptada (BCrypt)
- **Control de acceso por roles**: cada usuario tiene roles, cada rol tiene rutas
- **Cambio de contrasena** con validacion de seguridad
- **Sesion persistente** con ProtectedSessionStorage (sobrevive F5)
- **Descubrimiento dinamico** de PKs y FKs (compatible Postgres + SqlServer)

---

## Archivos creados

```
FrontBlazorTutorial/
├── Services/
│   └── AuthService.cs                 <- Logica de auth, roles, rutas, descubrimiento dinamico
├── Components/
│   ├── Pages/
│   │   ├── Login.razor                <- Formulario de login
│   │   ├── CambiarContrasena.razor    <- Cambio de contrasena
│   │   └── SinAcceso.razor            <- Pagina error 403
│   └── Layout/
│       ├── MainLayout.razor           <- Modificado: auth check + boton logout
│       └── EmptyLayout.razor          <- Layout vacio para login (sin sidebar)
├── Components/
│   └── App.razor                      <- Modificado: @rendermode InteractiveServer
└── Program.cs                         <- Modificado: registra AuthService
```

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

## Flujo de autenticacion

```
1. Usuario abre http://localhost:5003
2. MainLayout.OnAfterRenderAsync:
   a. _auth.Restaurar() -> intenta recuperar sesion del navegador
   b. No hay sesion -> nav.NavigateTo("/login")
3. Login.razor: usuario escribe email + contrasena
4. auth.Login():
   a. PrecargarEstructura() + POST /api/autenticacion/token  (EN PARALELO)
   b. CargarDatosUsuario() + CargarRoles()                   (EN PARALELO)
   c. CargarRutasPermitidas() (rutarol + rol + ruta)          (3 EN PARALELO)
   d. Guardar en ProtectedSessionStorage
5. nav.NavigateTo("/")
6. Cada pagina: MainLayout verifica _auth.TieneAcceso(ruta)
```

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

## Descubrimiento dinamico de PKs y FKs

El AuthService NO hardcodea nombres de columnas. Los descubre consultando la API:

```
GET /api/estructuras/basedatos  ->  retorna TODA la estructura de la BD
```

Ejemplo: para saber que columna de `rol_usuario` apunta a `usuario`:
- La API responde: `foreign_keys: [{column_name: "fkemail", foreign_table_name: "usuario"}]`
- El servicio cachea: `"rol_usuario->usuario" = "fkemail"`

Asi funciona con cualquier BD sin importar como se llamen las columnas.

---

## Configurar Gmail para SMTP (recuperacion de contrasena)

NOTA: La recuperacion de contrasena por email aun no esta implementada en Blazor
(esta pendiente). Pero cuando se agregue, necesitara SMTP.

### Paso 1: Activar verificacion en 2 pasos

1. Ir a https://myaccount.google.com/security
2. Buscar "Verificacion en 2 pasos" -> Activarla

### Paso 2: Crear App Password

1. Ir a https://myaccount.google.com/apppasswords
2. Nombre: "BlazorLogin"
3. Copiar la contrasena de 16 caracteres (ej: `abcdefghijklmnop`)

### Paso 3: Configurar appsettings.json

```json
{
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUser": "tucuenta@gmail.com",
  "SmtpPass": "abcdefghijklmnop",
  "SmtpFrom": "tucuenta@gmail.com"
}
```

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
