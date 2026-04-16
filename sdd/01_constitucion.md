# Constitución del Proyecto: FrontBlazorTutorial

> Según [Spec-Kit de GitHub](https://github.com/github/spec-kit): la constitución actúa como el
> "ADN arquitectónico del sistema". Es lo PRIMERO que lee la IA antes de generar código.

---

## Artículo I: Stack Tecnológico

| Capa | Tecnología | Versión | Justificación |
|------|-----------|---------|---------------|
| Lenguaje | C# | 13 | Lenguaje del ecosistema .NET |
| Framework | Blazor Server | .NET 9.0 | Renderiza en servidor, conexión SignalR al navegador |
| Templates | Razor Components | (incluido) | HTML con `@` para lógica C# |
| CSS | Bootstrap 5 | 5.3 CDN | Sin npm ni build tools, responsive |
| API backend | ApiGenericaCsharp | .NET 9.0 | API genérica, funciona con cualquier BD |
| BD | PostgreSQL | 17 | BD del curso, compatible con SqlServer |
| HTTP Client | HttpClient | (incluido en .NET) | Nativo, async, inyectado por DI |
| Control versiones | Git + GitHub | N/A | Trabajo colaborativo con ramas |

## Artículo II: Arquitectura del Sistema

### Arquitectura general: Cliente-Servidor en 3 capas

```
┌─────────────────────────────┐
│   CAPA DE PRESENTACIÓN      │   FrontBlazorTutorial (este proyecto)
│   Blazor Server + Razor     │   Puerto 5003
│   Components/ + Services/   │   Responsabilidad: UI, navegación, formularios
│   Conexión: SignalR          │   El navegador NO ejecuta C# — el servidor renderiza
└─────────────┬───────────────┘
              │ HTTP (HttpClient)
              │ Authorization: Bearer {JWT}
              v
┌─────────────────────────────┐
│   CAPA DE NEGOCIO (API)     │   ApiGenericaCsharp (.NET 9.0)
│   Controllers + Services    │   Puerto 5035
│   BCrypt, JWT, CRUD genérico│   Responsabilidad: validación, lógica, seguridad
└─────────────┬───────────────┘
              │ SQL (ADO.NET)
              v
┌─────────────────────────────┐
│   CAPA DE DATOS             │   PostgreSQL 17
│   Tablas, FKs, índices      │   Puerto 5432
│   ACID transaccional        │   Responsabilidad: persistencia, integridad
└─────────────────────────────┘
```

**Principio: el frontend NUNCA accede a la BD directamente.** Todo pasa por la API REST.

### Arquitectura interna: Blazor Server

```
                    ┌──────────────────────┐
                    │    SERVICIOS          │
                    │  Services/            │
                    │  ApiService.cs        │   Llama a la API vía HTTP + JWT
                    │  AuthService.cs       │   Login, roles, rutas, ConsultasController
                    └──────────┬───────────┘
                               │ Inyección de Dependencias (DI)
┌──────────────────┐           │           ┌──────────────────┐
│  LAYOUT          │           │           │ PÁGINAS           │
│  Components/     │ <─────────┼──────────>│ Components/       │
│  Layout/         │  renderiza│           │ Pages/            │
│  MainLayout.razor│           │           │ Producto.razor    │
│  NavMenu.razor   │           │           │ Login.razor       │
└──────────────────┘           │           └──────────────────┘
                               │
                    ┌──────────┴───────────┐
                    │  MIDDLEWARE (en Layout)│
                    │  MainLayout.razor     │
                    │  OnAfterRenderAsync   │   Restaurar sesión
                    │  LocationChanged      │   Verificar permisos en cada navegación
                    └──────────────────────┘
```

| Componente | Carpeta Blazor | Responsabilidad |
|------------|---------------|-----------------|
| Servicios | `Services/` | Lógica de negocio, llamadas HTTP a la API |
| Páginas | `Components/Pages/` | UI con Razor (equivale a templates/ en Flask) |
| Layout | `Components/Layout/` | Sidebar, barra superior, verificación auth |
| Programa | `Program.cs` | DI, configuración, middleware |

### Estructura de carpetas (no negociable)

```
FrontBlazorTutorial.csproj    <- Proyecto .NET
Program.cs                    <- Punto de entrada, DI, configuración
appsettings.json              <- ApiUrl, SMTP, JWT
Services/
  ApiService.cs               <- CRUD genérico con JWT en headers
  AuthService.cs              <- Login, roles, rutas, ConsultasController
Components/
  App.razor                   <- Rendermode InteractiveServer
  Layout/
    MainLayout.razor           <- Sidebar + auth + LocationChanged
    NavMenu.razor              <- Menú lateral
    EmptyLayout.razor          <- Layout vacío para login
  Pages/
    Home.razor                 <- @page "/"
    Login.razor                <- @page "/login"
    {Tabla}.razor              <- @page "/{tabla}" (CRUD)
    CambiarContrasena.razor
    RecuperarContrasena.razor
    SinAcceso.razor
wwwroot/css/app.css           <- Estilos
sdd/                          <- Documentación SDD (ESTOS archivos)
```

## Artículo III: Convenciones de Código

| Elemento | Convención | Ejemplo |
|----------|-----------|---------|
| Archivos C# | PascalCase | `AuthService.cs`, `ApiService.cs` |
| Archivos Razor | PascalCase | `Producto.razor`, `Login.razor` |
| Variables/campos | camelCase con `_` | `_auth`, `_http`, `_cargando` |
| Propiedades | PascalCase | `Usuario`, `Roles`, `EstaAutenticado` |
| Métodos | PascalCase | `Login()`, `Restaurar()`, `TieneAcceso()` |
| Constantes | PascalCase | `EstaAutenticado`, `DebeCambiarContrasena` |
| Rutas URL | minúsculas-guiones | `/cambiar-contrasena`, `/sin-acceso` |
| Tablas BD | minúsculas_guion_bajo | `rol_usuario`, `rutarol` |

## Artículo IV: Patrón CRUD (cada tabla)

```csharp
// Components/Pages/{Tabla}.razor
@page "/{tabla}"
@inject ApiService api

// Listar al cargar
protected override async Task OnInitializedAsync()
{
    datos = await api.ListarAsync("{tabla}");
}

// Crear
async Task Crear() { await api.CrearAsync("{tabla}", nuevoDato); }

// Editar
async Task Editar() { await api.ActualizarAsync("{tabla}", pk, valor, datos); }

// Eliminar
async Task Eliminar(string id) { await api.EliminarAsync("{tabla}", pk, id); }
```

## Artículo V: Seguridad (3 capas obligatorias)

| Capa | Dónde | Qué protege | Cómo |
|------|-------|-------------|------|
| **BCrypt** | BD (vía API) | Contraseñas | `?camposEncriptar=contrasena` |
| **JWT** | API (header HTTP) | Datos del backend | `Authorization: Bearer {token}` en ApiService |
| **Sesión** | Frontend (ProtectedSessionStorage) | Páginas | MainLayout verifica en OnAfterRenderAsync + LocationChanged |

### Verificación de acceso: 2 momentos

1. **OnAfterRenderAsync** (primera carga): restaura sesión, verifica auth, verifica ruta
2. **LocationChanged** (cada navegación): verifica permisos cuando el usuario hace clic o escribe URL

### Descubrimiento dinámico
- PKs y FKs se descubren vía `GET /api/estructuras/basedatos`
- NO se hardcodean nombres de columnas
- Compatible con PostgreSQL y SqlServer

## Artículo VI: Prohibiciones

| Prohibido | Razón |
|-----------|-------|
| Acceder a la BD directamente | Todo va por la API REST |
| Usar Entity Framework (ORM) | No hay BD directa |
| Push directo a main | Trabajo en ramas feature/ |
| Hardcodear URLs de la API | Van en appsettings.json |
| Hardcodear nombres FK/PK | Se descubren vía API |
| Contraseñas en texto plano | BCrypt obligatorio |
| JavaScript para lógica de negocio | Lógica en C# (servicios) |

## Artículo VII: Principios SOLID

| Principio | Sigla | Cómo se aplica en este proyecto |
|-----------|-------|--------------------------------|
| **Single Responsibility** | S | `ApiService` solo hace HTTP, `AuthService` solo hace auth, cada .razor maneja UNA tabla |
| **Open/Closed** | O | Agregar CRUD nuevo = crear .razor nuevo, NO modificar existentes |
| **Liskov Substitution** | L | `ApiService` y `AuthService` son intercambiables donde se necesite un servicio HTTP |
| **Interface Segregation** | I | `ApiService` tiene solo 4 métodos (Listar, Crear, Actualizar, Eliminar) |
| **Dependency Inversion** | D | Las páginas dependen de ApiService (inyectado por DI), no de HttpClient directo |

## Artículo VIII: Principios ACID

| Principio | Qué garantiza | Ejemplo en este proyecto |
|-----------|---------------|--------------------------|
| **Atomicidad** | Transacción completa o no se ejecuta | Factura maestro-detalle: si falla un producto, no se crea la factura |
| **Consistencia** | La BD pasa de un estado válido a otro | Un FK siempre apunta a un registro que existe |
| **Aislamiento** | Transacciones concurrentes no se interfieren | Dos usuarios creando facturas al mismo tiempo |
| **Durabilidad** | Después de COMMIT, persiste aunque se caiga el servidor | PostgreSQL garantiza esto |

## Artículo IX: Patrones de Diseño

| Patrón | Dónde se usa | Qué resuelve |
|--------|-------------|-------------|
| **Component-Based** | Components/Pages/*.razor | Cada página es un componente independiente |
| **Dependency Injection** | Program.cs → Services | Servicios inyectados, no instanciados manualmente |
| **Service Layer** | Services/ApiService.cs | Encapsular llamadas HTTP |
| **Observer** | LocationChanged en MainLayout | Reaccionar a navegación |
| **Template Method** | MainLayout.razor | Layout común, cada página llena @Body |
| **Facade** | ApiService.cs | Interfaz simple sobre la API REST |
| **Cache** | AuthService._cache | No repetir consultas de estructura |
| **Strategy (fallback)** | ConsultasController o 5 GETs | Cambiar estrategia según disponibilidad |

## Gobernanza

**Versión**: 1.0 | **Fecha**: 2026-04-14
