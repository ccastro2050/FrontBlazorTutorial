# Paso 4 — Configurar la Conexión a la API y Crear el ApiService

Este paso conecta el frontend Blazor con la API REST (ApiGenericaCsharp) que corre en el puerto 5035.

---

## 1. Configurar la URL de la API

Abrir el archivo `appsettings.json` (está en la raíz del proyecto) y agregar la línea `ApiBaseUrl`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApiBaseUrl": "http://localhost:5035"
}
```

La línea nueva es:
```json
"ApiBaseUrl": "http://localhost:5035"
```

Esto le dice al proyecto dónde está la API. Si la API corre en otro puerto, se cambia aquí.

---

## 2. Modificar Program.cs

`Program.cs` es el punto de entrada de la aplicación. Aquí se registran los servicios que van a estar disponibles en todo el proyecto.

Abrir `Program.cs` y reemplazar todo su contenido con:

```csharp
using FrontBlazorTutorial.Components;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar HttpClient para conectarse a la API
// La URL base se lee de appsettings.json
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5035";
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// Registrar los servicios de la API
builder.Services.AddScoped<FrontBlazorTutorial.Services.ApiService>();

var app = builder.Build();

// Configurar el pipeline HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

**¿Qué hace cada bloque?**

| Línea | Función |
|-------|---------|
| `AddRazorComponents()` | Habilita componentes Razor (.razor) |
| `AddInteractiveServerComponents()` | Activa el modo interactivo del servidor (SignalR) |
| `builder.Configuration["ApiBaseUrl"]` | Lee la URL de la API desde `appsettings.json` |
| `AddScoped<HttpClient>` | Registra HttpClient con la URL base de la API |
| `AddScoped<ApiService>` | Registra el servicio que van a usar las páginas |
| `UseAntiforgery()` | Protección contra ataques CSRF |
| `MapStaticAssets()` | Sirve archivos estáticos (CSS, JS, imágenes) |
| `MapRazorComponents<App>()` | Configura el componente raíz de la aplicación |

---

## 3. Crear la carpeta Services

Crear una carpeta `Services` en la raíz del proyecto:

```powershell
mkdir Services
```

---

## 4. Crear ApiService.cs

Crear el archivo `Services/ApiService.cs` con el siguiente contenido:

```csharp
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontBlazorTutorial.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        // ──────────────────────────────────────────────
        // LISTAR: GET /api/{tabla}
        // ──────────────────────────────────────────────
        public async Task<List<Dictionary<string, object?>>> ListarAsync(
            string tabla, int? limite = null)
        {
            try
            {
                string url = $"/api/{tabla}";
                if (limite.HasValue)
                    url += $"?limite={limite.Value}";

                var respuesta = await _http.GetFromJsonAsync<JsonElement>(url, _jsonOptions);

                if (respuesta.TryGetProperty("datos", out JsonElement datos))
                {
                    return ConvertirDatos(datos);
                }

                return new List<Dictionary<string, object?>>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al listar {tabla}: {ex.Message}");
                return new List<Dictionary<string, object?>>();
            }
        }

        // ──────────────────────────────────────────────
        // CREAR: POST /api/{tabla}
        // ──────────────────────────────────────────────
        public async Task<(bool exito, string mensaje)> CrearAsync(
            string tabla, Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            try
            {
                string url = $"/api/{tabla}";
                if (!string.IsNullOrEmpty(camposEncriptar))
                    url += $"?camposEncriptar={camposEncriptar}";

                var respuesta = await _http.PostAsJsonAsync(url, datos);
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(
                    _jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        // ACTUALIZAR: PUT /api/{tabla}/{clave}/{valor}
        // ──────────────────────────────────────────────
        public async Task<(bool exito, string mensaje)> ActualizarAsync(
            string tabla, string nombreClave, string valorClave,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            try
            {
                string url = $"/api/{tabla}/{nombreClave}/{valorClave}";
                if (!string.IsNullOrEmpty(camposEncriptar))
                    url += $"?camposEncriptar={camposEncriptar}";

                var respuesta = await _http.PutAsJsonAsync(url, datos);
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(
                    _jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        // ELIMINAR: DELETE /api/{tabla}/{clave}/{valor}
        // ──────────────────────────────────────────────
        public async Task<(bool exito, string mensaje)> EliminarAsync(
            string tabla, string nombreClave, string valorClave)
        {
            try
            {
                var respuesta = await _http.DeleteAsync(
                    $"/api/{tabla}/{nombreClave}/{valorClave}");
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(
                    _jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        // DIAGNOSTICO: GET /api/diagnostico/conexión
        // ──────────────────────────────────────────────
        public async Task<Dictionary<string, string>?> ObtenerDiagnosticoAsync()
        {
            try
            {
                var respuesta = await _http.GetFromJsonAsync<JsonElement>(
                    "/api/diagnostico/conexión", _jsonOptions);

                if (respuesta.TryGetProperty("servidor", out JsonElement servidor))
                {
                    var info = new Dictionary<string, string>();
                    foreach (var prop in servidor.EnumerateObject())
                    {
                        info[prop.Name] = prop.Value.ToString();
                    }
                    return info;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // ──────────────────────────────────────────────
        // Convierte JsonElement a lista de diccionarios
        // ──────────────────────────────────────────────
        private List<Dictionary<string, object?>> ConvertirDatos(JsonElement datos)
        {
            var lista = new List<Dictionary<string, object?>>();

            foreach (var fila in datos.EnumerateArray())
            {
                var diccionario = new Dictionary<string, object?>();

                foreach (var propiedad in fila.EnumerateObject())
                {
                    diccionario[propiedad.Name] = propiedad.Value.ValueKind switch
                    {
                        JsonValueKind.String => propiedad.Value.GetString(),
                        JsonValueKind.Number => propiedad.Value.TryGetInt32(out int i)
                            ? i : propiedad.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => propiedad.Value.GetRawText()
                    };
                }

                lista.Add(diccionario);
            }

            return lista;
        }
    }
}
```

**¿Qué hace cada método?**

| Método | Verbo HTTP | Endpoint | Función |
|--------|------------|----------|---------|
| `ListarAsync("producto")` | GET | `/api/producto` | Trae todos los registros de la tabla |
| `CrearAsync("producto", datos)` | POST | `/api/producto` | Inserta un registro nuevo |
| `ActualizarAsync("producto", "codigo", "P001", datos)` | PUT | `/api/producto/codigo/P001` | Modifica un registro existente |
| `EliminarAsync("producto", "codigo", "P001")` | DELETE | `/api/producto/codigo/P001` | Elimina un registro |
| `ObtenerDiagnosticoAsync()` | GET | `/api/diagnostico/conexión` | Información de la conexión a BD |

**¿Qué es `ConvertirDatos`?**

La API devuelve JSON. Este método lo transforma a `List<Dictionary<string, object?>>` para que las páginas Blazor puedan recorrerlo fácilmente con `@foreach`.

Ejemplo de lo que devuelve la API:
```json
{
  "datos": [
    { "codigo": "P001", "nombre": "Laptop", "precio": 999.99 },
    { "codigo": "P002", "nombre": "Mouse", "precio": 25.50 }
  ]
}
```

Después de `ConvertirDatos`, se accede así en C#:
```csharp
var nombre = registros[0]["nombre"]?.ToString();  // "Laptop"
```

---

## 5. Verificar que compila

```bash
dotnet build
```

Si muestra **"Build succeeded"**, todo está correcto.

---

## 6. Estructura del proyecto hasta ahora

```
FrontBlazorTutorial/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   ├── Pages/
│   │   ├── Counter.razor      ← se puede eliminar después
│   │   ├── Error.razor
│   │   ├── Home.razor
│   │   └── Weather.razor      ← se puede eliminar después
│   ├── App.razor
│   ├── Routes.razor
│   └── _Imports.razor
├── Services/
│   └── ApiService.cs          ← NUEVO
├── Program.cs                 ← MODIFICADO
├── appsettings.json           ← MODIFICADO
└── ...
```

---

## 7. Subir los cambios (como Estudiante 1)

```bash
git add .
git commit -m "Agregar ApiService y configurar conexión a la API"
git push
```

---

> **Siguiente paso:** Paso 5 — Configurar el Layout, la navegación y crear la página Home.
