# Etapa 5: Tareas y Código

> Según [Spec-Kit](https://github.com/github/spec-kit): las tareas se derivan del plan y
> se organizan por historia de usuario. Cada tarea tiene ruta de archivo específica
> y marcadores de paralelización `[P]`. Se generan con `/speckit.tasks`.
>
> Referencia: [tasks-template.md](https://github.com/github/spec-kit/blob/main/templates/plan-template.md)

---

## Leyenda

- `[x]` = Completado
- `[ ]` = Pendiente
- `[P]` = Paralelizable (puede hacerse al mismo tiempo que otra tarea [P])
- `->` = Dependencia (requiere que la tarea anterior esté completada)

---

## Historia 1: Proyecto base (Paso 3)

**Estudiante 1 — Rama: main (commit inicial)**

- [x] Crear proyecto: `dotnet new blazor -n FrontBlazorTutorial`
- [x] Configurar `FrontBlazorTutorial.csproj` con .NET 9.0
- [x] Configurar `Program.cs` con servicios Blazor Server
  - Archivo: `Program.cs`
  - `AddRazorComponents().AddInteractiveServerComponents()`
- [x] Configurar `appsettings.json` con `ApiBaseUrl`
- [x] Crear `.gitignore` (bin/, obj/, .vs/)
- [x] Inicializar git, primer commit, push a GitHub
- [x] Invitar colaboradores

---

## Historia 2: Servicio API (Paso 4)

**Estudiante 1 — Rama: api-service**

- [x] Crear `Services/ApiService.cs` con clase `ApiService`
  - Archivo: `Services/ApiService.cs`
  - Métodos: `ListarAsync()`, `CrearAsync()`, `ActualizarAsync()`, `EliminarAsync()`
  - Constructor recibe `HttpClient` inyectado por DI
- [x] Registrar en `Program.cs`: `builder.Services.AddScoped<ApiService>()`
- [x] Verificar conexión con la API: `GET /api/producto`
- [x] Merge a main

---

## Historia 3: Layout y navegación (Paso 5)

**Estudiante 1 — Rama: layout**

- [x] Configurar `Components/Layout/MainLayout.razor`
  - Sidebar + top-row + @Body + Bootstrap 5
- [x] Crear `Components/Layout/NavMenu.razor` con NavLink
- [x] Crear `Components/Pages/Home.razor` (página inicio)
- [x] Configurar `Components/App.razor` con `@rendermode="InteractiveServer"`
- [x] Verificar que `wwwroot/lib/bootstrap/` tiene Bootstrap 5
- [x] Merge a main

---

## Historia 4: CRUD Producto (Paso 6)

**Estudiante 1 — Rama: feature/crud-producto**

- [x] Crear `Components/Pages/Producto.razor`
  - @page "/producto"
  - @inject ApiService
  - Tabla HTML + formulario crear/editar + mensajes
  - Listar, Crear, Editar, Eliminar
- [x] Agregar NavLink en `NavMenu.razor`
- [x] Merge a main

---

## Historia 5: CRUD Persona + Usuario (Paso 7) `[P]`

**Estudiante 2 — Rama: feature/crud-persona**
**Estudiante 3 — Rama: feature/crud-usuario**

- [x] `[P]` Crear `Components/Pages/Persona.razor` (Est. 2)
- [x] `[P]` Crear `Components/Pages/Usuario.razor` (Est. 3)
- [x] Agregar NavLinks en `NavMenu.razor`
- [x] Merge ambas ramas a main

---

## Historia 6: CRUD Empresa, Cliente, Rol (Paso 8) `[P]`

**Estudiante 2 — Rama: feature/crud-empresa-cliente**

- [x] `[P]` Crear `Components/Pages/Empresa.razor`
- [x] `[P]` Crear `Components/Pages/Cliente.razor`
  - Select FK para persona y empresa (cargados con ListarAsync)
- [x] `[P]` Crear `Components/Pages/Rol.razor`
- [x] Merge a main

---

## Historia 7: CRUD Ruta, Vendedor, NavMenu (Paso 9) `[P]`

**Estudiante 3 — Rama: feature/crud-ruta-vendedor**

- [x] `[P]` Crear `Components/Pages/Ruta.razor`
- [x] `[P]` Crear `Components/Pages/Vendedor.razor`
- [x] Actualizar `Components/Layout/NavMenu.razor` con todas las tablas
- [x] Merge a main

---

## Historia 8: Factura maestro-detalle (Paso 10)

**Estudiante 2 — Rama: feature/factura**
-> Depende de: Historia 6 + Historia 7

- [x] Crear `Components/Pages/Factura.razor` con lógica maestro-detalle
  - Cabecera: vendedor (select FK), cliente (select FK), fecha
  - Detalle: tabla dinámica con producto, cantidad, precio
  - Botones agregar/eliminar filas de detalle
  - Cálculo de subtotales y total
- [x] Merge a main

---

## Historia 9: Login y Control de Acceso (Paso 12)

**Estudiante 1 — Rama: feature/login**
-> Depende de: Todas las historias anteriores

### 9.1 Servicio de autenticación
- [x] Crear `Services/AuthService.cs`
  - Archivo: `Services/AuthService.cs`
  - Constructor recibe `ProtectedSessionStorage` e `IConfiguration` (DI)
  - Helpers HTTP internos: `Listar()`, `PostJson()` (HttpClient directo, NO usa ApiService)
  - `PrecargarEstructura()`: GET `/api/estructuras/basedatos`, cachea PKs/FKs
  - `PostConsulta()`: POST a ConsultasController con SQL parametrizado
  - `CargarDatosRolesYRutas()`: 1 SQL con JOINs (5 tablas -> roles + rutas del usuario)
  - Fallbacks: `CargarDatosUsuarioFallback()`, `CargarRolesFallback()`, `CargarRutasPermitidasFallback()` (GETs separados si ConsultasController no está disponible)
  - `Login()`: PrecargarEstructura + PostJson("autenticacion/token") + CargarDatosRolesYRutas + guardar en ProtectedSessionStorage
  - `Restaurar()`: leer sesión de ProtectedSessionStorage (para sobrevivir F5)
  - `TieneAcceso(ruta)`: verificar si la ruta está en RutasPermitidas
  - `CambiarContrasena()`: PUT con `?camposEncriptar=contrasena`
  - `Logout()`: limpiar propiedades + borrar ProtectedSessionStorage

### 9.2 Control de acceso en MainLayout
- [x] Modificar `Components/Layout/MainLayout.razor`
  - Archivo: `Components/Layout/MainLayout.razor`
  - `@inject AuthService _auth` y `@inject NavigationManager _nav`
  - `@implements IDisposable` para limpiar suscripción a LocationChanged
  - `OnInitialized()`: suscribir `_nav.LocationChanged += OnLocationChanged`
  - `OnAfterRenderAsync(firstRender)`: llamar `_auth.Restaurar()`, verificar sesión, redirigir a `/login` si no autenticado, verificar `DebeCambiarContrasena`, llamar `VerificarAcceso()`
  - `OnLocationChanged()`: llamar `VerificarAcceso()` en cada navegación
  - `VerificarAcceso()`: extraer ruta del URL, verificar rutas públicas, llamar `_auth.TieneAcceso(ruta)`, redirigir a `/sin-acceso` si no tiene permiso
  - `Dispose()`: `_nav.LocationChanged -= OnLocationChanged`
  - Spinner mientras restaura sesión (`_cargando`)
  - Botón "Cerrar sesión" en top-row

### 9.3 JWT en ApiService
- [x] Modificar `Services/ApiService.cs`
  - Constructor recibe `AuthService? auth = null` (DI)
  - `AgregarTokenJwt()`: lee `_auth?.Token`, agrega header `Authorization: Bearer {token}`
  - Llamar `AgregarTokenJwt()` antes de cada `_http.GetFromJsonAsync` / `PostAsJsonAsync` / etc.

### 9.4 App.razor con InteractiveServer
- [x] Modificar `Components/App.razor`
  - `<Routes @rendermode="InteractiveServer" />` — necesario para ProtectedSessionStorage y eventos

### 9.5 Páginas de auth
- [x] `Components/Pages/Login.razor` — formulario login con EmptyLayout (sin sidebar)
- [x] `Components/Pages/CambiarContrasena.razor` — cambiar contraseña con validación
- [x] `Components/Pages/RecuperarContrasena.razor` — recuperar contraseña por SMTP
- [x] `Components/Pages/SinAcceso.razor` — página 403

### 9.6 Layout vacío para login
- [x] Crear `Components/Layout/EmptyLayout.razor` — sin sidebar, solo @Body

### 9.7 Registro de servicios en Program.cs
- [x] Modificar `Program.cs`
  - `builder.Services.AddScoped<AuthService>()`
  - `builder.Services.AddScoped<ApiService>(sp => new ApiService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<AuthService>()))`

### 9.8 Configuración SMTP
- [x] Agregar sección `"Smtp"` en `appsettings.json` (Host, Port, User, Pass, From)

### 9.9 Documentación
- [x] Crear `Paso12_LoginYControlDeAcceso.md` — documento completo

---

## Validación final

- [x] Login con usuario sin roles -> "No tiene roles asignados"
- [x] Login con usuario con rol Vendedor -> entra, solo ve sus rutas
- [x] Login con usuario con todos los roles -> acceso total
- [x] JWT se envía en cada petición (verificar en F12 Network: header Authorization)
- [x] Ruta no permitida -> página 403 (SinAcceso.razor)
- [x] Cambiar contraseña funciona con BCrypt
- [x] Sesión sobrevive F5 (ProtectedSessionStorage)
- [x] Sesión se pierde al cerrar tab del navegador
- [x] Proyecto arranca con `dotnet run` sin errores

---

## Artefactos generados (resultado del SDD)

Siguiendo [Spec-Kit](https://github.com/github/spec-kit), estos son los artefactos que se produjeron:

| Artefacto SDD | Archivo en este proyecto |
|---------------|--------------------------|
| constitution.md | `sdd/01_constitucion.md` |
| spec.md | `sdd/02_especificacion.md` |
| clarify | `sdd/03_clarificacion.md` |
| plan.md | `sdd/04_plan.md` |
| tasks.md | `sdd/05_tareas.md` (ESTE archivo) |
| Código fuente | `Program.cs`, `Services/`, `Components/`, etc. |
| Documentación paso a paso | `Paso0.md` a `Paso12.md` |

> "Lo más importante del SDD es que la documentación es un entregable que se versiona,
> y el código es el resultado de esta documentación."

---

## Referencias

- [GitHub Spec-Kit](https://github.com/github/spec-kit) — Toolkit oficial SDD
- [spec-driven.md](https://github.com/github/spec-kit/blob/main/spec-driven.md) — Documento técnico SDD
- [Diving Into SDD With Spec Kit (Microsoft)](https://developer.microsoft.com/blog/spec-driven-development-spec-kit)
- [Video: La forma CORRECTA de programar con IA en 2026](https://youtu.be/p2WA672HrdI)
