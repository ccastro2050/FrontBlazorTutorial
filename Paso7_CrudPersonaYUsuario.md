# Paso 7 — CRUD Persona (Estudiante 2) y CRUD Usuario (Estudiante 3)

Este paso lo hacen **Estudiante 2 y Estudiante 3 en paralelo**, cada uno en su propia rama.

| Estudiante | Tabla | Rama | Campos |
|------------|-------|------|--------|
| **Estudiante 2** | persona | `crud-persona` | codigo, nombre, email, telefono |
| **Estudiante 3** | usuario | `crud-usuario` | codigo, nombre, email, clave |

Ambos siguen la misma estructura que Producto (Paso 6). Lo que cambia son los campos y el nombre de la tabla.

---

## Antes de empezar

Cada estudiante debe asegurarse de estar en `main` y tener los últimos cambios (el merge del CRUD Producto):

```bash
git checkout main
git pull
```

Luego crear su rama:

**Estudiante 2:**
```bash
git checkout -b crud-persona
```

**Estudiante 3:**
```bash
git checkout -b crud-usuario
```

---

## Estudiante 2 — CRUD Persona

### 1. Crear el archivo `Components/Pages/Persona.razor`

```razor
@page "/persona"
@rendermode InteractiveServer
@inject FrontBlazorTutorial.Services.ApiService Api
@inject IJSRuntime JS

<PageTitle>Personas</PageTitle>

<div class="container mt-4">
    <h3>Personas</h3>

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
        <button class="btn btn-primary mb-3" @onclick="NuevoRegistro">Nueva Persona</button>
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
                @(editando ? "Editar Persona" : "Nueva Persona")
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
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Email</label>
                        <input class="form-control" type="email" @bind="campoEmail" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Telefono</label>
                        <input class="form-control" @bind="campoTelefono" />
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
                    <th>Email</th>
                    <th>Telefono</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var reg in registros)
                {
                    <tr>
                        <td>@reg["codigo"]</td>
                        <td>@reg["nombre"]</td>
                        <td>@reg["email"]</td>
                        <td>@reg["telefono"]</td>
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
        <div class="alert alert-warning">No se encontraron registros en la tabla persona.</div>
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
    private string campoEmail = string.Empty;
    private string campoTelefono = string.Empty;
    private int? limite = null;

    protected override async Task OnInitializedAsync()
    {
        await CargarRegistros();
    }

    private async Task CargarRegistros()
    {
        cargando = true;
        registros = await Api.ListarAsync("persona", limite);
        cargando = false;
    }

    private void NuevoRegistro()
    {
        editando = false;
        campoCodigo = string.Empty;
        campoNombre = string.Empty;
        campoEmail = string.Empty;
        campoTelefono = string.Empty;
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private void EditarRegistro(Dictionary<string, object?> reg)
    {
        editando = true;
        campoCodigo = reg["codigo"]?.ToString() ?? "";
        campoNombre = reg["nombre"]?.ToString() ?? "";
        campoEmail = reg["email"]?.ToString() ?? "";
        campoTelefono = reg["telefono"]?.ToString() ?? "";
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private async Task GuardarRegistro()
    {
        var datos = new Dictionary<string, object?>
        {
            ["codigo"] = campoCodigo,
            ["nombre"] = campoNombre,
            ["email"] = campoEmail,
            ["telefono"] = campoTelefono
        };

        if (editando)
        {
            datos.Remove("codigo");
            var confirmar = await JS.InvokeAsync<bool>("confirm",
                $"¿Está seguro de actualizar la Persona '{campoCodigo}'?");
            if (!confirmar) return;
            var resultado = await Api.ActualizarAsync("persona", "codigo", campoCodigo, datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }
        else
        {
            var resultado = await Api.CrearAsync("persona", datos);
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
            $"¿Está seguro de eliminar la Persona '{codigo}'?");
        if (!confirmar) return;
        var resultado = await Api.EliminarAsync("persona", "codigo", codigo);
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

| Producto | Persona |
|----------|---------|
| `@page "/producto"` | `@page "/persona"` |
| Campos: codigo, nombre, stock, valorunitario | Campos: codigo, nombre, email, telefono |
| `campoStock` (int), `campoValor` (double) | `campoEmail` (string), `campoTelefono` (string) |
| `Api.ListarAsync("producto", ...)` | `Api.ListarAsync("persona", ...)` |

La estructura y la lógica son idénticas. Solo cambian el nombre de la tabla y los campos.

### 3. Verificar que compila

```bash
dotnet build
```

### 4. Subir cambios y crear PR

```bash
git add .
git commit -m "Agregar página CRUD Persona"
git push -u origin crud-persona
```

Quien hizo push ve el botón amarillo "Compare & pull request" en GitHub y crea el PR: `crud-persona` → `main`. Si no aparece: pestaña **Pull requests** → **New pull request**.

**Estudiante 1** revisa y aprueba el PR, luego hace merge.

Después del merge, GitHub muestra un botón **Delete branch**. Se recomienda hacer clic para borrar la branch `crud-persona` del repositorio remoto. Esto no borra el código — el código ya está seguro en `main`. Solo elimina la branch que ya cumplió su propósito, para no acumular branches viejas.

---

## Estudiante 3 — CRUD Usuario

### 1. Crear el archivo `Components/Pages/Usuario.razor`

```razor
@page "/usuario"
@rendermode InteractiveServer
@inject FrontBlazorTutorial.Services.ApiService Api
@inject IJSRuntime JS

<PageTitle>Usuarios</PageTitle>

<div class="container mt-4">
    <h3>Usuarios</h3>

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
        <button class="btn btn-primary mb-3" @onclick="NuevoRegistro">Nuevo Usuario</button>
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
                @(editando ? "Editar Usuario" : "Nuevo Usuario")
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
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Email</label>
                        <input class="form-control" type="email" @bind="campoEmail" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Clave</label>
                        <input class="form-control" type="password" @bind="campoClave" />
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
                    <th>Email</th>
                    <th>Clave</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var reg in registros)
                {
                    <tr>
                        <td>@reg["codigo"]</td>
                        <td>@reg["nombre"]</td>
                        <td>@reg["email"]</td>
                        <td>@reg["clave"]</td>
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
        <div class="alert alert-warning">No se encontraron registros en la tabla usuario.</div>
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
    private string campoEmail = string.Empty;
    private string campoClave = string.Empty;
    private int? limite = null;

    protected override async Task OnInitializedAsync()
    {
        await CargarRegistros();
    }

    private async Task CargarRegistros()
    {
        cargando = true;
        registros = await Api.ListarAsync("usuario", limite);
        cargando = false;
    }

    private void NuevoRegistro()
    {
        editando = false;
        campoCodigo = string.Empty;
        campoNombre = string.Empty;
        campoEmail = string.Empty;
        campoClave = string.Empty;
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private void EditarRegistro(Dictionary<string, object?> reg)
    {
        editando = true;
        campoCodigo = reg["codigo"]?.ToString() ?? "";
        campoNombre = reg["nombre"]?.ToString() ?? "";
        campoEmail = reg["email"]?.ToString() ?? "";
        campoClave = reg["clave"]?.ToString() ?? "";
        mostrarFormulario = true;
        mensaje = string.Empty;
    }

    private async Task GuardarRegistro()
    {
        var datos = new Dictionary<string, object?>
        {
            ["codigo"] = campoCodigo,
            ["nombre"] = campoNombre,
            ["email"] = campoEmail,
            ["clave"] = campoClave
        };

        if (editando)
        {
            datos.Remove("codigo");
            var confirmar = await JS.InvokeAsync<bool>("confirm",
                $"¿Está seguro de actualizar el Usuario '{campoCodigo}'?");
            if (!confirmar) return;
            var resultado = await Api.ActualizarAsync("usuario", "codigo", campoCodigo, datos);
            exito = resultado.exito;
            mensaje = resultado.mensaje;
        }
        else
        {
            var resultado = await Api.CrearAsync("usuario", datos);
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
            $"¿Está seguro de eliminar el Usuario '{codigo}'?");
        if (!confirmar) return;
        var resultado = await Api.EliminarAsync("usuario", "codigo", codigo);
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

| Producto | Usuario |
|----------|---------|
| `@page "/producto"` | `@page "/usuario"` |
| Campos: codigo, nombre, stock, valorunitario | Campos: codigo, nombre, email, clave |
| `campoStock` (int), `campoValor` (double) | `campoEmail` (string), `campoClave` (string) |
| Input de stock es `type="number"` | Input de clave es `type="password"` |
| `Api.ListarAsync("producto", ...)` | `Api.ListarAsync("usuario", ...)` |

### 3. Verificar que compila

```bash
dotnet build
```

### 4. Subir cambios y crear PR

```bash
git add .
git commit -m "Agregar página CRUD Usuario"
git push -u origin crud-usuario
```

Quien hizo push ve el botón amarillo "Compare & pull request" en GitHub y crea el PR: `crud-usuario` → `main`. Si no aparece: pestaña **Pull requests** → **New pull request**.

**Estudiante 1** revisa y aprueba el PR, luego hace merge.

Después del merge, GitHub muestra un botón **Delete branch**. Se recomienda hacer clic para borrar la branch `crud-usuario` del repositorio remoto. Esto no borra el código — el código ya está seguro en `main`. Solo elimina la branch que ya cumplió su propósito.

---

## Después de los dos merges

Una vez que ambos PRs estén mergeados, **los tres estudiantes** actualizan su main:

```bash
git checkout main
git pull
```

Ahora el proyecto tiene 3 páginas CRUD funcionando: Producto, Persona y Usuario.

---

## ¿Cómo ver el trabajo de cada estudiante?

Después de hacer merge y borrar las ramas, el trabajo no se pierde. Hay varias formas de ver qué hizo cada estudiante:

### Pull Requests cerrados (la mejor forma)

En GitHub → pestaña **Pull requests** → clic en **Closed**.

Ahí aparece cada PR con:
- Quién lo creó
- Qué archivos cambió
- Los commits incluidos
- Comentarios y revisiones

Los PRs cerrados **no se borran nunca**. Son la mejor evidencia del trabajo de cada estudiante.

### Historial de commits

En GitHub → pestaña **Code** → clic en **"X Commits"** (arriba a la derecha).

Cada commit muestra el autor y qué archivos cambió. Se puede hacer clic en cualquier commit para ver el detalle.

### Por terminal

```bash
git log --oneline --author="nombre"
```

Muestra los commits de un estudiante específico.

### ¿Y las ramas?

Mientras las ramas existan, se pueden ver en GitHub → **Code** → clic en el dropdown que dice `main` → ahí aparecen todas las ramas. Al seleccionar una, se ve el código tal como estaba en esa rama.

Pero una vez que se borran, ya no se pueden ver. Por eso **los PRs cerrados son mejor referencia** — las ramas son temporales, los PRs son permanentes.

---

## Problemas comunes

### "Creé la branch antes de hacer pull y no tengo los cambios de main"

Esto pasa cuando un estudiante crea su branch **antes** de actualizar main con `git pull`. La branch se crea desde un main viejo y no tiene los cambios de los PRs anteriores (ApiService, Layout, Producto, etc.).

**Síntoma:** la branch solo tiene los commits iniciales (proyecto Blazor + .gitignore) y le faltan archivos como `Services/ApiService.cs` o `Components/Pages/Producto.razor`.

**Solución:**

```bash
git fetch origin
git merge origin/main
```

- `git fetch origin` descarga los cambios que hay en GitHub al repositorio local, pero no los aplica todavía. Es como "revisar el correo sin abrirlo".
- `git merge origin/main` aplica esos cambios a la branch actual. Es como "abrir el correo y leerlo".

Después de esto, la branch tendrá todo lo que hay en main y se puede seguir trabajando normalmente.

### "Hice commit en main en lugar de crear una branch"

Esto pasa cuando un estudiante hace su trabajo directamente en `main` sin crear una branch primero.

**Solución:**

1. Crear la branch desde donde está (el commit ya queda incluido):
```bash
git branch nombre-de-la-branch
git checkout nombre-de-la-branch
git push -u origin nombre-de-la-branch
```

2. Volver main al estado del remoto:
```bash
git checkout main
git reset --hard origin/main
```

Ahora el commit solo está en la branch nueva y main está limpio.

### "Git dice 'Everything up-to-date' pero no aparece el PR"

Esto pasa cuando la branch en GitHub no tiene cambios nuevos respecto a main. Puede ser porque:
- El commit se perdió (se hizo `reset --hard` antes de crear la branch)
- La branch se creó desde un punto que ya estaba en main

**Solución:** verificar que la branch tiene el commit con:
```bash
git log --oneline nombre-de-la-branch -5
```

Si el commit no aparece, hay que volver a crear el archivo, hacer commit y push.

---

> **Siguiente paso:** Paso 8 — CRUD Empresa (Estudiante 1) y CRUD Rol (Estudiante 3) en paralelo. Estudiante 2 hace CRUD Cliente.
