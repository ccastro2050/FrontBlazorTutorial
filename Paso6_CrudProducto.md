# Paso 6 — CRUD Producto

**Quién lo hace:** Estudiante 1

**Rama:** `crud-producto-pagina`

Este es el primer CRUD completo. Sirve como modelo para los demás. La página permite **Listar, Crear, Editar y Eliminar** productos.

---

## 1. Crear el archivo Producto.razor

Crear el archivo `Components/Pages/Producto.razor`.

Este archivo tiene dos partes: el **markup** (HTML con sintaxis Razor) y el bloque **@code** (lógica en C#). Se explican por separado.

---

## 2. Directivas (inicio del archivo)

```razor
@page "/producto"
@rendermode InteractiveServer
@inject FrontBlazorTutorial.Services.ApiService Api
@inject IJSRuntime JS
```

| Directiva | Función |
|-----------|---------|
| `@page "/producto"` | Esta página se accede en la URL `/producto` |
| `@rendermode InteractiveServer` | Activa la interactividad (botones, formularios) |
| `@inject ApiService Api` | Inyecta el servicio para llamar a la API |
| `@inject IJSRuntime JS` | Inyecta acceso a JavaScript (para `confirm`) |

---

## 3. Markup — Estructura visual

La página tiene 5 secciones visuales que se muestran o esconden según el estado:

```
┌─────────────────────────────────────┐
│  Alerta (mensaje de éxito/error)    │  ← solo si hay mensaje
├─────────────────────────────────────┤
│  Botón "Nuevo Producto"             │  ← solo si el formulario está oculto
├─────────────────────────────────────┤
│  Límite + botón Cargar              │  ← siempre visible
├─────────────────────────────────────┤
│  Formulario (Crear / Editar)        │  ← solo si mostrarFormulario == true
├─────────────────────────────────────┤
│  Spinner de carga                   │  ← solo si cargando == true
├─────────────────────────────────────┤
│  Tabla de registros                 │  ← solo si hay registros
│  (o mensaje "No hay datos")         │  ← solo si no hay registros
└─────────────────────────────────────┘
```

### Alerta de éxito o error
```razor
@if (!string.IsNullOrEmpty(mensaje))
{
    <div class="alert @(exito ? "alert-success" : "alert-danger")">
        @mensaje
    </div>
}
```
Si `exito` es true → alerta verde. Si es false → alerta roja.

### Formulario con @bind
```razor
<input class="form-control" @bind="campoCodigo" disabled="@editando" />
<input class="form-control" @bind="campoNombre" />
<input class="form-control" type="number" @bind="campoStock" />
<input class="form-control" type="number" step="0.01" @bind="campoValor" />
```
- `@bind` conecta cada input con una variable de C#.
- `disabled="@editando"` deshabilita el campo código cuando se está editando (la clave primaria no se puede cambiar).

### Tabla con @foreach
```razor
@foreach (var reg in registros)
{
    <tr>
        <td>@reg["codigo"]</td>
        <td>@reg["nombre"]</td>
        <td>@reg["stock"]</td>
        <td>@reg["valorunitario"]</td>
        <td>
            <button @onclick="() => EditarRegistro(reg)">Editar</button>
            <button @onclick="() => EliminarRegistro(reg)">Eliminar</button>
        </td>
    </tr>
}
```
Recorre la lista de registros y genera una fila por cada uno.

---

## 4. Bloque @code — Lógica en C#

### Variables de estado
```csharp
private List<Dictionary<string, object?>> registros = new();  // datos de la tabla
private bool cargando = true;           // mostrar spinner
private bool mostrarFormulario = false; // mostrar/ocultar formulario
private bool editando = false;          // modo crear o editar
private string mensaje = string.Empty;  // texto de la alerta
private bool exito = false;             // color de la alerta
```

### Campos del formulario
```csharp
private string campoCodigo = string.Empty;
private string campoNombre = string.Empty;
private int campoStock = 0;
private double campoValor = 0;
private int? limite = null;
```
Cada variable está conectada a un input del formulario mediante `@bind`.

### Cargar datos (OnInitializedAsync)
```csharp
protected override async Task OnInitializedAsync()
{
    await CargarRegistros();
}

private async Task CargarRegistros()
{
    cargando = true;
    registros = await Api.ListarAsync("producto", limite);
    cargando = false;
}
```
Al abrir la página, se llama a la API para traer todos los productos.

### Nuevo registro
```csharp
private void NuevoRegistro()
{
    editando = false;
    campoCodigo = string.Empty;
    campoNombre = string.Empty;
    campoStock = 0;
    campoValor = 0;
    mostrarFormulario = true;
}
```
Limpia el formulario y lo muestra en modo "crear".

### Editar registro
```csharp
private void EditarRegistro(Dictionary<string, object?> reg)
{
    editando = true;
    campoCodigo = reg["codigo"]?.ToString() ?? "";
    campoNombre = reg["nombre"]?.ToString() ?? "";
    campoStock = int.TryParse(reg["stock"]?.ToString(), out int s) ? s : 0;
    campoValor = double.TryParse(reg["valorunitario"]?.ToString(), out double v) ? v : 0;
    mostrarFormulario = true;
}
```
Llena el formulario con los datos del registro seleccionado y activa modo "editar".

### Guardar (crear o actualizar)
```csharp
private async Task GuardarRegistro()
{
    var datos = new Dictionary<string, object?>
    {
        ["codigo"] = campoCodigo,
        ["nombre"] = campoNombre,
        ["stock"] = campoStock,
        ["valorunitario"] = campoValor
    };

    if (editando)
    {
        datos.Remove("codigo");  // no enviar la clave en el body
        var resultado = await Api.ActualizarAsync("producto", "codigo", campoCodigo, datos);
        exito = resultado.exito;
        mensaje = resultado.mensaje;
    }
    else
    {
        var resultado = await Api.CrearAsync("producto", datos);
        exito = resultado.exito;
        mensaje = resultado.mensaje;
    }

    if (exito)
    {
        mostrarFormulario = false;
        await CargarRegistros();  // recargar la tabla
    }
}
```

### Eliminar
```csharp
private async Task EliminarRegistro(Dictionary<string, object?> reg)
{
    string codigo = reg["codigo"]?.ToString() ?? "";
    var confirmar = await JS.InvokeAsync<bool>("confirm",
        $"¿Está seguro de eliminar el Producto '{codigo}'?");
    if (!confirmar) return;

    var resultado = await Api.EliminarAsync("producto", "codigo", codigo);
    exito = resultado.exito;
    mensaje = resultado.mensaje;

    if (exito) await CargarRegistros();
}
```
Muestra un diálogo de confirmación con JavaScript antes de eliminar.

---

## 5. Resumen del flujo

```
Usuario abre /producto
  → OnInitializedAsync → Api.ListarAsync("producto") → tabla se llena

Usuario clic "Nuevo Producto"
  → NuevoRegistro() → formulario aparece vacío

Usuario llena campos y clic "Guardar"
  → GuardarRegistro() → Api.CrearAsync() → recarga tabla

Usuario clic "Editar" en una fila
  → EditarRegistro() → formulario se llena con datos existentes

Usuario modifica y clic "Guardar"
  → GuardarRegistro() → confirm() → Api.ActualizarAsync() → recarga tabla

Usuario clic "Eliminar" en una fila
  → EliminarRegistro() → confirm() → Api.EliminarAsync() → recarga tabla
```

---

## 6. Verificar que compila

```bash
dotnet build
```

---

## 7. Subir cambios, PR y merge

```bash
git add .
git commit -m "Agregar pagina CRUD Producto"
git push -u origin crud-producto-pagina
```

En GitHub: crear PR `crud-producto-pagina` → `main`, aprobar y hacer merge.

Después del merge, **Estudiante 2 y 3** actualizan:
```bash
git checkout main
git pull
```

---

> **Siguiente paso:** Paso 7 — CRUD Persona (Estudiante 2) y CRUD Usuario (Estudiante 3) en paralelo.
