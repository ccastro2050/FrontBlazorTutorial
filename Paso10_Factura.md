# Paso 10 — Factura con Stored Procedures (Maestro-Detalle)

Factura es la página más compleja del proyecto. A diferencia de los CRUDs anteriores (que usan la API genérica), Factura usa **Stored Procedures (SPs)** porque una factura es una operación **maestro-detalle**: una factura tiene muchos productos.

Este paso se divide en **3 tareas con dependencias**:

| Orden | Estudiante | Tarea | Rama | Depende de |
|-------|------------|-------|------|------------|
| 1ro | **Estudiante 1** | Crear `SpService.cs` + registrar en `Program.cs` | `sp-service` | Nada |
| 2do | **Estudiante 3** | Actualizar `Home.razor` con todas las tablas | `actualizar-home` | Nada |
| 3ro | **Estudiante 2** | Crear `Factura.razor` | `crud-factura` | SpService (Est1) |

**Importante:** Estudiante 2 debe esperar a que el PR de Estudiante 1 esté mergeado antes de subir su PR, porque Factura.razor usa SpService.

Estudiante 1 y Estudiante 3 pueden trabajar en paralelo.

---

## Antes de empezar

Cada estudiante actualiza main y crea su rama:

```bash
git checkout main
git pull
git checkout -b nombre-de-la-rama
```

---

## Estudiante 1 — Crear SpService

SpService es un servicio similar a ApiService, pero en lugar de hacer CRUD genérico, ejecuta **Stored Procedures** en la base de datos a través de la API.

### 1. Crear el archivo `Services/SpService.cs`

```csharp
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontBlazorTutorial.Services
{
    public class SpService
    {
        private readonly HttpClient _http;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SpService(HttpClient http)
        {
            _http = http;
        }

        public async Task<(bool exito, List<Dictionary<string, object?>> resultados, string mensaje)>
            EjecutarSpAsync(string nombreSP, Dictionary<string, object?>? parametros = null)
        {
            try
            {
                var payload = new Dictionary<string, object?> { ["nombreSP"] = nombreSP };
                if (parametros != null)
                {
                    foreach (var kvp in parametros)
                        payload[kvp.Key] = kvp.Value;
                }

                var respuesta = await _http.PostAsJsonAsync("/api/procedimientos/ejecutarsp", payload);
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? ""
                    : contenido.TryGetProperty("Mensaje", out JsonElement msg2)
                        ? msg2.GetString() ?? ""
                        : "";

                if (!respuesta.IsSuccessStatusCode)
                {
                    string detalle = contenido.TryGetProperty("detalle", out JsonElement det)
                        ? det.GetString() ?? mensaje
                        : mensaje;
                    return (false, new(), detalle);
                }

                var resultados = new List<Dictionary<string, object?>>();

                JsonElement datosArray;
                if (contenido.TryGetProperty("resultados", out datosArray) ||
                    contenido.TryGetProperty("Resultados", out datosArray))
                {
                    if (datosArray.ValueKind == JsonValueKind.Array)
                    {
                        resultados = ConvertirDatos(datosArray);
                    }
                }

                return (true, resultados, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, new(), $"Error de conexion: {ex.Message}");
            }
        }

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
                        JsonValueKind.Number => propiedad.Value.TryGetInt32(out int i) ? i : propiedad.Value.GetDouble(),
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

### 2. ¿Qué hace SpService?

| Concepto | Explicación |
|----------|-------------|
| `EjecutarSpAsync` | Envía el nombre del SP y sus parámetros a la API |
| Endpoint | `POST /api/procedimientos/ejecutarsp` |
| Payload | `{ "nombreSP": "sp_listar_facturas", "p_resultado": null }` |
| Retorno | Una tupla con: `exito` (bool), `resultados` (lista de filas), `mensaje` (string) |
| `ConvertirDatos` | Convierte el JSON de respuesta a una lista de diccionarios (igual que ApiService) |

### 3. Registrar SpService en Program.cs

Abrir `Program.cs` y agregar una línea después del registro de ApiService:

**Antes:**
```csharp
builder.Services.AddScoped<FrontBlazorTutorial.Services.ApiService>();
```

**Después:**
```csharp
builder.Services.AddScoped<FrontBlazorTutorial.Services.ApiService>();
builder.Services.AddScoped<FrontBlazorTutorial.Services.SpService>();
```

Esta línea le dice a Blazor que SpService existe y puede inyectarse con `@inject`.

### 4. Verificar, subir y crear PR

```bash
dotnet build
git add .
git commit -m "Agregar SpService para stored procedures"
git push -u origin sp-service
```

En GitHub: crear Pull Request `sp-service` → `main`, aprobar y hacer merge.
Después del merge, clic en **Delete branch**.

---

## Estudiante 3 — Actualizar Home.razor

Actualizar la página Home para que muestre la lista completa de tablas disponibles (ahora incluye Cliente, Vendedor y Factura).

### 1. Modificar `Components/Pages/Home.razor`

Buscar la línea que dice:

```razor
<strong>Tablas disponibles:</strong> Producto, Persona, Usuario, Empresa, Rol, Ruta.
```

Y cambiarla por:

```razor
<strong>Tablas disponibles:</strong> Producto, Persona, Usuario, Empresa, Rol, Ruta, Cliente, Vendedor, Factura.
```

### 2. Verificar, subir y crear PR

```bash
dotnet build
git add .
git commit -m "Actualizar Home con lista completa de tablas"
git push -u origin actualizar-home
```

En GitHub: crear Pull Request `actualizar-home` → `main`, aprobar y hacer merge.
Después del merge, clic en **Delete branch**.

---

## Estudiante 2 — Crear Factura.razor

**Requisito:** el PR de SpService (Estudiante 1) debe estar mergeado antes de empezar. Actualizar la rama:

```bash
git fetch origin
git merge origin/main
```

### ¿Por qué Factura es diferente?

Los CRUDs anteriores operan sobre **una sola tabla**. Factura es **maestro-detalle**:
- **Maestro:** la factura (número, cliente, vendedor, fecha)
- **Detalle:** los productos de esa factura (código, cantidad)

Esto requiere Stored Procedures porque en una sola operación se debe:
- Crear la factura Y agregar sus productos
- Consultar la factura CON sus productos
- Eliminar los productos Y la factura

### Conceptos nuevos en esta página

| Concepto | Explicación |
|----------|-------------|
| `@inject SpService Sp` | Se inyecta el nuevo servicio de SPs además del ApiService |
| `vista` (string) | En lugar de `mostrarFormulario` (bool), se usa un string para manejar 3 vistas: "listar", "ver", "formulario" |
| `filasProductos` | Lista dinámica de productos que se pueden agregar/quitar del formulario |
| Clases auxiliares | `ProductoFila`, `ClienteInfo`, `VendedorInfo` — para organizar los datos de los selects |
| JSON con `JsonSerializer` | Los productos se envían al SP como un string JSON |
| `AplanarFacturaJson` | El SP devuelve JSON anidado que hay que convertir a diccionario |

### 1. Crear el archivo `Components/Pages/Factura.razor`

```razor
@page "/factura"
@rendermode InteractiveServer
@inject FrontBlazorTutorial.Services.ApiService Api
@inject FrontBlazorTutorial.Services.SpService Sp
@inject IJSRuntime JS
@using System.Text.Json

<PageTitle>Facturas</PageTitle>

<div class="container mt-4">
    <h3>Facturas</h3>

    @* ───────── ALERTA ───────── *@
    @if (!string.IsNullOrEmpty(mensaje))
    {
        <div class="alert @(exito ? "alert-success" : "alert-danger") alert-dismissible fade show">
            @mensaje
            <button type="button" class="btn-close" @onclick="() => mensaje = string.Empty"></button>
        </div>
    }

    @* ═══════════════════════════════════════ *@
    @* VISTA: LISTAR FACTURAS                 *@
    @* ═══════════════════════════════════════ *@
    @if (vista == "listar")
    {
        <button class="btn btn-primary mb-3" @onclick="MostrarFormularioNueva">Nueva Factura</button>

        @if (cargando)
        {
            <div class="d-flex justify-content-center my-4">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Cargando...</span>
                </div>
            </div>
        }
        else if (facturas.Any())
        {
            <table class="table table-striped table-hover">
                <thead class="table-dark">
                    <tr>
                        <th>Numero</th>
                        <th>Cliente</th>
                        <th>Vendedor</th>
                        <th>Fecha</th>
                        <th>Total</th>
                        <th>Productos</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var f in facturas)
                    {
                        <tr>
                            <td>@ObtenerValor(f, "numero")</td>
                            <td>@ObtenerValor(f, "nombre_cliente")</td>
                            <td>@ObtenerValor(f, "nombre_vendedor")</td>
                            <td>@FormatearFecha(ObtenerValor(f, "fecha"))</td>
                            <td>@ObtenerValor(f, "total")</td>
                            <td>@ObtenerProductos(f).Count</td>
                            <td>
                                <button class="btn btn-info btn-sm me-1"
                                        @onclick="() => VerFactura(f)">Ver</button>
                                <button class="btn btn-warning btn-sm me-1"
                                        @onclick="() => EditarFactura(f)">Editar</button>
                                <button class="btn btn-danger btn-sm"
                                        @onclick="() => EliminarFactura(f)">Eliminar</button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        }
        else
        {
            <div class="alert alert-warning">No se encontraron facturas.</div>
        }
    }

    @* ═══════════════════════════════════════ *@
    @* VISTA: VER DETALLE DE FACTURA          *@
    @* ═══════════════════════════════════════ *@
    @if (vista == "ver" && facturaActual != null)
    {
        <button class="btn btn-secondary mb-3 me-2" @onclick="Volver">Volver</button>
        <button class="btn btn-warning mb-3" @onclick="() => EditarFactura(facturaActual)">Editar</button>

        <div class="card mb-3">
            <div class="card-header"><strong>Factura #@ObtenerValor(facturaActual, "numero")</strong></div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6"><strong>Cliente:</strong> @ObtenerValor(facturaActual, "nombre_cliente")</div>
                    <div class="col-md-6"><strong>Vendedor:</strong> @ObtenerValor(facturaActual, "nombre_vendedor")</div>
                    <div class="col-md-6"><strong>Fecha:</strong> @FormatearFecha(ObtenerValor(facturaActual, "fecha"))</div>
                    <div class="col-md-6"><strong>Total:</strong> @ObtenerValor(facturaActual, "total")</div>
                </div>
            </div>
        </div>

        @if (ObtenerProductos(facturaActual).Any())
        {
            <h5>Productos</h5>
            <table class="table table-striped">
                <thead class="table-dark">
                    <tr>
                        <th>Codigo</th>
                        <th>Nombre</th>
                        <th>Cantidad</th>
                        <th>Valor Unitario</th>
                        <th>Subtotal</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var p in ObtenerProductos(facturaActual))
                    {
                        <tr>
                            <td>@ObtenerValor(p, "codigo_producto")</td>
                            <td>@ObtenerValor(p, "nombre_producto")</td>
                            <td>@ObtenerValor(p, "cantidad")</td>
                            <td>@ObtenerValor(p, "valorunitario")</td>
                            <td>@ObtenerValor(p, "subtotal")</td>
                        </tr>
                    }
                </tbody>
            </table>
        }
    }

    @* ═══════════════════════════════════════ *@
    @* VISTA: FORMULARIO (CREAR / EDITAR)     *@
    @* ═══════════════════════════════════════ *@
    @if (vista == "formulario")
    {
        <div class="card mb-3">
            <div class="card-header">
                @(editando ? $"Editar Factura #{numeroEditar}" : "Nueva Factura")
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Cliente</label>
                        <select class="form-select" @bind="campoCliente">
                            <option value="0">-- Seleccione --</option>
                            @foreach (var cli in clientes)
                            {
                                <option value="@cli.Id">@cli.Nombre (Credito: $@cli.Credito)</option>
                            }
                        </select>
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Vendedor</label>
                        <select class="form-select" @bind="campoVendedor">
                            <option value="0">-- Seleccione --</option>
                            @foreach (var ven in vendedores)
                            {
                                <option value="@ven.Id">@ven.Nombre (Carnet: @ven.Carnet)</option>
                            }
                        </select>
                    </div>
                </div>

                <h5 class="mt-3">Productos</h5>
                @for (int i = 0; i < filasProductos.Count; i++)
                {
                    var idx = i;
                    <div class="row mb-2 align-items-end">
                        <div class="col-md-6">
                            <label class="form-label">Producto</label>
                            <select class="form-select" @bind="filasProductos[idx].Codigo">
                                <option value="">-- Seleccione --</option>
                                @foreach (var prod in productosDisponibles)
                                {
                                    <option value="@prod["codigo"]">
                                        @prod["nombre"] (Stock: @prod["stock"] - $@prod["valorunitario"])
                                    </option>
                                }
                            </select>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Cantidad</label>
                            <input class="form-control" type="number" min="1" @bind="filasProductos[idx].Cantidad" />
                        </div>
                        <div class="col-md-3">
                            @if (filasProductos.Count > 1)
                            {
                                <button class="btn btn-outline-danger" @onclick="() => QuitarFila(idx)">Quitar</button>
                            }
                        </div>
                    </div>
                }

                <button class="btn btn-outline-primary mb-3" @onclick="AgregarFila">+ Agregar Producto</button>

                <div>
                    <button class="btn btn-success me-2" @onclick="GuardarFactura">Guardar</button>
                    <button class="btn btn-secondary" @onclick="Volver">Cancelar</button>
                </div>
            </div>
        </div>
    }
</div>

@code {
    private string vista = "listar";
    private bool cargando = true;
    private string mensaje = string.Empty;
    private bool exito = false;

    private List<Dictionary<string, object?>> facturas = new();
    private Dictionary<string, object?>? facturaActual;

    private bool editando = false;
    private int numeroEditar = 0;
    private int campoCliente = 0;
    private int campoVendedor = 0;
    private List<ProductoFila> filasProductos = new() { new() };

    private List<ClienteInfo> clientes = new();
    private List<VendedorInfo> vendedores = new();
    private List<Dictionary<string, object?>> productosDisponibles = new();

    // ───────── CLASES AUXILIARES ─────────
    private class ProductoFila
    {
        public string Codigo { get; set; } = "";
        public int Cantidad { get; set; } = 1;
    }

    private class ClienteInfo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Credito { get; set; } = "0";
    }

    private class VendedorInfo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Carnet { get; set; } = "";
    }

    // ───────── CICLO DE VIDA ─────────
    protected override async Task OnInitializedAsync()
    {
        await CargarFacturas();
    }

    // ───────── CARGAR FACTURAS ─────────
    private async Task CargarFacturas()
    {
        cargando = true;
        facturas = new();

        var parametros = new Dictionary<string, object?> { ["p_resultado"] = null };
        var (ok, resultados, msg) = await Sp.EjecutarSpAsync("sp_listar_facturas_y_productosporfactura", parametros);

        if (ok && resultados.Count > 0)
        {
            var jsonStr = resultados[0].Values.FirstOrDefault()?.ToString();
            if (!string.IsNullOrEmpty(jsonStr))
            {
                try
                {
                    facturas = ParsearJsonArray(jsonStr);
                }
                catch
                {
                    facturas = resultados;
                }
            }
        }

        cargando = false;
    }

    // ───────── CARGAR DATOS PARA SELECTS ─────────
    private async Task CargarDatosFormulario()
    {
        var rawClientes = await Api.ListarAsync("cliente");
        var rawVendedores = await Api.ListarAsync("vendedor");
        var personas = await Api.ListarAsync("persona");
        productosDisponibles = await Api.ListarAsync("producto");

        var mapaPersonas = personas.ToDictionary(
            p => p["codigo"]?.ToString() ?? "",
            p => p["nombre"]?.ToString() ?? "Sin nombre"
        );

        clientes = rawClientes.Select(c => new ClienteInfo
        {
            Id = int.TryParse(c["id"]?.ToString(), out int id) ? id : 0,
            Nombre = mapaPersonas.GetValueOrDefault(c["fkcodpersona"]?.ToString() ?? "", "Sin nombre"),
            Credito = c["credito"]?.ToString() ?? "0"
        }).ToList();

        vendedores = rawVendedores.Select(v => new VendedorInfo
        {
            Id = int.TryParse(v["id"]?.ToString(), out int id) ? id : 0,
            Nombre = mapaPersonas.GetValueOrDefault(v["fkcodpersona"]?.ToString() ?? "", "Sin nombre"),
            Carnet = v["carnet"]?.ToString() ?? ""
        }).ToList();
    }

    // ───────── ACCIONES DE VISTA ─────────
    private async Task MostrarFormularioNueva()
    {
        editando = false;
        numeroEditar = 0;
        campoCliente = 0;
        campoVendedor = 0;
        filasProductos = new() { new() };
        mensaje = string.Empty;
        await CargarDatosFormulario();
        vista = "formulario";
    }

    private async Task VerFactura(Dictionary<string, object?> f)
    {
        int numero = int.TryParse(ObtenerValor(f, "numero"), out int n) ? n : 0;

        var parametros = new Dictionary<string, object?>
        {
            ["p_numero"] = numero,
            ["p_resultado"] = null
        };
        var (ok, resultados, msg) = await Sp.EjecutarSpAsync("sp_consultar_factura_y_productosporfactura", parametros);

        if (ok && resultados.Count > 0)
        {
            var jsonStr = resultados[0].Values.FirstOrDefault()?.ToString();
            if (!string.IsNullOrEmpty(jsonStr))
            {
                try
                {
                    facturaActual = AplanarFacturaJson(jsonStr);
                }
                catch
                {
                    facturaActual = f;
                }
            }
            else
            {
                facturaActual = f;
            }
        }
        else
        {
            facturaActual = f;
        }

        vista = "ver";
    }

    private async Task EditarFactura(Dictionary<string, object?> f)
    {
        await CargarDatosFormulario();

        editando = true;
        numeroEditar = int.TryParse(ObtenerValor(f, "numero"), out int n) ? n : 0;
        campoCliente = int.TryParse(ObtenerValor(f, "fkidcliente"), out int c) ? c : 0;
        campoVendedor = int.TryParse(ObtenerValor(f, "fkidvendedor"), out int v) ? v : 0;

        var productos = ObtenerProductos(f);
        if (productos.Any())
        {
            filasProductos = productos.Select(p => new ProductoFila
            {
                Codigo = ObtenerValor(p, "codigo_producto"),
                Cantidad = int.TryParse(ObtenerValor(p, "cantidad"), out int cant) ? cant : 1
            }).ToList();
        }
        else
        {
            filasProductos = new() { new() };
        }

        mensaje = string.Empty;
        vista = "formulario";
    }

    private async Task EliminarFactura(Dictionary<string, object?> f)
    {
        int numero = int.TryParse(ObtenerValor(f, "numero"), out int n) ? n : 0;

        var confirmar = await JS.InvokeAsync<bool>("confirm", $"¿Está seguro de eliminar la Factura #{numero}?");
        if (!confirmar) return;

        var parametros = new Dictionary<string, object?>
        {
            ["p_numero"] = numero,
            ["p_resultado"] = null
        };
        var (ok, resultados, msg) = await Sp.EjecutarSpAsync("sp_borrar_factura_y_productosporfactura", parametros);

        exito = ok;
        mensaje = ok ? "Factura eliminada exitosamente." : $"Error al eliminar: {msg}";

        if (ok)
            await CargarFacturas();
    }

    // ───────── FORMULARIO ─────────
    private void AgregarFila()
    {
        filasProductos.Add(new ProductoFila());
    }

    private void QuitarFila(int index)
    {
        if (filasProductos.Count > 1)
            filasProductos.RemoveAt(index);
    }

    private async Task GuardarFactura()
    {
        var productosValidos = filasProductos
            .Where(p => !string.IsNullOrEmpty(p.Codigo) && p.Cantidad > 0)
            .Select(p => new { codigo = p.Codigo, cantidad = p.Cantidad })
            .ToList();

        if (!productosValidos.Any())
        {
            exito = false;
            mensaje = "Debe agregar al menos un producto.";
            return;
        }

        string jsonProductos = JsonSerializer.Serialize(productosValidos);

        if (editando)
        {
            var confirmar = await JS.InvokeAsync<bool>("confirm", $"¿Está seguro de actualizar la Factura #{numeroEditar}?");
            if (!confirmar) return;

            var parametros = new Dictionary<string, object?>
            {
                ["p_numero"] = numeroEditar,
                ["p_fkidcliente"] = campoCliente,
                ["p_fkidvendedor"] = campoVendedor,
                ["p_productos"] = jsonProductos,
                ["p_resultado"] = null
            };
            var (ok, _, msg) = await Sp.EjecutarSpAsync("sp_actualizar_factura_y_productosporfactura", parametros);
            exito = ok;
            mensaje = ok ? "Factura actualizada exitosamente." : $"Error: {msg}";
        }
        else
        {
            var parametros = new Dictionary<string, object?>
            {
                ["p_fkidcliente"] = campoCliente,
                ["p_fkidvendedor"] = campoVendedor,
                ["p_productos"] = jsonProductos,
                ["p_resultado"] = null
            };
            var (ok, _, msg) = await Sp.EjecutarSpAsync("sp_insertar_factura_y_productosporfactura", parametros);
            exito = ok;
            mensaje = ok ? "Factura creada exitosamente." : $"Error: {msg}";
        }

        if (exito)
        {
            await CargarFacturas();
            vista = "listar";
        }
    }

    private async Task Volver()
    {
        mensaje = string.Empty;
        await CargarFacturas();
        vista = "listar";
    }

    // ───────── HELPERS DE JSON ─────────
    private Dictionary<string, object?> AplanarFacturaJson(string json)
    {
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var resultado = new Dictionary<string, object?>();

        if (root.TryGetProperty("factura", out var factura) && factura.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in factura.EnumerateObject())
            {
                resultado[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString(),
                    JsonValueKind.Number => prop.Value.TryGetInt32(out int i) ? i : prop.Value.GetDouble(),
                    JsonValueKind.Null => null,
                    _ => prop.Value.GetRawText()
                };
            }
        }

        if (root.TryGetProperty("productos", out var productos))
        {
            resultado["productos"] = productos.GetRawText();
        }

        if (!resultado.Any())
        {
            resultado = ParsearJsonObject(root);
        }

        return resultado;
    }

    private string ObtenerValor(Dictionary<string, object?> dic, string clave)
    {
        if (dic.TryGetValue(clave, out var val) && val != null)
            return val.ToString() ?? "";
        return "";
    }

    private string FormatearFecha(string fecha)
    {
        if (DateTime.TryParse(fecha, out var dt))
            return dt.ToString("yyyy-MM-dd HH:mm");
        return fecha;
    }

    private List<Dictionary<string, object?>> ObtenerProductos(Dictionary<string, object?> factura)
    {
        if (factura.TryGetValue("productos", out var val) && val != null)
        {
            var strVal = val.ToString() ?? "";

            if (strVal.StartsWith("["))
            {
                try { return ParsearJsonArray(strVal); } catch { }
            }

            if (val is List<Dictionary<string, object?>> lista)
                return lista;
        }
        return new();
    }

    private List<Dictionary<string, object?>> ParsearJsonArray(string json)
    {
        var lista = new List<Dictionary<string, object?>>();
        var doc = JsonDocument.Parse(json);

        foreach (var elem in doc.RootElement.EnumerateArray())
        {
            lista.Add(ParsearJsonObject(elem));
        }
        return lista;
    }

    private Dictionary<string, object?> ParsearJsonObject(JsonElement elem)
    {
        var dic = new Dictionary<string, object?>();
        foreach (var prop in elem.EnumerateObject())
        {
            dic[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString(),
                JsonValueKind.Number => prop.Value.TryGetInt32(out int i) ? i : prop.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Array => prop.Value.GetRawText(),
                JsonValueKind.Object => prop.Value.GetRawText(),
                _ => prop.Value.GetRawText()
            };
        }
        return dic;
    }
}
```

### 2. Stored Procedures que usa Factura

| SP | Acción | Parámetros |
|----|--------|------------|
| `sp_listar_facturas_y_productosporfactura` | Listar todas | `p_resultado` (salida) |
| `sp_consultar_factura_y_productosporfactura` | Ver una | `p_numero`, `p_resultado` |
| `sp_insertar_factura_y_productosporfactura` | Crear | `p_fkidcliente`, `p_fkidvendedor`, `p_productos` (JSON), `p_resultado` |
| `sp_actualizar_factura_y_productosporfactura` | Editar | `p_numero`, `p_fkidcliente`, `p_fkidvendedor`, `p_productos` (JSON), `p_resultado` |
| `sp_borrar_factura_y_productosporfactura` | Eliminar | `p_numero`, `p_resultado` |

### 3. Verificar, subir y crear PR

```bash
dotnet build
git add .
git commit -m "Agregar pagina Factura con stored procedures"
git push -u origin crud-factura
```

En GitHub: crear Pull Request `crud-factura` → `main`, aprobar y hacer merge.
Después del merge, clic en **Delete branch**.

---

## Después de los tres merges

**Los tres estudiantes** actualizan su main:

```bash
git checkout main
git pull
```

El proyecto está completo con 9 páginas CRUD: Producto, Persona, Usuario, Empresa, Rol, Ruta, Cliente, Vendedor y Factura.

---

## Resumen final del proyecto

| Paso | Quién | Qué |
|------|-------|-----|
| 1-2 | Todos | Conceptos y Herramientas |
| 3 | Est1 crea repo, Est2 y Est3 clonan | Proyecto + GitHub |
| 4 | Est1 | ApiService + conexión API |
| 5 | Est1 | Layout, navegación, Home |
| 6 | Est1 | CRUD Producto |
| 7 | Est2 + Est3 | CRUD Persona + CRUD Usuario |
| 8 | Est1 + Est2 + Est3 | CRUD Empresa + Cliente + Rol |
| 9 | Est1 + Est2 + Est3 | CRUD Ruta + Vendedor + NavMenu |
| 10 | Est1 + Est2 + Est3 | SpService + Factura + Home |
