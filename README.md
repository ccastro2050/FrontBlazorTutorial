# FrontBlazorTutorial

**Profesor:** Carlos Arturo Castro Castro

Frontend web construido con **Blazor Server (.NET 9 / C#)** que consume una API REST genérica en C# (`ApiGenericaCsharp`) para gestionar las tablas de una base de datos de facturación.

---

## 1. Descripción del proyecto

Sistema web tipo **SPA (Single Page Application)** que permite realizar operaciones CRUD (Crear, Leer, Actualizar, Eliminar) sobre 9 tablas de una base de datos, más una página de facturación con maestro-detalle usando Stored Procedures.

El frontend **no accede directamente a la base de datos**. Toda la comunicación pasa por una API REST intermedia, siguiendo el principio de **separación de responsabilidades**:

```
┌──────────────┐     SignalR +     ┌────────────────────┐       SQL        ┌──────────────┐
│   Frontend   │     HTTP          │   API REST (C#)    │  ←────────────→  │  Base de     │
│   Blazor     │  ←────────────→   │  ApiGenericaCsharp  │  Queries/SPs    │  Datos       │
│   Puerto 5235│                   │  Puerto 5035       │                  │  (la que use │
│              │                   │                    │                  │   la API)    │
└──────────────┘                   └────────────────────┘                  └──────────────┘
```

**¿Qué es Blazor Server?** A diferencia de React o Angular (donde el JavaScript corre en el navegador), Blazor Server ejecuta la lógica C# **en el servidor**. El navegador solo muestra HTML. Cada clic del usuario viaja al servidor por una conexión **SignalR** (WebSocket), el servidor ejecuta el código C#, genera el HTML actualizado y lo envía de vuelta. El usuario no nota la diferencia porque la conexión es en tiempo real.

---

## 2. Arquitectura

### 2.1 Patrón arquitectónico: Component-based + Service Layer

Blazor usa un modelo basado en **componentes** (archivos `.razor` que mezclan HTML con C#):

| Capa | Rol | Archivos |
|------|-----|----------|
| **Componentes (Pages)** | Páginas interactivas con HTML + lógica C# en `@code {}` | `Components/Pages/*.razor` |
| **Componentes (Layout)** | Estructura visual compartida (menú, layout) | `Components/Layout/*.razor` |
| **Servicios** | Clases C# que encapsulan las llamadas HTTP a la API | `Services/ApiService.cs`, `Services/SpService.cs` |
| **Configuración** | URL de la API, inyección de dependencias | `Program.cs`, `appsettings.json` |

### 2.2 Flujo de una petición (ejemplo: listar productos)

```
1. El usuario abre http://localhost:5235/producto
         ↓
2. Blazor busca el componente con @page "/producto" → Producto.razor
         ↓
3. Se ejecuta OnInitializedAsync():
   - Llama a api.ListarAsync("producto")
         ↓
4. ApiService hace GET http://localhost:5035/api/producto con HttpClient
         ↓
5. La API consulta la base de datos y retorna JSON:
   {"datos": [{"codigo": "P001", "nombre": "Laptop", ...}]}
         ↓
6. ApiService deserializa el JSON y retorna una List<Dictionary>
         ↓
7. Blazor actualiza la variable "productos" → el HTML se re-renderiza
   automáticamente (reactividad)
         ↓
8. SignalR envía solo el diff del HTML al navegador (no recarga la página)
```

### 2.3 Flujo de una operación de escritura (ejemplo: crear producto)

```
1. El usuario llena el formulario (los inputs están vinculados con @bind)
         ↓
2. Hace clic en "Guardar" → se ejecuta la función Guardar() en @code
         ↓
3. Guardar() llama a api.CrearAsync("producto", datos)
         ↓
4. ApiService hace POST http://localhost:5035/api/producto con JSON
         ↓
5. La API inserta en la base de datos y retorna éxito/error
         ↓
6. Guardar() actualiza el mensaje de alerta y recarga la lista
         ↓
7. Blazor re-renderiza solo los elementos que cambiaron (no recarga la página)
```

---

## 3. Tecnologías y conceptos

### 3.1 Stack tecnológico

| Tecnología | Versión | Rol |
|-----------|---------|-----|
| **C#** | 12 | Lenguaje de programación |
| **.NET** | 9 | Plataforma de ejecución |
| **Blazor Server** | .NET 9 | Framework web (componentes interactivos con SignalR) |
| **Razor** | - | Sintaxis para mezclar HTML con C# (`.razor`) |
| **HttpClient** | - | Clase nativa de .NET para peticiones HTTP |
| **SignalR** | - | Conexión en tiempo real entre servidor y navegador (WebSocket) |
| **Bootstrap** | 5.3 | Framework CSS para diseño visual (incluido en el template) |
| **Git / GitHub** | - | Control de versiones y colaboración |

### 3.2 Conceptos clave de Blazor

| Concepto | Qué es | Dónde se usa |
|----------|--------|-------------|
| **Componente (.razor)** | Archivo que combina HTML y C# en un solo lugar. Todo en Blazor es un componente | `Components/Pages/*.razor` |
| **@page "/ruta"** | Directiva que asocia una URL con un componente | Al inicio de cada página |
| **@code { }** | Bloque donde va la lógica C# del componente (variables, funciones, eventos) | En cada `.razor` |
| **@bind** | Two-way binding: vincula un input HTML con una variable C#. Si el usuario escribe, la variable se actualiza automáticamente | Inputs de formularios |
| **@inject** | Inyección de dependencias: pide al framework una instancia de un servicio registrado | `@inject ApiService api` |
| **OnInitializedAsync()** | Método que se ejecuta automáticamente cuando el componente se carga por primera vez | Para cargar datos iniciales |
| **StateHasChanged()** | Le dice a Blazor que algo cambió y debe re-renderizar el HTML | Después de operaciones async |
| **NavigationManager** | Servicio para navegar entre páginas sin recargar | Redirecciones programáticas |
| **Inyección de dependencias (DI)** | Patrón donde el framework crea y provee las instancias de servicios | `Program.cs` registra, `@inject` consume |

### 3.3 Conceptos de la API REST

| Concepto | Qué es |
|----------|--------|
| **REST** | Estilo de arquitectura para APIs web. Usa HTTP (GET, POST, PUT, DELETE) para operar sobre recursos |
| **Endpoint** | URL específica de la API (ej: `GET /api/producto` lista productos) |
| **JSON** | Formato de intercambio de datos entre el frontend y la API |
| **CRUD** | Las 4 operaciones básicas: Create, Read, Update, Delete |
| **Stored Procedure (SP)** | Función almacenada en la base de datos que ejecuta lógica compleja (ej: crear factura con detalle) |

### 3.4 Mapeo HTTP → CRUD → API

| Operación | Método HTTP | Endpoint de la API | Método en ApiService |
|-----------|------------|-------------------|---------------------|
| Listar | GET | `/api/{tabla}` | `ListarAsync(tabla)` |
| Crear | POST | `/api/{tabla}` | `CrearAsync(tabla, datos)` |
| Actualizar | PUT | `/api/{tabla}/{clave}/{valor}` | `ActualizarAsync(tabla, clave, valor, datos)` |
| Eliminar | DELETE | `/api/{tabla}/{clave}/{valor}` | `EliminarAsync(tabla, clave, valor)` |
| Ejecutar SP | POST | `/api/procedimientos/ejecutarsp` | `EjecutarSpAsync(nombre, params)` |

---

## 4. Estructura del proyecto

```
FrontBlazorTutorial/
├── Program.cs                           ← Punto de entrada: configura servicios y DI
├── appsettings.json                     ← Configuración: URL de la API
├── FrontBlazorTutorial.csproj           ← Archivo de proyecto .NET
├── Services/
│   ├── ApiService.cs                    ← Servicio genérico: CRUD con HttpClient
│   └── SpService.cs                     ← Servicio para Stored Procedures
├── Components/
│   ├── App.razor                        ← Componente raíz
│   ├── _Imports.razor                   ← Usings globales
│   ├── Layout/
│   │   ├── MainLayout.razor             ← Layout principal (sidebar + contenido)
│   │   └── NavMenu.razor                ← Menú de navegación lateral
│   └── Pages/
│       ├── Home.razor                   ← Info de conexión a la BD
│       ├── Producto.razor               ← CRUD Producto (modelo para los demás)
│       ├── Persona.razor                ← CRUD Persona
│       ├── Usuario.razor                ← CRUD Usuario
│       ├── Empresa.razor                ← CRUD Empresa
│       ├── Cliente.razor                ← Con selects de FK (persona, empresa)
│       ├── Rol.razor                    ← Clave int en vez de string
│       ├── Ruta.razor                   ← Clave con nombre igual a la tabla
│       ├── Vendedor.razor               ← Con select de FK (persona)
│       ├── Factura.razor                ← Maestro-detalle con SPs y JavaScript
│       └── Error.razor                  ← Página de error
├── wwwroot/                             ← Archivos estáticos (CSS, JS)
├── bin/                                 ← Compilados (NO se sube a GitHub)
└── obj/                                 ← Archivos temporales (NO se sube a GitHub)
```

### Convención de archivos

Cada tabla tiene **1 solo archivo** `.razor` que contiene tanto el HTML como la lógica C# en `@code {}`. Esto es diferente a Flask (2 archivos: `.py` + `.html`) pero similar a React (1 archivo `.jsx`).

---

## 5. Patrones de diseño utilizados

| Patrón | Dónde se aplica | Qué resuelve |
|--------|----------------|-------------|
| **Service Layer** | `Services/ApiService.cs` | Encapsula toda la comunicación HTTP. Los componentes no usan `HttpClient` directamente |
| **Dependency Injection** | `Program.cs` + `@inject` | El framework crea y provee instancias de servicios. Los componentes no hacen `new ApiService()` |
| **Component-based** | Cada `.razor` es un componente | Cada página es independiente, con su propia lógica y vista |
| **Two-way Binding** | `@bind` en inputs | Los datos del formulario se sincronizan automáticamente con variables C# |
| **Async/Await** | Todos los métodos del servicio | Las llamadas HTTP no bloquean la interfaz. El usuario puede seguir interactuando |
| **Configuration** | `appsettings.json` | La URL de la API se configura externamente, no está hardcodeada en el código |

---

## 6. Páginas del sistema

| Página | Ruta | Tabla | Campos | Responsable |
|--------|------|-------|--------|-------------|
| Home | `/` | - | Info de conexión a la BD | Estudiante 1 |
| Producto | `/producto` | producto | codigo, nombre, stock, valorunitario | Estudiante 1 |
| Persona | `/persona` | persona | codigo, nombre, email, telefono | Estudiante 2 |
| Usuario | `/usuario` | usuario | codigo, nombre, email, clave | Estudiante 3 |
| Empresa | `/empresa` | empresa | codigo, nombre | Estudiante 1 |
| Cliente | `/cliente` | cliente | id (auto), credito, fkcodpersona, fkcodempresa | Estudiante 2 |
| Rol | `/rol` | rol | id, nombre | Estudiante 3 |
| Ruta | `/ruta` | ruta | ruta, descripcion | Estudiante 1 |
| Vendedor | `/vendedor` | vendedor | id (auto), carnet, direccion, fkcodpersona | Estudiante 2 |
| Factura | `/factura` | factura + factura_detalle | Maestro-detalle con SPs | Estudiante 2 |

---

## 7. Requisitos previos

- **.NET 9 SDK** instalado
- **API REST** (`ApiGenericaCsharp`) corriendo en `http://localhost:5035`
- **Base de datos** SQL Server, PostgreSQL, MySQL u otra con las tablas de facturación creadas
- **Git** instalado

---

## 8. Instalación y ejecución

```bash
# 1. Clonar el repositorio
git clone https://github.com/ccastro2050/FrontBlazorTutorial.git
cd FrontBlazorTutorial

# 2. Restaurar paquetes NuGet (automático con dotnet)
dotnet restore

# 3. Verificar que la API está corriendo
# (debe estar en http://localhost:5035)

# 4. Ejecutar
dotnet run
```

Abrir en el navegador: **http://localhost:5235**

---

## 9. Flujo de trabajo con Git

```
Estudiante 2 o 3                              Estudiante 1
(en su PC)                                    (dueño del repo)
     |                                              |
  1. git checkout main                              |
  2. git pull                                       |
  3. git checkout -b feature/nombre                 |
  4. (escribe el código)                            |
  5. git add .                                      |
  6. git commit -m "feat: descripción"              |
  7. git push -u origin feature/nombre              |
     |                                              |
  8. Ve el botón amarillo en GitHub                 |
     "Compare & pull request"                       |
     Crea el Pull Request                           |
     |                                              |
     └──── le avisa a Estudiante 1 ────────────────→|
                                                    |
                                         9.  Va a la pestaña Pull requests
                                         10. Abre el PR, revisa en Files changed
                                         11. Merge pull request → Confirm merge
                                                    |
                                         Los cambios quedan en main
```

---

## 10. Comparación con otros frontends del mismo proyecto

| Aspecto | Blazor (C#) | Flask (Python) | React (JavaScript) |
|---------|-------------|---------------|-------------------|
| **Tipo de app** | SPA (SignalR) | MPA (recarga páginas) | SPA (cliente) |
| **Lenguaje** | C# | Python | JavaScript (JSX) |
| **Servidor dev** | `dotnet run` | `python app.py` | `npm run dev` |
| **Página CRUD** | 1 archivo: `.razor` | 2 archivos: `.py` + `.html` | 1 archivo: `.jsx` |
| **Servicio API** | `ApiService.cs` (HttpClient) | `api_service.py` (requests) | `ApiService.js` (fetch) |
| **Formularios** | `@bind` (two-way binding) | `request.form` (POST) | `useState` + `onChange` |
| **Navegación** | NavLink (sin recarga, SignalR) | Links HTML (recarga completa) | React Router (sin recarga) |
| **Estado** | Variables en `@code` | Sesiones del servidor | `useState` / `useEffect` |
| **Puerto** | 5235 | 5300 | 5173 |

---

## 11. Distribución del trabajo

| Sprint | Paso | Estudiante 1 | Estudiante 2 | Estudiante 3 |
|--------|------|-------------|-------------|-------------|
| 1 | 1-3 | Crear repo + proyecto + GitHub | Clonar y verificar | Clonar y verificar |
| 2 | 4-5 | ApiService + Layout + Home | Pull y verificar | Pull y verificar |
| 3 | 6 | CRUD Producto | Pull y verificar | Pull y verificar |
| 4 | 7 | Revisar PRs + merge | CRUD Persona | CRUD Usuario |
| 5 | 8 | CRUD Empresa | CRUD Cliente | CRUD Rol |
| 6 | 9 | CRUD Ruta | CRUD Vendedor | NavMenu |
| 7 | 10 | Home actualizado | CRUD Factura | Revisar + verificar |

---

## 12. Tutorial paso a paso

| Archivo | Contenido |
|---------|----------|
| `Paso0_PlanDeDesarrollo.md` | Plan de desarrollo, buenas prácticas, historias de usuario |
| `Paso1_ConceptosBasicos.md` | Conceptos de Blazor, Razor, componentes |
| `Paso2_Herramientas.md` | Instalación de .NET SDK, VS Code, Git |
| `Paso3_CrearProyectoYGitHub.md` | Crear proyecto Blazor, GitHub, ramas |
| `Paso4_ConexionApiYServicio.md` | ApiService + appsettings.json |
| `Paso5_LayoutNavegacionHome.md` | MainLayout, NavMenu, página Home |
| `Paso6_CrudProducto.md` | Primer CRUD completo (modelo para los demás) |
| `Paso7_CrudPersonaYUsuario.md` | CRUD Persona (Est2) + CRUD Usuario (Est3) en paralelo |
| `Paso8_CrudEmpresaClienteRol.md` | CRUD Empresa (Est1) + CRUD Cliente (Est2) + CRUD Rol (Est3) |
| `Paso9_CrudRutaVendedorNavMenu.md` | CRUD Ruta (Est1) + CRUD Vendedor (Est2) + NavMenu (Est3) |
| `Paso10_Factura.md` | Factura maestro-detalle con Stored Procedures |
| `Paso11_ResumenFlujoCompleto.md` | Resumen final del proyecto |
