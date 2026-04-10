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
