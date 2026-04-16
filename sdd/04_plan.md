# Etapa 4: Plan de Implementación

> Según [Spec-Kit](https://github.com/github/spec-kit): el plan traduce los requisitos de la
> especificación en decisiones técnicas concretas. "Cada elección de tecnología tiene una
> rationale documentada." El plan se genera con `/speckit.plan` y el humano lo valida.
>
> Referencia: [plan-template.md](https://github.com/github/spec-kit/blob/main/templates/plan-template.md)

---

## 1. Resumen técnico

Frontend web en Blazor Server (.NET 9.0) que consume la API genérica C# vía HTTP.
Arquitectura basada en componentes: Services (lógica) + Pages (vistas Razor) + Layout (estructura).
Autenticación con BCrypt + JWT + ProtectedSessionStorage (sesión encriptada).
Control de acceso por roles y rutas con `LocationChanged` + `VerificarAcceso()` en MainLayout.
Inyección de dependencias (DI) registrada en `Program.cs`.

## 2. Estructura de archivos del proyecto

```
FrontBlazorTutorial/
├── FrontBlazorTutorial.csproj           <- Proyecto .NET 9.0 Blazor Server
├── FrontBlazorTutorial.sln              <- Solución Visual Studio
├── Program.cs                           <- Punto de entrada: DI, HttpClient, servicios
├── appsettings.json                     <- ApiBaseUrl, Smtp config
├── appsettings.Development.json         <- Config desarrollo
│
├── Services/
│   ├── ApiService.cs                    <- CRUD genérico: ListarAsync, CrearAsync, etc.
│   ├── AuthService.cs                   <- Login, roles, rutas, ConsultasController, fallback
│   └── SpService.cs                     <- Stored procedures
│
├── Components/
│   ├── App.razor                        <- HTML raíz, @rendermode="InteractiveServer"
│   ├── Routes.razor                     <- Router de Blazor
│   ├── _Imports.razor                   <- Usings globales
│   │
│   ├── Layout/
│   │   ├── MainLayout.razor             <- Layout: sidebar + top-row + auth + LocationChanged
│   │   ├── MainLayout.razor.css         <- Estilos del layout
│   │   ├── EmptyLayout.razor            <- Layout vacío para login (sin sidebar)
│   │   ├── NavMenu.razor                <- Menú lateral con NavLink
│   │   └── NavMenu.razor.css            <- Estilos del menú
│   │
│   └── Pages/
│       ├── Home.razor                   <- Página inicio
│       ├── Login.razor                  <- Formulario login (EmptyLayout)
│       ├── CambiarContrasena.razor      <- Cambiar contraseña con validación
│       ├── RecuperarContrasena.razor     <- Recuperar contraseña por SMTP
│       ├── SinAcceso.razor              <- Error 403 (no tiene permiso)
│       ├── Producto.razor               <- CRUD producto
│       ├── Persona.razor                <- CRUD persona
│       ├── Usuario.razor                <- CRUD usuario
│       ├── Cliente.razor                <- CRUD con selects FK
│       ├── Empresa.razor                <- CRUD empresa
│       ├── Vendedor.razor               <- CRUD vendedor
│       ├── Rol.razor                    <- CRUD rol
│       ├── Ruta.razor                   <- CRUD ruta
│       ├── Factura.razor                <- Maestro-detalle
│       └── Error.razor                  <- Página de error genérica
│
├── wwwroot/
│   ├── app.css                          <- Estilos custom
│   └── lib/bootstrap/                   <- Bootstrap 5 (incluido en plantilla)
│
├── Properties/
│   └── launchSettings.json              <- Puertos de desarrollo
│
├── script_bd/                           <- SQL para crear tablas
│
├── Paso0_PlanDeDesarrollo.md            <- Plan de desarrollo
├── Paso1 a Paso12*.md                   <- Tutorial paso a paso
│
└── sdd/                                 <- Documentación SDD (estos archivos)
```

## 3. Orden de implementación (por pasos)

> Cada paso corresponde a un Paso{N}.md del tutorial y a una o más ramas feature/.

| Orden | Paso | Qué se implementa | Dependencias | Estudiante |
|-------|------|--------------------|-------------|------------|
| 1 | Paso 0 | Plan de desarrollo, reglas | Ninguna | Todos |
| 2 | Paso 3 | Proyecto base: `dotnet new blazor`, Program.cs, git | Paso 0 | Est. 1 |
| 3 | Paso 4 | ApiService (CRUD genérico HTTP) | Paso 3 | Est. 1 |
| 4 | Paso 5 | Layout base, NavMenu, Home | Paso 4 | Est. 1 |
| 5 | Paso 6 | CRUD producto | Paso 5 | Est. 1 |
| 6 | Paso 7 | CRUD persona + usuario | Paso 5 | Est. 2 + 3 |
| 7 | Paso 8 | CRUD empresa, cliente, rol | Paso 7 | Est. 2 |
| 8 | Paso 9 | CRUD ruta, vendedor, NavMenu | Paso 7 | Est. 3 |
| 9 | Paso 10 | Factura maestro-detalle | Paso 8+9 | Est. 2 |
| 10 | Paso 12 | Login + JWT + control de acceso | Paso 9 | Est. 1 |

### Diagrama de dependencias

```
Paso 0 (plan)
  |
  v
Paso 3 (proyecto base: dotnet new blazor)
  |
  v
Paso 4 (ApiService)
  |
  v
Paso 5 (layout + NavMenu + home)
  |
  +-------+-------+
  v       v       v
Paso 6  Paso 7  (paralelo: producto, persona+usuario)
  |       |
  v       +-------+
  |       v       v
  |     Paso 8  Paso 9  (paralelo: empresa+cliente, ruta+vendedor)
  |       |       |
  |       v       v
  |     Paso 10   |   (factura, depende de 8+9)
  |               |
  +-------+-------+
          v
        Paso 12 (login + seguridad)
```

## 4. Modelo de datos

### Tablas CRUD (negocio)

| Tabla | PK | Campos clave | FKs |
|-------|-----|-------------|-----|
| producto | codigo | nombre, precio, existencia | - |
| persona | codigo | nombre, telefono, direccion | - |
| empresa | codigo | nombre, nit, direccion | - |
| cliente | id | credito | fkcodpersona->persona, fkcodempresa->empresa |
| vendedor | codigo | nombre, comision | - |
| usuario | email | contrasena (BCrypt), nombre | - |
| factura | numfactura | fecha, total | fkcodvendedor->vendedor, fkcodcliente->cliente |
| productosporfactura | id | cantidad, precio | fknumfact->factura, fkcodprod->producto |

### Tablas de seguridad (auth)

| Tabla | PK | Campos | FKs |
|-------|-----|--------|-----|
| rol | id | nombre | - |
| rol_usuario | id | - | fkemail->usuario, fkidrol->rol |
| ruta | id | ruta, descripcion | - |
| rutarol | id | - | fkidrol->rol, fkidruta->ruta |

## 5. Decisiones técnicas

| Decisión | Alternativa | Razón |
|----------|-------------|-------|
| ConsultasController (1 SQL) | 5 GETs separados | Eficiencia: BD filtra, no C# |
| ProtectedSessionStorage | Cookie / localStorage | Encriptación con Data Protection API |
| HttpClient inyectado por DI | RestSharp, Refit | Incluido en .NET, sin dependencias extra |
| Bootstrap incluido en wwwroot | CDN | Ya viene con la plantilla Blazor |
| LocationChanged + VerificarAcceso | Middleware HTTP | Blazor navega por SignalR, no HTTP |
| OnAfterRenderAsync | OnInitializedAsync | ProtectedSessionStorage necesita JS (post-render) |
| @rendermode="InteractiveServer" | Static SSR | Necesario para eventos, SignalR, ProtectedSessionStorage |

## 6. Endpoints de la API utilizados

### CRUD genérico (cada tabla)

```
GET    /api/{tabla}?limite=N           <- Listar
POST   /api/{tabla}                    <- Crear
PUT    /api/{tabla}/{pk}/{valor}       <- Actualizar
DELETE /api/{tabla}/{pk}/{valor}       <- Eliminar
```

### Autenticación y seguridad

```
POST   /api/autenticacion/token        <- Login BCrypt + JWT
GET    /api/estructuras/basedatos      <- Descubrir PKs/FKs
POST   /api/consultas/ejecutar...      <- SQL JOINs roles/rutas
PUT    /api/usuario/{pk}/{val}?camposEncriptar=contrasena  <- Cambiar clave
```

---

## 7. Diagramas de secuencia

> Los diagramas de secuencia muestran la interacción entre componentes en el tiempo.
> Formato: [Mermaid](https://mermaid.js.org/) — se renderiza automáticamente en GitHub.

### 7.1 Secuencia: Login completo

```mermaid
sequenceDiagram
    actor U as Usuario
    participant N as Navegador
    participant ML as MainLayout.razor<br>(OnAfterRenderAsync)
    participant L as Login.razor
    participant A as AuthService<br>(Services/AuthService.cs)
    participant API as API Genérica C#<br>(puerto 5035)
    participant BD as PostgreSQL

    U->>N: Abre http://localhost:5003
    N->>ML: Renderiza MainLayout
    ML->>ML: OnAfterRenderAsync(firstRender=true)
    ML->>A: _auth.Restaurar()
    A->>A: ProtectedSessionStorage.GetAsync("usuario")
    A-->>ML: No hay sesión guardada

    ML->>ML: EstaAutenticado? No
    ML->>N: NavigateTo("/login")
    N-->>U: Muestra Login.razor (EmptyLayout, sin sidebar)

    U->>N: Escribe email + contraseña
    N->>L: Clic "Iniciar sesión"
    L->>A: _auth.Login(email, contraseña)

    Note over A,API: PASO 1: PrecargarEstructura()
    A->>API: GET /api/estructuras/basedatos
    API->>BD: Consulta information_schema
    BD-->>API: Tablas, columnas, PKs, FKs
    API-->>A: JSON con estructura completa
    A->>A: Cachear PKs y FKs en _cache

    Note over A,API: PASO 2: PostJson("autenticacion/token")
    A->>API: POST /api/autenticacion/token<br>{tabla, campoUsuario, campoContrasena, usuario, contrasena}
    API->>BD: SELECT contrasena FROM usuario WHERE email = ?
    BD-->>API: Hash BCrypt
    API->>API: BCrypt.Verify(contrasena, hash)
    alt Contraseña correcta
        API-->>A: 200 OK + JWT token
        A->>A: Token = JWT capturado
    else Contraseña incorrecta
        API-->>A: 401 Unauthorized
        A-->>L: (false, "Credenciales incorrectas")
        L-->>N: Muestra error en pantalla
        N-->>U: Ve mensaje de error
    end

    Note over A,API: PASO 3: CargarDatosRolesYRutas() vía ConsultasController
    A->>A: Armar SQL dinámico con FKs descubiertos
    A->>API: POST /api/consultas/ejecutarconsultaparametrizada<br>{consulta: "SELECT r.nombre, ruta_t.ruta FROM usuario u JOIN...", parametros: {email}}
    API->>BD: Ejecuta SQL con JOINs de 5 tablas
    BD-->>API: Filas: [{nombre_rol, ruta}, ...]
    API-->>A: JSON con resultados
    A->>A: Extraer roles únicos + rutas únicas

    Note over A: Guardar en ProtectedSessionStorage
    A->>A: _session.SetAsync("usuario", email)
    A->>A: _session.SetAsync("token", JWT)
    A->>A: _session.SetAsync("roles", [roles])
    A->>A: _session.SetAsync("rutas", [rutas])

    L-->>N: NavigateTo("/")
    N->>ML: Renderiza MainLayout con sidebar
    ML->>ML: VerificarAcceso() -> "/" siempre accesible
    ML-->>N: Muestra Home con sidebar + nombre usuario
    N-->>U: Página inicio con botón "Cerrar sesión"
```

### 7.2 Secuencia: CRUD Listar con JWT (AgregarTokenJwt)

```mermaid
sequenceDiagram
    actor U as Usuario
    participant N as Navegador
    participant ML as MainLayout.razor
    participant P as Producto.razor
    participant S as ApiService<br>(Services/ApiService.cs)
    participant API as API Genérica C#
    participant BD as PostgreSQL

    U->>N: Clic en "Producto" del NavMenu
    N->>ML: LocationChanged event
    ML->>ML: VerificarAcceso()
    ML->>ML: _auth.TieneAcceso("/producto")? Sí

    N->>P: Renderiza Producto.razor
    P->>S: api.ListarAsync("producto")
    S->>S: AgregarTokenJwt()
    S->>S: _auth?.Token -> lee token JWT
    S->>S: headers["Authorization"] = "Bearer eyJhbG..."
    S->>API: GET /api/producto?limite=999999<br>Header: Authorization: Bearer eyJhbG...
    API->>API: Verificar JWT (firma + expiración)
    alt JWT válido
        API->>BD: SELECT * FROM producto
        BD-->>API: Registros
        API-->>S: 200 OK + {datos: [...]}
    else JWT inválido o expirado
        API-->>S: 401 Unauthorized
        S-->>P: Lista vacía
    end
    S-->>P: List<Dictionary<string, object?>>
    P->>N: Renderiza tabla HTML con @foreach
    N-->>U: Muestra tabla con productos
```

### 7.3 Secuencia: CRUD Crear

```mermaid
sequenceDiagram
    actor U as Usuario
    participant N as Navegador
    participant P as Producto.razor
    participant S as ApiService
    participant API as API Genérica C#
    participant BD as PostgreSQL

    U->>N: Llena formulario + clic "Guardar"
    N->>P: Evento @onclick del botón
    P->>P: datos = diccionario con campos del formulario
    P->>S: api.CrearAsync("producto", datos)
    S->>S: AgregarTokenJwt()
    S->>API: POST /api/producto<br>Header: Authorization: Bearer ...<br>Body: {codigo, nombre, precio}
    API->>BD: INSERT INTO producto VALUES (...)
    BD-->>API: OK
    API-->>S: 200 + {mensaje: "Registro creado"}
    S-->>P: (true, "Registro creado")
    P->>P: mensaje = "Registro creado exitosamente"
    P->>S: api.ListarAsync("producto")
    S-->>P: Lista actualizada
    P->>N: Re-renderiza tabla + alerta verde
    N-->>U: Tabla actualizada con nuevo registro
```

### 7.4 Secuencia: Acceso denegado (LocationChanged + VerificarAcceso)

```mermaid
sequenceDiagram
    actor U as Usuario
    participant N as Navegador
    participant ML as MainLayout.razor
    participant A as AuthService

    U->>N: Escribe /factura en la barra de direcciones
    N->>ML: LocationChanged event dispara OnLocationChanged()
    ML->>ML: VerificarAcceso()
    ML->>ML: uri.AbsolutePath -> "/factura"
    ML->>ML: Es ruta pública (/, /login, /sin-acceso)? No
    ML->>A: _auth.TieneAcceso("/factura")
    A->>A: RutasPermitidas.Contains("/factura")? No
    A-->>ML: false
    ML->>N: NavigateTo("/sin-acceso")
    N-->>U: Muestra "No tiene permisos para acceder" con botón "Volver"
```

### 7.5 Secuencia: Cambiar contraseña

```mermaid
sequenceDiagram
    actor U as Usuario
    participant N as Navegador
    participant C as CambiarContrasena.razor
    participant A as AuthService
    participant API as API Genérica C#
    participant BD as PostgreSQL

    U->>N: Navega a /cambiar-contrasena
    N-->>U: Muestra formulario (nueva + confirmar)
    U->>N: Escribe nueva contraseña + confirmar
    N->>C: Clic "Cambiar contraseña"
    C->>C: Validar: coinciden? 6+ chars? mayúscula? número?
    alt Validación falla
        C-->>N: Muestra mensaje de error
        N-->>U: Ve error de validación
    end
    C->>A: _auth.CambiarContrasena(email, nueva)
    A->>A: pk = ObtenerPk("usuario") -> "email"
    A->>API: PUT /api/usuario/email/{email}?camposEncriptar=contrasena<br>Body: {contrasena: "NuevaClave123"}
    API->>API: BCrypt.HashPassword("NuevaClave123") -> "$2a$12$..."
    API->>BD: UPDATE usuario SET contrasena = "$2a$12$..." WHERE email = ?
    BD-->>API: OK
    API-->>A: 200 OK
    A-->>C: (true, "Contraseña actualizada")
    C-->>N: NavigateTo("/") + mensaje éxito
    N-->>U: Página inicio con alerta "Contraseña actualizada"
```

---

## 8. Diagrama de clases

> Muestra las clases C# del proyecto, sus atributos, métodos y relaciones.
> Formato: [Mermaid](https://mermaid.js.org/) — se renderiza en GitHub.

### 8.1 Diagrama de clases completo

```mermaid
classDiagram
    direction TB

    class ApiService {
        -HttpClient _http
        -AuthService? _auth
        -JsonSerializerOptions _jsonOptions
        -AgregarTokenJwt() void
        +ListarAsync(tabla, limite) Task~List~
        +CrearAsync(tabla, datos, camposEncriptar) Task~tuple~
        +ActualizarAsync(tabla, pk, valor, datos) Task~tuple~
        +EliminarAsync(tabla, pk, valor) Task~tuple~
    }

    class AuthService {
        +Usuario: string?
        +NombreUsuario: string?
        +Roles: List~string~
        +RutasPermitidas: HashSet~string~
        +Token: string?
        +EstaAutenticado: bool
        +DebeCambiarContrasena: bool
        -ProtectedSessionStorage _session
        -string _apiUrl
        -HttpClient _http
        -Dictionary _cache
        +Login(email, contrasena) Task~tuple~
        +Restaurar() Task
        +Logout() Task
        +TieneAcceso(ruta) bool
        +CambiarContrasena(email, nueva) Task~tuple~
        -Listar(tabla, limite) Task~List~
        -PostJson(endpoint, datos) Task~tuple~
        -PostConsulta(sql, parametros) Task~List~
        -PrecargarEstructura() Task
        -CargarDatosRolesYRutas(email) Task
        -ObtenerPk(tabla) string
        -ObtenerFk(origen, destino) string
        -CargarDatosUsuarioFallback(email) Task
        -CargarRolesFallback(email) Task
        -CargarRutasPermitidasFallback() Task
    }

    class MainLayout {
        <<LayoutComponentBase>>
        <<IDisposable>>
        -bool _cargando
        -AuthService _auth
        -NavigationManager _nav
        #OnInitialized() void
        #OnAfterRenderAsync(firstRender) Task
        -OnLocationChanged(sender, e) void
        -VerificarAcceso() void
        -_Logout() Task
        +Dispose() void
    }

    class NavMenu {
        <<Razor Component>>
        NavLink: Home
        NavLink: Producto
        NavLink: Persona
        NavLink: Usuario
        NavLink: Empresa
        NavLink: Rol
        NavLink: Ruta
        NavLink: Cliente
        NavLink: Vendedor
        NavLink: Factura
    }

    class PaginaCRUD {
        <<Razor Component>>
        -ApiService api
        -List datos
        -Dictionary formulario
        +Listar() Task
        +Crear() Task
        +Editar(pk) Task
        +Eliminar(pk) Task
    }

    class LoginPage {
        <<Razor Component>>
        -AuthService _auth
        -NavigationManager _nav
        -string email
        -string contrasena
        +IniciarSesion() Task
    }

    MainLayout --> NavMenu : contiene sidebar
    MainLayout --> PaginaCRUD : @Body renderiza páginas
    MainLayout --> LoginPage : redirige si no autenticado
    MainLayout o-- AuthService : @inject (DI)
    MainLayout ..|> IDisposable : implementa

    PaginaCRUD o-- ApiService : @inject (DI)
    LoginPage o-- AuthService : @inject (DI)

    ApiService o-- AuthService : recibe en constructor (DI)

    note for AuthService "Descubrimiento dinámico FK/PK\nConsultasController (1 SQL)\nFallback: GETs separados\nProtectedSessionStorage encriptado"
    note for ApiService "JWT en header Authorization\nFacade sobre la API REST\nAgregarTokenJwt() antes de cada request"
    note for MainLayout "OnAfterRenderAsync: restaurar sesión\nLocationChanged: verificar acceso\nIDisposable: limpiar suscripción"
```

### 8.2 Relaciones entre clases

| Relación | Tipo | Descripción |
|----------|------|-------------|
| MainLayout -> NavMenu | Composición | MainLayout contiene NavMenu en el sidebar |
| MainLayout -> Pages | Composición | MainLayout renderiza páginas en @Body |
| MainLayout o-- AuthService | Inyección DI | @inject inyecta AuthService en MainLayout |
| Pages o-- ApiService | Inyección DI | @inject inyecta ApiService en cada página CRUD |
| LoginPage o-- AuthService | Inyección DI | Login usa AuthService para autenticar |
| ApiService o-- AuthService | Constructor DI | ApiService recibe AuthService para leer el token JWT |
| MainLayout ..|> IDisposable | Implementación | Limpia suscripción a LocationChanged al destruir |
| AuthService --|> ApiService | Independiente | AuthService NO depende de ApiService (usa HttpClient directo) |

### 8.3 Por qué AuthService es independiente de ApiService?

```
AuthService usa HttpClient directo, NO ApiService.

Razón: ApiService puede tener firmas diferentes según el proyecto
(ListarAsync vs Listar, parámetros distintos, etc).
AuthService con HttpClient directo funciona en CUALQUIER proyecto Blazor.

AuthService                    ApiService
  |                              |
  +-- HttpClient (directo)       +-- HttpClient (inyectado por DI)
  |   (métodos Listar, PostJson) |   (con AgregarTokenJwt del _auth)
  v                              v
  API Genérica C#                API Genérica C#
```

---

## Referencias Spec-Kit

- Formato plan: [plan-template.md](https://github.com/github/spec-kit/blob/main/templates/plan-template.md)
- Principio de simplicidad: [spec-driven.md, Artículo VII](https://github.com/github/spec-kit/blob/main/spec-driven.md)
- Flujo SDD: [README de Spec-Kit](https://github.com/github/spec-kit)
- Mermaid (diagramas): [mermaid.js.org](https://mermaid.js.org/)
