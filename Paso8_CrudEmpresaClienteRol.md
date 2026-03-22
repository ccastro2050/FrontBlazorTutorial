# Paso 8 — CRUD Empresa (Est1), CRUD Cliente (Est2) y CRUD Rol (Est3)

Los tres estudiantes trabajan **en paralelo**, cada uno en su propia rama.

### ¿Qué pasa si un estudiante trabaja más rápido que otro?

No pasa nada. Cada `push` va a su propia branch, no a `main`. Las branches son independientes entre sí.

Por ejemplo, si Estudiante 3 hace push de `crud-rol` y Estudiante 2 hace push de `crud-cliente` al mismo tiempo, cada uno está en su propia branch. No se chocan.

Los PRs se pueden hacer en cualquier orden y momento. Lo único que importa es:

- **Archivos diferentes = sin conflicto.** Como cada estudiante crea un archivo nuevo distinto (Empresa.razor, Cliente.razor, Rol.razor), no hay conflicto de merge.
- **Si dos estudiantes modifican el mismo archivo**, ahí sí puede haber un conflicto. Pero en este paso eso no pasa.
- **Si un CRUD depende de otro** (como Cliente depende de Empresa y Persona), conviene hacer merge del que no depende primero.

Resumen: pueden acumularse varias branches con push sin PR y está bien. Cada branch es un mundo aparte hasta que se hace merge a `main`.

---

| Estudiante | Tabla | Rama | Campos | Nota |
|------------|-------|------|--------|------|
| **Estudiante 1** | empresa | `crud-empresa` | codigo, nombre | Tabla simple, 2 campos |
| **Estudiante 2** | cliente | `crud-cliente` | id, credito, fkcodpersona, fkcodempresa | Tiene llaves foráneas (selects) |
| **Estudiante 3** | rol | `crud-rol` | id, nombre | La clave es `id` (int), no `codigo` (string) |

---

## Antes de empezar

Cada estudiante debe actualizar su main y crear su rama:

```bash
git checkout main
git pull
git checkout -b nombre-de-la-rama
```

Ejemplo para Estudiante 1:
```bash
git checkout main
git pull
git checkout -b crud-empresa
```

---

## Estudiante 1 — CRUD Empresa

Empresa es una tabla simple con solo 2 campos: `codigo` y `nombre`.

### 1. Crear el archivo `Components/Pages/Empresa.razor`

```razor
@page "/empresa"
@rendermode InteractiveServer
@inject FrontBlazorTutorial.Services.ApiService Api
@inject IJSRuntime JS

<PageTitle>Empresas</PageTitle>

<div class="container mt-4">
    <h3>Empresas</h3>

    @* ───────── ALERTA ───────── *@
    @if (!string.IsNullOrEmpty(mensaje))
    {
        <div class="alert @(exito ? "alert-success" : "alert-danger") alert-dismissible fade show">
            @mensaje
            <button type="button" class="btn-close" @onclick="() => mensaje = string.Empty"></button>
        </div>
    }

    @* ───────── BOTON NUEVO ───────── *@
    @if (!mostrarFormulario)
    {
        <button class="btn btn-primary mb-3" @onclick="NuevoRegistro">Nueva Empresa</button>
    }

    @* ───────── LIMITE DE REGISTROS ───────── *@
    <div class="d-flex align-items-center mb-3">
        <label class="form-label me-2 mb-0">Limite:</label>
        <input class="form-control me-2" type="number" style="width:100px" @bind="limite" />
        <button class="btn btn-outline-secondary" @onclick="CargarRegistros">Cargar</button>
    </div>

    @* ───────── FORMULARIO ───────── *@
    @if (mostrarFormulario)
    {
        <div class="card mb-3">
            <div class="card-header">
                @(editando ? "Editar Empresa" : "Nueva Empresa")
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Codigo</label>
                        <input class="form-control" @bind="campoCodigo" disabled="@editando" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Nombre</label>
                        <input class="form-control" @bind="campoNombre" />
                    </div>
                </div>
                <button class="btn btn-success me-2" @onclick="GuardarRegistro">Guardar</button>
                <button class="btn btn-secondary" @onclick="Cancelar">Cancelar</button>
            </div>
        </div>
    }

    @* ───────── SPINNER ───────── *@
    @if (cargando)
    {
        <div class="d-flex justify-content-center my-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
        </div>
    }

    @* ───────── TABLA ───────── *@
    @if (!cargando && registros.Any())
    {
        <table class="table table-striped table-hover">
            <thead class="table-dark">
                <tr>
                    <th>Codigo</th>
                    <th>Nombre</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var reg in registros)
                {
                    <tr>
                        <td>@reg["codigo"]</td>
                        <td>@reg["nombre"]</td>
                        <td>
                            <button class="btn btn-warning btn-sm me-1"
                                    @onclick="() => EditarRegistro(reg)">Editar</button>
                            <button class="btn btn-danger btn-sm"
                                    @onclick="() => EliminarRegistro(reg)">Eliminar</button>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }

    @if (!cargando && !registros.Any())
    {
        <div class="alert alert-warning">No se encontraron registros en la tabla empresa.</div>
    }
</div>

@code {
    private List<Dictionary<string, object?>> registros = new();
    private bool cargando = true;
    private bool mostrarFormulario = false;
    private bool editando = false;
    private string mensaje = string.Empty;
    private bool exito = false;

    private string campoCodigo = string.Empty;
    private string campoNombre = string.Empty;
    private int? limite = null;

    protected override async Task OnInitializedAsync()
    {
        await CargarRegistros();
    }

    private async Task CargarRegistros()
    {
        cargando = true;
        registros = await Api.ListarAsync("empresa", limite);
        cargando = false;
    }

    private void NuevoRegistro()
    {
        editando = false;
        campoCodigo = string.Empty;
        campoNombre = string.Empty;
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private void EditarRegistro(Dictionary<string, object?> reg)
    {
        editando = true;
        campoCodigo = reg["codigo"]?.ToString() ?? "";
        campoNombre = reg["nombre"]?.ToString() ?? "";
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private async Task GuardarRegistro()
    {
        var datos = new Dictionary<string, object?>
        {
            ["codigo"] = campoCodigo,
            ["nombre"] = campoNombre
        };

        if (editando)
        {
            datos.Remove("codigo");
            var confirmar = await JS.InvokeAsync<bool>("confirm",
                $"¿Está seguro de actualizar la Empresa '{campoCodigo}'?");
            if (!confirmar) return;
            var resultado = await Api.ActualizarAsync("empresa", "codigo", campoCodigo, datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }
        else
        {
            var resultado = await Api.CrearAsync("empresa", datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }

        if (exito)
        {
            mostrarFormulario = false;
            await CargarRegistros();
        }
    }

    private async Task EliminarRegistro(Dictionary<string, object?> reg)
    {
        string codigo = reg["codigo"]?.ToString() ?? "";
        var confirmar = await JS.InvokeAsync<bool>("confirm",
            $"¿Está seguro de eliminar la Empresa '{codigo}'?");
        if (!confirmar) return;
        var resultado = await Api.EliminarAsync("empresa", "codigo", codigo);
        exito = resultado.exito;
        mensaje = resultado.mensaje;

        if (exito)
            await CargarRegistros();
    }

    private void Cancelar()
    {
        mostrarFormulario = false;
        mensaje = string.Empty;
    }
}
```

### 2. ¿Qué cambió respecto a Producto?

| Producto | Empresa |
|----------|---------|
| 4 campos: codigo, nombre, stock, valorunitario | 2 campos: codigo, nombre |
| Variables `campoStock` y `campoValor` | No tiene — solo `campoCodigo` y `campoNombre` |

Empresa es más simple que Producto: misma estructura, menos campos.

### 3. Verificar, subir y crear PR

```bash
dotnet build
git add .
git commit -m "Agregar pagina CRUD Empresa"
git push -u origin crud-empresa
```

En GitHub: crear Pull Request `crud-empresa` → `main`, aprobar y hacer merge.
Después del merge, clic en **Delete branch** para limpiar.

---

## Estudiante 3 — CRUD Rol

Rol tiene una diferencia importante: la clave primaria es `id` (un número entero), no `codigo` (un texto). Esto cambia el tipo de la variable y cómo se pasa a la API.

### 1. Crear el archivo `Components/Pages/Rol.razor`

```razor
@page "/rol"
@rendermode InteractiveServer
@inject FrontBlazorTutorial.Services.ApiService Api
@inject IJSRuntime JS

<PageTitle>Roles</PageTitle>

<div class="container mt-4">
    <h3>Roles</h3>

    @* ───────── ALERTA ───────── *@
    @if (!string.IsNullOrEmpty(mensaje))
    {
        <div class="alert @(exito ? "alert-success" : "alert-danger") alert-dismissible fade show">
            @mensaje
            <button type="button" class="btn-close" @onclick="() => mensaje = string.Empty"></button>
        </div>
    }

    @* ───────── BOTON NUEVO ───────── *@
    @if (!mostrarFormulario)
    {
        <button class="btn btn-primary mb-3" @onclick="NuevoRegistro">Nuevo Rol</button>
    }

    @* ───────── LIMITE DE REGISTROS ───────── *@
    <div class="d-flex align-items-center mb-3">
        <label class="form-label me-2 mb-0">Limite:</label>
        <input class="form-control me-2" type="number" style="width:100px" @bind="limite" />
        <button class="btn btn-outline-secondary" @onclick="CargarRegistros">Cargar</button>
    </div>

    @* ───────── FORMULARIO ───────── *@
    @if (mostrarFormulario)
    {
        <div class="card mb-3">
            <div class="card-header">
                @(editando ? "Editar Rol" : "Nuevo Rol")
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6 mb-3">
                        <label class="form-label">ID</label>
                        <input class="form-control" type="number" @bind="campoId" disabled="@editando" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Nombre</label>
                        <input class="form-control" @bind="campoNombre" />
                    </div>
                </div>
                <button class="btn btn-success me-2" @onclick="GuardarRegistro">Guardar</button>
                <button class="btn btn-secondary" @onclick="Cancelar">Cancelar</button>
            </div>
        </div>
    }

    @* ───────── SPINNER ───────── *@
    @if (cargando)
    {
        <div class="d-flex justify-content-center my-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
        </div>
    }

    @* ───────── TABLA ───────── *@
    @if (!cargando && registros.Any())
    {
        <table class="table table-striped table-hover">
            <thead class="table-dark">
                <tr>
                    <th>ID</th>
                    <th>Nombre</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var reg in registros)
                {
                    <tr>
                        <td>@reg["id"]</td>
                        <td>@reg["nombre"]</td>
                        <td>
                            <button class="btn btn-warning btn-sm me-1"
                                    @onclick="() => EditarRegistro(reg)">Editar</button>
                            <button class="btn btn-danger btn-sm"
                                    @onclick="() => EliminarRegistro(reg)">Eliminar</button>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }

    @if (!cargando && !registros.Any())
    {
        <div class="alert alert-warning">No se encontraron registros en la tabla rol.</div>
    }
</div>

@code {
    private List<Dictionary<string, object?>> registros = new();
    private bool cargando = true;
    private bool mostrarFormulario = false;
    private bool editando = false;
    private string mensaje = string.Empty;
    private bool exito = false;

    private int campoId = 0;
    private string campoNombre = string.Empty;
    private int? limite = null;

    protected override async Task OnInitializedAsync()
    {
        await CargarRegistros();
    }

    private async Task CargarRegistros()
    {
        cargando = true;
        registros = await Api.ListarAsync("rol", limite);
        cargando = false;
    }

    private void NuevoRegistro()
    {
        editando = false;
        campoId = 0;
        campoNombre = string.Empty;
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private void EditarRegistro(Dictionary<string, object?> reg)
    {
        editando = true;
        campoId = int.TryParse(reg["id"]?.ToString(), out int i) ? i : 0;
        campoNombre = reg["nombre"]?.ToString() ?? "";
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private async Task GuardarRegistro()
    {
        var datos = new Dictionary<string, object?>
        {
            ["id"] = campoId,
            ["nombre"] = campoNombre
        };

        if (editando)
        {
            datos.Remove("id");
            var confirmar = await JS.InvokeAsync<bool>("confirm",
                $"¿Está seguro de actualizar el Rol #{campoId}?");
            if (!confirmar) return;
            var resultado = await Api.ActualizarAsync("rol", "id", campoId.ToString(), datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }
        else
        {
            var resultado = await Api.CrearAsync("rol", datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }

        if (exito)
        {
            mostrarFormulario = false;
            await CargarRegistros();
        }
    }

    private async Task EliminarRegistro(Dictionary<string, object?> reg)
    {
        string id = reg["id"]?.ToString() ?? "";
        var confirmar = await JS.InvokeAsync<bool>("confirm",
            $"¿Está seguro de eliminar el Rol #{id}?");
        if (!confirmar) return;
        var resultado = await Api.EliminarAsync("rol", "id", id);
        exito = resultado.exito;
        mensaje = resultado.mensaje;

        if (exito)
            await CargarRegistros();
    }

    private void Cancelar()
    {
        mostrarFormulario = false;
        mensaje = string.Empty;
    }
}
```

### 2. Diferencias clave respecto a Producto

| Producto | Rol |
|----------|-----|
| Clave: `codigo` (string) | Clave: `id` (int) |
| `campoCodigo` es `string` | `campoId` es `int` |
| Input de texto para la clave | Input `type="number"` para la clave |
| `Api.ActualizarAsync("producto", "codigo", campoCodigo, ...)` | `Api.ActualizarAsync("rol", "id", campoId.ToString(), ...)` |

Notar que `campoId.ToString()` es necesario porque la API recibe la clave como string en la URL, aunque en la base de datos sea un número.

### 3. Verificar, subir y crear PR

```bash
dotnet build
git add .
git commit -m "Agregar pagina CRUD Rol"
git push -u origin crud-rol
```

En GitHub: crear Pull Request `crud-rol` → `main`, aprobar y hacer merge.
Después del merge, clic en **Delete branch** para limpiar.

---

## Estudiante 2 — CRUD Cliente (con llaves foráneas)

Cliente es **más complejo** que los anteriores porque tiene **llaves foráneas**: cada cliente está asociado a una `persona` y opcionalmente a una `empresa`. Esto introduce conceptos nuevos:

- **Selects (`<select>`)** en lugar de inputs de texto, para elegir de una lista existente
- **Cargar datos de otras tablas** al iniciar la página
- **Funciones auxiliares** para mostrar nombres en lugar de códigos en la tabla
- **Clave autoincremental** (`id`): no se envía al crear, la genera la base de datos

### 1. Crear el archivo `Components/Pages/Cliente.razor`

```razor
@page "/cliente"
@rendermode InteractiveServer
@inject FrontBlazorTutorial.Services.ApiService Api
@inject IJSRuntime JS

<PageTitle>Clientes</PageTitle>

<div class="container mt-4">
    <h3>Clientes</h3>

    @* ───────── ALERTA ───────── *@
    @if (!string.IsNullOrEmpty(mensaje))
    {
        <div class="alert @(exito ? "alert-success" : "alert-danger") alert-dismissible fade show">
            @mensaje
            <button type="button" class="btn-close" @onclick="() => mensaje = string.Empty"></button>
        </div>
    }

    @* ───────── BOTON NUEVO ───────── *@
    @if (!mostrarFormulario)
    {
        <button class="btn btn-primary mb-3" @onclick="NuevoRegistro">Nuevo Cliente</button>
    }

    @* ───────── LIMITE DE REGISTROS ───────── *@
    <div class="d-flex align-items-center mb-3">
        <label class="form-label me-2 mb-0">Limite:</label>
        <input class="form-control me-2" type="number" style="width:100px" @bind="limite" />
        <button class="btn btn-outline-secondary" @onclick="CargarRegistros">Cargar</button>
    </div>

    @* ───────── FORMULARIO ───────── *@
    @if (mostrarFormulario)
    {
        <div class="card mb-3">
            <div class="card-header">
                @(editando ? "Editar Cliente" : "Nuevo Cliente")
            </div>
            <div class="card-body">
                <div class="row">
                    @if (editando)
                    {
                        <div class="col-md-6 mb-3">
                            <label class="form-label">ID</label>
                            <input class="form-control" value="@campoId" disabled />
                        </div>
                    }
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Credito</label>
                        <input class="form-control" type="number" step="0.01" @bind="campoCredito" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Persona</label>
                        <select class="form-select" @bind="campoFkcodpersona">
                            <option value="">-- Seleccione --</option>
                            @foreach (var p in personas)
                            {
                                <option value="@p["codigo"]">@p["nombre"] (@p["codigo"])</option>
                            }
                        </select>
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Empresa</label>
                        <select class="form-select" @bind="campoFkcodempresa">
                            <option value="">-- Ninguna --</option>
                            @foreach (var e in empresas)
                            {
                                <option value="@e["codigo"]">@e["nombre"] (@e["codigo"])</option>
                            }
                        </select>
                    </div>
                </div>
                <button class="btn btn-success me-2" @onclick="GuardarRegistro">Guardar</button>
                <button class="btn btn-secondary" @onclick="Cancelar">Cancelar</button>
            </div>
        </div>
    }

    @* ───────── SPINNER ───────── *@
    @if (cargando)
    {
        <div class="d-flex justify-content-center my-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
        </div>
    }

    @* ───────── TABLA ───────── *@
    @if (!cargando && registros.Any())
    {
        <table class="table table-striped table-hover">
            <thead class="table-dark">
                <tr>
                    <th>ID</th>
                    <th>Persona</th>
                    <th>Credito</th>
                    <th>Empresa</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var reg in registros)
                {
                    <tr>
                        <td>@reg["id"]</td>
                        <td>@ObtenerNombrePersona(reg["fkcodpersona"]?.ToString())</td>
                        <td>@reg["credito"]</td>
                        <td>@ObtenerNombreEmpresa(reg["fkcodempresa"]?.ToString())</td>
                        <td>
                            <button class="btn btn-warning btn-sm me-1"
                                    @onclick="() => EditarRegistro(reg)">Editar</button>
                            <button class="btn btn-danger btn-sm"
                                    @onclick="() => EliminarRegistro(reg)">Eliminar</button>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }

    @if (!cargando && !registros.Any())
    {
        <div class="alert alert-warning">No se encontraron registros en la tabla cliente.</div>
    }
</div>

@code {
    private List<Dictionary<string, object?>> registros = new();
    private List<Dictionary<string, object?>> personas = new();
    private List<Dictionary<string, object?>> empresas = new();
    private bool cargando = true;
    private bool mostrarFormulario = false;
    private bool editando = false;
    private string mensaje = string.Empty;
    private bool exito = false;

    private string campoId = string.Empty;
    private double campoCredito = 0;
    private string campoFkcodpersona = string.Empty;
    private string campoFkcodempresa = string.Empty;
    private int? limite = null;

    protected override async Task OnInitializedAsync()
    {
        personas = await Api.ListarAsync("persona");
        empresas = await Api.ListarAsync("empresa");
        await CargarRegistros();
    }

    private async Task CargarRegistros()
    {
        cargando = true;
        registros = await Api.ListarAsync("cliente", limite);
        cargando = false;
    }

    private string ObtenerNombrePersona(string? codigo)
    {
        if (string.IsNullOrEmpty(codigo)) return "Sin persona";
        var persona = personas.FirstOrDefault(p => p["codigo"]?.ToString() == codigo);
        return persona?["nombre"]?.ToString() ?? codigo;
    }

    private string ObtenerNombreEmpresa(string? codigo)
    {
        if (string.IsNullOrEmpty(codigo)) return "-";
        var empresa = empresas.FirstOrDefault(e => e["codigo"]?.ToString() == codigo);
        return empresa?["nombre"]?.ToString() ?? codigo;
    }

    private void NuevoRegistro()
    {
        editando = false;
        campoId = string.Empty;
        campoCredito = 0;
        campoFkcodpersona = string.Empty;
        campoFkcodempresa = string.Empty;
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private void EditarRegistro(Dictionary<string, object?> reg)
    {
        editando = true;
        campoId = reg["id"]?.ToString() ?? "";
        campoCredito = double.TryParse(reg["credito"]?.ToString(), out double c) ? c : 0;
        campoFkcodpersona = reg["fkcodpersona"]?.ToString() ?? "";
        campoFkcodempresa = reg["fkcodempresa"]?.ToString() ?? "";
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private async Task GuardarRegistro()
    {
        var datos = new Dictionary<string, object?>
        {
            ["credito"] = campoCredito,
            ["fkcodpersona"] = campoFkcodpersona,
            ["fkcodempresa"] = string.IsNullOrEmpty(campoFkcodempresa) ? null : campoFkcodempresa
        };

        if (editando)
        {
            var confirmar = await JS.InvokeAsync<bool>("confirm",
                $"¿Está seguro de actualizar el Cliente #{campoId}?");
            if (!confirmar) return;
            var resultado = await Api.ActualizarAsync("cliente", "id", campoId, datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }
        else
        {
            var resultado = await Api.CrearAsync("cliente", datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }

        if (exito)
        {
            mostrarFormulario = false;
            await CargarRegistros();
        }
    }

    private async Task EliminarRegistro(Dictionary<string, object?> reg)
    {
        string id = reg["id"]?.ToString() ?? "";
        var confirmar = await JS.InvokeAsync<bool>("confirm",
            $"¿Está seguro de eliminar el Cliente #{id}?");
        if (!confirmar) return;
        var resultado = await Api.EliminarAsync("cliente", "id", id);
        exito = resultado.exito;
        mensaje = resultado.mensaje;

        if (exito)
            await CargarRegistros();
    }

    private void Cancelar()
    {
        mostrarFormulario = false;
        mensaje = string.Empty;
    }
}
```

### 2. Conceptos nuevos en Cliente

#### Llaves foráneas con `<select>`

En lugar de escribir el código de la persona a mano, se usa un `<select>` que muestra una lista desplegable:

```razor
<select class="form-select" @bind="campoFkcodpersona">
    <option value="">-- Seleccione --</option>
    @foreach (var p in personas)
    {
        <option value="@p["codigo"]">@p["nombre"] (@p["codigo"])</option>
    }
</select>
```

El `@foreach` recorre la lista de personas y genera una opción por cada una. El `value` es el código (lo que se guarda en la BD) y el texto visible es el nombre.

#### Cargar datos de otras tablas

Al iniciar la página se cargan las listas de personas y empresas para llenar los selects:

```csharp
protected override async Task OnInitializedAsync()
{
    personas = await Api.ListarAsync("persona");
    empresas = await Api.ListarAsync("empresa");
    await CargarRegistros();
}
```

#### Mostrar nombres en la tabla

La tabla de clientes guarda códigos (`fkcodpersona`, `fkcodempresa`), pero al usuario le queremos mostrar nombres. Para eso se usan funciones auxiliares:

```csharp
private string ObtenerNombrePersona(string? codigo)
{
    if (string.IsNullOrEmpty(codigo)) return "Sin persona";
    var persona = personas.FirstOrDefault(p => p["codigo"]?.ToString() == codigo);
    return persona?["nombre"]?.ToString() ?? codigo;
}
```

Busca en la lista de personas la que tiene ese código y devuelve su nombre. Si no la encuentra, devuelve el código tal cual.

#### Clave autoincremental

El `id` del cliente lo genera la base de datos automáticamente. Por eso:
- Al **crear**, no se envía el `id` en los datos
- Al **editar**, el `id` se muestra como campo deshabilitado pero no se incluye en los datos a actualizar

#### Campo opcional (empresa)

La empresa es opcional (un cliente puede no pertenecer a ninguna empresa). Si el select queda vacío, se envía `null`:

```csharp
["fkcodempresa"] = string.IsNullOrEmpty(campoFkcodempresa) ? null : campoFkcodempresa
```

### 3. Importante: orden de los PRs

El CRUD Cliente depende de que existan las tablas **persona** y **empresa** en el proyecto. Por eso:

1. Primero debe hacerse merge del PR de **Empresa** (Estudiante 1)
2. Luego Estudiante 2 actualiza su rama con los cambios de main:
   ```bash
   git fetch origin
   git merge origin/main
   ```
3. Después sube su PR de Cliente

Si Estudiante 2 sube el PR antes de que Empresa esté en main, la página funcionará pero el select de empresas estará vacío hasta que se agreguen empresas.

### 4. Verificar, subir y crear PR

```bash
dotnet build
git add .
git commit -m "Agregar pagina CRUD Cliente"
git push -u origin crud-cliente
```

En GitHub: crear Pull Request `crud-cliente` → `main`, aprobar y hacer merge.
Después del merge, clic en **Delete branch** para limpiar.

---

## Después de los tres merges

**Los tres estudiantes** actualizan su main:

```bash
git checkout main
git pull
```

Ahora el proyecto tiene 6 páginas CRUD funcionando: Producto, Persona, Usuario, Empresa, Rol y Cliente.

---

## ¿Cómo resolver conflictos de merge?

Cuando dos branches modifican el **mismo archivo en las mismas líneas**, GitHub no puede hacer merge automáticamente y muestra un botón **Resolve conflicts** en el PR.

Al hacer clic, se abre un editor con marcas como estas:

```
<<<<<<< crud-cliente
    enlace a Cliente
=======
    enlace a Rol
>>>>>>> main
```

Esto significa: "tu branch dice una cosa, pero main dice otra". Para resolverlo:

1. Borrar las tres líneas de marcas (`<<<<<<<`, `=======`, `>>>>>>>`)
2. Dejar el código correcto — puede ser uno, el otro, o ambos combinados
3. Clic en **Mark as resolved**
4. Clic en **Commit merge**

Ejemplo resuelto (dejando ambos):
```
    enlace a Cliente
    enlace a Rol
```

**¿Cuándo pasa esto en este proyecto?** Casi nunca, porque cada estudiante crea archivos nuevos diferentes. Pero podría pasar si dos estudiantes modifican el mismo archivo existente (por ejemplo, `NavMenu.razor` para agregar un link al menú).

**Tip:** para evitar conflictos, hagan merge de los PRs uno a la vez. Después de cada merge, los demás estudiantes pueden actualizar su branch con:
```bash
git fetch origin
git merge origin/main
```

---

> **Siguiente paso:** Paso 9 — CRUD Ruta y Vendedor.
