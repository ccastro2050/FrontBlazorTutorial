# Paso 9 — CRUD Ruta (Est1), CRUD Vendedor (Est2) y Actualizar NavMenu (Est3)

Los tres estudiantes trabajan **en paralelo**, cada uno en su propia rama.

| Estudiante | Tarea | Rama | Nota |
|------------|-------|------|------|
| **Estudiante 1** | CRUD Ruta | `crud-ruta` | Tabla simple. La clave primaria se llama `ruta` (igual que la tabla) |
| **Estudiante 2** | CRUD Vendedor | `crud-vendedor` | Tiene FK a persona + id autoincremental |
| **Estudiante 3** | Actualizar NavMenu | `actualizar-navmenu` | Agregar links de Cliente, Vendedor y Factura al menú |

---

## ¿En qué orden se hacen los PRs?

Los PRs se pueden hacer **en cualquier orden** cuando cada uno toca archivos diferentes y no hay dependencias entre ellos. En este paso, Ruta.razor, Vendedor.razor y NavMenu.razor son archivos independientes — no se chocan.

En el Paso 8, en cambio, **sí importaba el orden**: Cliente necesitaba que Empresa y Persona ya estuvieran en main, porque su código carga datos de esas tablas con `Api.ListarAsync("empresa")`.

**Regla simple para saber si hay dependencia:** si la página hace `Api.ListarAsync("otra_tabla")` para llenar un select, esa otra tabla debe tener su CRUD mergeado primero. Si la página solo usa su propia tabla, no depende de nadie y el orden no importa.

---

## Antes de empezar

Cada estudiante actualiza main y crea su rama:

```bash
git checkout main
git pull
git checkout -b nombre-de-la-rama
```

---

## Estudiante 1 — CRUD Ruta

Ruta tiene una particularidad: la clave primaria se llama `ruta`, que es el mismo nombre que la tabla. Los campos son `ruta` (string) y `descripción` (string).

### 1. Crear el archivo `Components/Pages/Ruta.razor`

```razor
@page "/ruta"
@rendermode InteractiveServer
@inject FrontBlazorTutorial.Services.ApiService Api
@inject IJSRuntime JS

<PageTitle>Rutas</PageTitle>

<div class="container mt-4">
    <h3>Rutas</h3>

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
        <button class="btn btn-primary mb-3" @onclick="NuevoRegistro">Nueva Ruta</button>
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
                @(editando ? "Editar Ruta" : "Nueva Ruta")
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Ruta</label>
                        <input class="form-control" @bind="campoRuta" disabled="@editando" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Descripcion</label>
                        <input class="form-control" @bind="campoDescripcion" />
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
                    <th>Ruta</th>
                    <th>Descripcion</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var reg in registros)
                {
                    <tr>
                        <td>@reg["ruta"]</td>
                        <td>@reg["descripción"]</td>
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
        <div class="alert alert-warning">No se encontraron registros en la tabla ruta.</div>
    }
</div>

@code {
    private List<Dictionary<string, object?>> registros = new();
    private bool cargando = true;
    private bool mostrarFormulario = false;
    private bool editando = false;
    private string mensaje = string.Empty;
    private bool exito = false;

    private string campoRuta = string.Empty;
    private string campoDescripcion = string.Empty;
    private int? limite = null;

    protected override async Task OnInitializedAsync()
    {
        await CargarRegistros();
    }

    private async Task CargarRegistros()
    {
        cargando = true;
        registros = await Api.ListarAsync("ruta", limite);
        cargando = false;
    }

    private void NuevoRegistro()
    {
        editando = false;
        campoRuta = string.Empty;
        campoDescripcion = string.Empty;
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private void EditarRegistro(Dictionary<string, object?> reg)
    {
        editando = true;
        campoRuta = reg["ruta"]?.ToString() ?? "";
        campoDescripcion = reg["descripción"]?.ToString() ?? "";
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private async Task GuardarRegistro()
    {
        var datos = new Dictionary<string, object?>
        {
            ["ruta"] = campoRuta,
            ["descripción"] = campoDescripcion
        };

        if (editando)
        {
            datos.Remove("ruta");
            var confirmar = await JS.InvokeAsync<bool>("confirm",
                $"¿Está seguro de actualizar la Ruta '{campoRuta}'?");
            if (!confirmar) return;
            var resultado = await Api.ActualizarAsync("ruta", "ruta", campoRuta, datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }
        else
        {
            var resultado = await Api.CrearAsync("ruta", datos);
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
        string ruta = reg["ruta"]?.ToString() ?? "";
        var confirmar = await JS.InvokeAsync<bool>("confirm",
            $"¿Está seguro de eliminar la Ruta '{ruta}'?");
        if (!confirmar) return;
        var resultado = await Api.EliminarAsync("ruta", "ruta", ruta);
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

### 2. Diferencia clave

En las tablas anteriores, la clave se llamaba `codigo` o `id`. Aquí la clave se llama `ruta`, igual que la tabla:

```csharp
var resultado = await Api.ActualizarAsync("ruta", "ruta", campoRuta, datos);
//                                        tabla   clave  valor
```

El primer `"ruta"` es el nombre de la tabla. El segundo `"ruta"` es el nombre de la columna clave. No es un error — así se llama el campo en la base de datos.

### 3. Verificar, subir y crear PR

```bash
dotnet build
git add .
git commit -m "Agregar página CRUD Ruta"
git push -u origin crud-ruta
```

Quien hizo push ve el botón amarillo "Compare & pull request" en GitHub y crea el PR: `crud-ruta` → `main`. Si no aparece el botón: ir a la pestaña **Pull requests** → **New pull request**. Después, **Estudiante 1** va a la pestaña **Pull requests**, abre el PR, revisa en **Files changed**, y hace **Merge pull request** → **Confirm merge**.
Después del merge, clic en **Delete branch**.

---

## Estudiante 2 — CRUD Vendedor

Vendedor es similar a Cliente: tiene una **llave foránea** a persona y un **id autoincremental**. Los campos son: `id` (auto), `carnet` (int), `direccion` (string) y `fkcodpersona` (FK).

### 1. Crear el archivo `Components/Pages/Vendedor.razor`

```razor
@page "/vendedor"
@rendermode InteractiveServer
@inject FrontBlazorTutorial.Services.ApiService Api
@inject IJSRuntime JS

<PageTitle>Vendedores</PageTitle>

<div class="container mt-4">
    <h3>Vendedores</h3>

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
        <button class="btn btn-primary mb-3" @onclick="NuevoRegistro">Nuevo Vendedor</button>
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
                @(editando ? "Editar Vendedor" : "Nuevo Vendedor")
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
                        <label class="form-label">Carnet</label>
                        <input class="form-control" type="number" @bind="campoCarnet" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Direccion</label>
                        <input class="form-control" @bind="campoDireccion" />
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
                    <th>Carnet</th>
                    <th>Direccion</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var reg in registros)
                {
                    <tr>
                        <td>@reg["id"]</td>
                        <td>@ObtenerNombrePersona(reg["fkcodpersona"]?.ToString())</td>
                        <td>@reg["carnet"]</td>
                        <td>@reg["direccion"]</td>
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
        <div class="alert alert-warning">No se encontraron registros en la tabla vendedor.</div>
    }
</div>

@code {
    private List<Dictionary<string, object?>> registros = new();
    private List<Dictionary<string, object?>> personas = new();
    private bool cargando = true;
    private bool mostrarFormulario = false;
    private bool editando = false;
    private string mensaje = string.Empty;
    private bool exito = false;

    private string campoId = string.Empty;
    private int campoCarnet = 0;
    private string campoDireccion = string.Empty;
    private string campoFkcodpersona = string.Empty;
    private int? limite = null;

    protected override async Task OnInitializedAsync()
    {
        personas = await Api.ListarAsync("persona");
        await CargarRegistros();
    }

    private async Task CargarRegistros()
    {
        cargando = true;
        registros = await Api.ListarAsync("vendedor", limite);
        cargando = false;
    }

    private string ObtenerNombrePersona(string? codigo)
    {
        if (string.IsNullOrEmpty(codigo)) return "Sin persona";
        var persona = personas.FirstOrDefault(p => p["codigo"]?.ToString() == codigo);
        return persona?["nombre"]?.ToString() ?? codigo;
    }

    private void NuevoRegistro()
    {
        editando = false;
        campoId = string.Empty;
        campoCarnet = 0;
        campoDireccion = string.Empty;
        campoFkcodpersona = string.Empty;
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private void EditarRegistro(Dictionary<string, object?> reg)
    {
        editando = true;
        campoId = reg["id"]?.ToString() ?? "";
        campoCarnet = int.TryParse(reg["carnet"]?.ToString(), out int c) ? c : 0;
        campoDireccion = reg["direccion"]?.ToString() ?? "";
        campoFkcodpersona = reg["fkcodpersona"]?.ToString() ?? "";
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private async Task GuardarRegistro()
    {
        var datos = new Dictionary<string, object?>
        {
            ["carnet"] = campoCarnet,
            ["direccion"] = campoDireccion,
            ["fkcodpersona"] = campoFkcodpersona
        };

        if (editando)
        {
            var confirmar = await JS.InvokeAsync<bool>("confirm",
                $"¿Está seguro de actualizar el Vendedor #{campoId}?");
            if (!confirmar) return;
            var resultado = await Api.ActualizarAsync("vendedor", "id", campoId, datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }
        else
        {
            var resultado = await Api.CrearAsync("vendedor", datos);
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
            $"¿Está seguro de eliminar el Vendedor #{id}?");
        if (!confirmar) return;
        var resultado = await Api.EliminarAsync("vendedor", "id", id);
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

### 2. Similitud con Cliente

Vendedor usa el mismo patrón que Cliente (Paso 8): id autoincremental, FK a persona con select, función `ObtenerNombrePersona`. La diferencia es que Vendedor tiene `carnet` (int) y `direccion` (string) en lugar de `credito` y `fkcodempresa`.

### 3. Verificar, subir y crear PR

```bash
dotnet build
git add .
git commit -m "Agregar página CRUD Vendedor"
git push -u origin crud-vendedor
```

Quien hizo push ve el botón amarillo "Compare & pull request" en GitHub y crea el PR: `crud-vendedor` → `main`. Si no aparece el botón: ir a la pestaña **Pull requests** → **New pull request**. Después, **Estudiante 1** va a la pestaña **Pull requests**, abre el PR, revisa en **Files changed**, y hace **Merge pull request** → **Confirm merge**.
Después del merge, clic en **Delete branch**.

---

## Estudiante 3 — Actualizar NavMenu

El menú lateral (`NavMenu.razor`) actualmente tiene links a: Home, Producto, Persona, Usuario, Empresa, Rol y Ruta. Faltan los links a **Cliente**, **Vendedor** y **Factura**.

**Este paso es importante** porque es la primera vez que un estudiante modifica un archivo existente en lugar de crear uno nuevo. Esto puede causar conflictos si otro estudiante también modifica este archivo.

### 1. Modificar `Components/Layout/NavMenu.razor`

Agregar los siguientes 3 bloques **antes** del cierre de `</nav>`:

```razor
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="cliente">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Cliente
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="vendedor">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Vendedor
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="factura">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Factura
            </NavLink>
        </div>
```

El NavMenu completo debe quedar así:

```razor
<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">CRUD Facturas</a>
    </div>
</div>

<input type="checkbox" title="Navigation menu" class="navbar-toggler" />

<div class="nav-scrollable" onclick="document.querySelector('.navbar-toggler').click()">
    <nav class="nav flex-column">

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Home
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="producto">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Producto
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="persona">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Persona
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="usuario">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Usuario
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="empresa">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Empresa
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="rol">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Rol
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="ruta">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Ruta
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="cliente">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Cliente
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="vendedor">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Vendedor
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="factura">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Factura
            </NavLink>
        </div>

    </nav>
</div>
```

### 2. ¿Qué se agregó?

Solo 3 bloques `<div class="nav-item">` nuevos al final de la lista, antes del cierre de `</nav>`. Cada uno es un link a una página CRUD:

- `href="cliente"` → abre `/cliente`
- `href="vendedor"` → abre `/vendedor`
- `href="factura"` → abre `/factura` (la página aún no existe, se creará en el Paso 10)

### 3. Verificar, subir y crear PR

```bash
dotnet build
git add .
git commit -m "Agregar links de Cliente, Vendedor y Factura al menu"
git push -u origin actualizar-navmenu
```

Quien hizo push ve el botón amarillo "Compare & pull request" en GitHub y crea el PR: `actualizar-navmenu` → `main`. Si no aparece el botón: ir a la pestaña **Pull requests** → **New pull request**. Después, **Estudiante 1** va a la pestaña **Pull requests**, abre el PR, revisa en **Files changed**, y hace **Merge pull request** → **Confirm merge**.
Después del merge, clic en **Delete branch**.

---

## Después de los tres merges

**Los tres estudiantes** actualizan su main:

```bash
git checkout main
git pull
```

Ahora el proyecto tiene 8 páginas CRUD (Producto, Persona, Usuario, Empresa, Rol, Ruta, Cliente, Vendedor) y el menú completo con todos los links.

---

> **Siguiente paso:** Paso 10 — Factura con Stored Procedures (maestro-detalle).
