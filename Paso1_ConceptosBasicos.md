# Paso 1 — Conceptos Básicos

Este es el punto de partida. Aquí se cubren todos los conceptos que el proyecto utiliza, resumidos y con ejemplos directos.

---

## 1. ¿Qué es Blazor Server?

Blazor Server es un framework de Microsoft para construir interfaces web usando **C#** en lugar de JavaScript. El código C# se ejecuta en el servidor y la página del navegador se actualiza en tiempo real mediante una conexión **SignalR** (WebSocket).

```
Navegador  ←──SignalR──→  Servidor (.NET)
   (HTML)                  (C# + Razor)
```

- El navegador solo muestra HTML/CSS.
- Toda la lógica vive en el servidor.
- Cada clic del usuario viaja al servidor, se procesa, y el servidor envía de vuelta solo el fragmento de HTML que cambió.

---

## 2. Componentes (.razor)

En Blazor, todo es un **componente**. Un componente es un archivo `.razor` que combina HTML con C#.

```razor
@* Esto es un componente *@
<h3>Hola, @nombre</h3>

@code {
    private string nombre = "Mundo";
}
```

- La parte de arriba es **markup** (HTML + sintaxis Razor).
- El bloque `@code { }` contiene la **lógica en C#**: variables, métodos, propiedades.
- `@nombre` inserta el valor de la variable directamente en el HTML.

**Tipos de componentes en este proyecto:**

| Tipo | Ubicación | Ejemplo |
|------|-----------|---------|
| Páginas | `Components/Pages/` | `Producto.razor` |
| Layout | `Components/Layout/` | `MainLayout.razor` |
| Raíz | `Components/` | `App.razor`, `Routes.razor` |

---

## 3. Directivas principales

Las directivas son instrucciones especiales que van al inicio del archivo `.razor`:

```razor
@page "/producto"              @* Define la ruta URL de esta página *@
@inject ApiService Api         @* Inyecta un servicio para usarlo *@
@inject IJSRuntime JS          @* Inyecta acceso a JavaScript *@
@rendermode InteractiveServer  @* Activa interactividad del lado del servidor *@
@using MiProyecto.Services     @* Importa un namespace *@
```

| Directiva | Función |
|-----------|---------|
| `@page` | Registra el componente como página accesible por URL |
| `@inject` | Inyecta un servicio registrado en `Program.cs` |
| `@rendermode` | Define el modo de renderizado (InteractiveServer para Blazor Server) |
| `@using` | Importa namespaces de C# |

---

## 4. Sintaxis Razor: mezclar C# y HTML

Razor permite escribir C# directamente dentro del HTML usando `@`:

### Expresiones
```razor
<p>El producto es: @nombreProducto</p>
<p>Total: @(precio * cantidad)</p>
```

### Condicionales
```razor
@if (cargando)
{
    <p>Cargando...</p>
}
else if (registros.Count == 0)
{
    <p>No hay datos.</p>
}
else
{
    <table>...</table>
}
```

### Bucles
```razor
@foreach (var item in registros)
{
    <tr>
        <td>@item["codigo"]</td>
        <td>@item["nombre"]</td>
    </tr>
}
```

---

## 5. Data Binding (enlace de datos)

El binding conecta una variable de C# con un elemento del HTML. Cuando uno cambia, el otro se actualiza.

### One-way (solo lectura)
```razor
<p>@mensaje</p>
```
La variable `mensaje` se muestra, pero el HTML no la modifica.

### Two-way con @bind
```razor
<input @bind="campoCodigo" />
```
- Si el usuario escribe en el input, `campoCodigo` se actualiza.
- Si el código cambia `campoCodigo`, el input se actualiza.

### Variantes de @bind
```razor
<input @bind="nombre" />                          @* Texto *@
<input type="number" @bind="cantidad" />           @* Entero *@
<input type="number" step="0.01" @bind="precio" /> @* Decimal *@
<select @bind="rolSeleccionado">                   @* Dropdown *@
    <option value="">-- Seleccione --</option>
    <option value="1">Admin</option>
</select>
```

---

## 6. Eventos

Los eventos conectan acciones del usuario con métodos de C#:

```razor
<button @onclick="Guardar">Guardar</button>
<button @onclick="() => Editar(item)">Editar</button>
<button @onclick="() => Eliminar(codigo)">Eliminar</button>
```

| Evento | Se dispara cuando... |
|--------|----------------------|
| `@onclick` | El usuario hace clic |
| `@onchange` | El valor de un input cambia |
| `@onsubmit` | Se envía un formulario |
| `@oninput` | El usuario escribe (en tiempo real) |

Los métodos pueden ser `async`:
```razor
<button @onclick="GuardarAsync">Guardar</button>

@code {
    private async Task GuardarAsync()
    {
        await Api.CrearAsync("producto", datos);
    }
}
```

---

## 7. Ciclo de vida de un componente

Cada componente pasa por etapas. Las más usadas:

```csharp
@code {
    // Se ejecuta cuando el componente se carga por primera vez
    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
    }
}
```

| Método | Cuándo se ejecuta |
|--------|-------------------|
| `OnInitializedAsync()` | Una vez, al cargar el componente |
| `OnParametersSetAsync()` | Cuando los parámetros del componente cambian |
| `OnAfterRenderAsync(bool first)` | Después de cada renderizado del HTML |

En este proyecto, `OnInitializedAsync` se usa en todas las páginas para cargar los datos iniciales.

---

## 8. Inyección de dependencias (DI)

Los servicios se registran en `Program.cs` y se inyectan en los componentes con `@inject`.

**Registro (Program.cs):**
```csharp
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<SpService>();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5035")
});
```

**Uso en un componente:**
```razor
@inject ApiService Api

@code {
    private async Task CargarDatos()
    {
        registros = await Api.ListarAsync("producto");
    }
}
```

`AddScoped` significa: una instancia por conexión de usuario. Cada usuario que abre la app tiene su propia instancia del servicio.

---

## 9. HttpClient y consumo de APIs

`HttpClient` es la clase de .NET para hacer peticiones HTTP. En este proyecto, `ApiService` lo usa internamente:

```
Componente  →  ApiService  →  HttpClient  →  API REST (puerto 5035)
```

Las operaciones CRUD se mapean a verbos HTTP:

| Operación | Verbo HTTP | Endpoint |
|-----------|------------|----------|
| Listar | GET | `/api/{tabla}` |
| Crear | POST | `/api/{tabla}` |
| Actualizar | PUT | `/api/{tabla}/{clave}/{valor}` |
| Eliminar | DELETE | `/api/{tabla}/{clave}/{valor}` |

---

## 10. async / await

Las operaciones que toman tiempo (llamadas HTTP, acceso a BD) son **asíncronas**. En C# se manejan con `async` y `await`:

```csharp
// SIN async: la página se congela hasta que termine
var datos = Api.ListarAsync("producto").Result; // MAL

// CON async: la página sigue respondiendo
var datos = await Api.ListarAsync("producto");  // BIEN
```

Reglas:
- El método debe retornar `Task` o `Task<T>`.
- Se marca con `async`.
- Se usa `await` antes de cada llamada asíncrona.

```csharp
private async Task CargarProductos()
{
    cargando = true;
    registros = await Api.ListarAsync("producto");
    cargando = false;
}
```

---

## 11. Dictionary<string, object?> como modelo de datos

En lugar de crear una clase por cada tabla, este proyecto usa diccionarios genéricos:

```csharp
// Una fila de la tabla producto:
// { "codigo": "P001", "nombre": "Laptop", "precio": 999.99 }

var fila = registros[0];
string codigo = fila["codigo"]?.ToString() ?? "";
```

Esto permite que un solo `ApiService` funcione con **cualquier tabla** sin cambiar código.

**Acceder a valores:**
```csharp
var valor = registro["nombreColumna"]?.ToString();
```

**Crear un registro para enviar:**
```csharp
var datos = new Dictionary<string, object?>
{
    { "codigo", campoCodigo },
    { "nombre", campoNombre },
    { "precio", campoPrecio }
};
await Api.CrearAsync("producto", datos);
```

---

## 12. Renderizado condicional de UI

Las páginas alternan entre estados usando variables booleanas:

```razor
@if (mostrarFormulario)
{
    <div class="card">
        @* Formulario de creación/edición *@
    </div>
}

@if (cargando)
{
    <div class="spinner-border"></div>
}
else
{
    <table>
        @* Tabla de datos *@
    </table>
}
```

Variables de estado típicas:
```csharp
private bool cargando = false;
private bool mostrarFormulario = false;
private bool editando = false;    // false = crear, true = editar
private string mensaje = "";
private bool exito = true;
```

---

## 13. Interop con JavaScript (IJSRuntime)

Blazor puede llamar funciones de JavaScript cuando es necesario. En este proyecto se usa para diálogos de confirmación:

```csharp
@inject IJSRuntime JS

@code {
    private async Task Eliminar(string codigo)
    {
        bool confirmado = await JS.InvokeAsync<bool>("confirm", "¿Eliminar este registro?");
        if (confirmado)
        {
            await Api.EliminarAsync("producto", "codigo", codigo);
        }
    }
}
```

`confirm` es una función nativa de JavaScript que muestra un diálogo con Aceptar/Cancelar.

---

## 14. Layout y navegación

El layout define la estructura visual que envuelve todas las páginas:

```
┌─────────────────────────────────────┐
│  NavMenu (sidebar izquierdo)        │
│  ┌─────────────────────────────┐    │
│  │                             │    │
│  │     @Body (contenido de     │    │
│  │     la página actual)       │    │
│  │                             │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

- `MainLayout.razor`: define la estructura (sidebar + contenido).
- `NavMenu.razor`: contiene los links de navegación.
- `@Body`: se reemplaza con el contenido de la página que corresponda a la URL.

**Navegación:**
```razor
<NavLink class="nav-link" href="producto">
    Productos
</NavLink>
```
`NavLink` es como un `<a>` pero agrega la clase `active` automáticamente cuando la URL coincide.

---

## 15. Estructura de un proyecto Blazor Server

```
MiProyecto/
├── Program.cs                    ← Punto de entrada, configura servicios
├── appsettings.json              ← Configuración (URL de API, etc.)
├── MiProyecto.csproj             ← Archivo del proyecto (.NET 9)
├── Components/
│   ├── App.razor                 ← Documento HTML raíz
│   ├── Routes.razor              ← Configuración del router
│   ├── _Imports.razor            ← Usings globales para todos los .razor
│   ├── Layout/
│   │   ├── MainLayout.razor      ← Estructura visual principal
│   │   └── NavMenu.razor         ← Menú de navegación
│   └── Pages/
│       ├── Home.razor            ← Página de inicio
│       └── *.razor               ← Demás páginas
├── Services/
│   └── *.cs                      ← Servicios (lógica de negocio)
└── wwwroot/
    ├── app.css                   ← Estilos personalizados
    └── lib/                      ← Librerías CSS/JS (Bootstrap)
```

---

## 16. Bootstrap en Blazor

Bootstrap es el framework CSS que da estilo a la aplicación. No requiere configuración especial en Blazor, solo se referencia en `App.razor`:

```html
<link href="lib/bootstrap/css/bootstrap.min.css" rel="stylesheet" />
```

Clases más usadas en este proyecto:

| Clase | Efecto |
|-------|--------|
| `container`, `mt-4` | Contenedor con margen superior |
| `btn btn-primary` | Botón azul |
| `btn btn-danger` | Botón rojo |
| `table table-striped` | Tabla con filas alternadas |
| `form-control` | Input estilizado |
| `form-select` | Dropdown estilizado |
| `alert alert-success` | Mensaje verde de éxito |
| `alert alert-danger` | Mensaje rojo de error |
| `spinner-border` | Indicador de carga giratorio |
| `card`, `card-body` | Tarjeta con borde y padding |

---

## Resumen de flujo completo

```
1. Usuario abre http://localhost:5200/producto
2. Router detecta @page "/producto" → carga Producto.razor
3. OnInitializedAsync() → llama Api.ListarAsync("producto")
4. ApiService hace GET http://localhost:5035/api/producto
5. API responde con JSON → se parsea a List<Dictionary>
6. Tabla se renderiza con @foreach
7. Usuario llena formulario → @bind actualiza variables
8. Clic en Guardar → @onclick llama GuardarRegistro()
9. Se construye Dictionary y se envía con Api.CrearAsync()
10. Se muestra mensaje de éxito/error y se recarga la tabla
```

---

> **Siguiente paso:** Paso 2 — Crear el proyecto desde cero y configurar la conexión a la API.
