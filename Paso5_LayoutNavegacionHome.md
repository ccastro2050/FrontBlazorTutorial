# Paso 5 — Layout, Navegación y Página Home

**Quién lo hace:** Estudiante 1 (código compartido, se hace merge a main para que todos lo tengan).

**Rama:** `layout-navegacion-home`

---

## 1. Modificar MainLayout.razor

Archivo: `Components/Layout/MainLayout.razor`

Cambiar la línea del link "About" por el título de la aplicación:

**Antes:**
```razor
<a href="https://learn.microsoft.com/aspnet/core/" target="_blank">About</a>
```

**Después:**
```razor
<span>Frontend Blazor — API GenericaCsharp</span>
```

**¿Qué es MainLayout?** Es la estructura visual que envuelve todas las páginas. Tiene dos zonas: el sidebar (menú lateral) y el área de contenido donde `@Body` se reemplaza con la página actual.

---

## 2. Modificar NavMenu.razor

Archivo: `Components/Layout/NavMenu.razor`

Reemplazar todo el contenido con:

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

    </nav>
</div>
```

**¿Qué se cambió?**
- Título del sidebar: `"CRUD Facturas"`
- Se eliminaron los links de ejemplo (Counter, Weather)
- Se agregaron los links a las tablas del proyecto: Producto, Persona, Usuario, Empresa, Rol, Ruta

**¿Qué es `NavLink`?** Funciona como un `<a>` pero agrega automáticamente la clase CSS `active` cuando la URL del navegador coincide con el `href`. Así el link actual se resalta en el menú.

---

## 3. Modificar Home.razor

Archivo: `Components/Pages/Home.razor`

Reemplazar todo el contenido con:

```razor
@page "/"
@inject FrontBlazorTutorial.Services.ApiService Api

<PageTitle>CRUD Facturas</PageTitle>

<div class="container mt-4">
    <h1>CRUD - Base de Datos Facturas</h1>
    <p class="lead">
        Frontend Blazor que consume la API generica
        <strong>ApiGenericaCsharp</strong> para gestionar tablas
        de una base de datos.
    </p>

    <div class="alert alert-info">
        <strong>Tablas disponibles:</strong> Producto, Persona, Usuario, Empresa, Rol, Ruta.
        <br />
        Use el menú lateral para navegar a cada tabla.
    </div>

    @@if (diagnostico != null)
    {
        <div class="card mt-4 border-secondary">
            <div class="card-header bg-secondary bg-opacity-10 text-muted py-2">
                <small><strong>Conexion activa</strong></small>
            </div>
            <div class="card-body py-2">
                <table class="table table-sm table-borderless mb-0"
                       style="font-size: 0.85rem;">
                    <tbody>
                        @@if (diagnostico.ContainsKey("proveedor"))
                        {
                            <tr>
                                <td class="text-muted" style="width:160px">Proveedor</td>
                                <td><strong>@@diagnostico["proveedor"]</strong></td>
                            </tr>
                        }
                        @@if (diagnostico.ContainsKey("baseDatos"))
                        {
                            <tr>
                                <td class="text-muted">Base de datos</td>
                                <td><strong>@@diagnostico["baseDatos"]</strong></td>
                            </tr>
                        }
                        @@if (diagnostico.ContainsKey("version"))
                        {
                            <tr>
                                <td class="text-muted">Version</td>
                                <td><small>@@(diagnostico["version"].Length > 80
                                    ? diagnostico["version"][..80] + "..."
                                    : diagnostico["version"])</small></td>
                            </tr>
                        }
                        @@if (diagnostico.ContainsKey("direccionIP"))
                        {
                            <tr>
                                <td class="text-muted">Servidor</td>
                                <td>
                                    @@diagnostico["direccionIP"]@@if (diagnostico.ContainsKey("puerto")){<text>:@@diagnostico["puerto"]</text>}
                                </td>
                            </tr>
                        }
                        @@if (diagnostico.ContainsKey("usuarioConectado"))
                        {
                            <tr>
                                <td class="text-muted">Usuario</td>
                                <td>@@diagnostico["usuarioConectado"]</td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    }
    else if (cargado)
    {
        <div class="mt-4">
            <small class="text-muted">No se pudo obtener informacion
                de conexión de la API.</small>
        </div>
    }
</div>

@@code {
    private Dictionary<string, string>? diagnostico;
    private bool cargado;

    protected override async Task OnInitializedAsync()
    {
        diagnostico = await Api.ObtenerDiagnosticoAsync();
        cargado = true;
    }
}
```

**¿Qué hace esta página?**

| Sección | Función |
|---------|---------|
| `@page "/"` | Esta es la página de inicio (ruta raíz) |
| `@inject ApiService Api` | Inyecta el servicio para llamar a la API |
| Título y descripción | Información general del proyecto |
| Sección de diagnóstico | Muestra la conexión activa a la BD (proveedor, base, versión, IP, usuario) |
| `OnInitializedAsync` | Al cargar la página, llama a la API para obtener info de conexión |

---

## 4. Eliminar páginas de ejemplo

Eliminar estos dos archivos que venían con el template y ya no se necesitan:

- `Components/Pages/Counter.razor`
- `Components/Pages/Weather.razor`

Se pueden eliminar manualmente o por terminal:

```bash
rm Components/Pages/Counter.razor
rm Components/Pages/Weather.razor
```

---

## 5. Verificar que compila

```bash
dotnet build
```

Debe mostrar **"0 Errores"**.

---

## 6. Subir cambios y merge

```bash
git add .
git commit -m "Configurar layout, navegacion y página Home"
git push -u origin layout-navegacion-home
```

Después, **Estudiante 1** fusiona desde la terminal:
```bash
git fetch origin
git merge origin/layout-navegacion-home
git push origin main
```

Después del merge, **Estudiante 2 y Estudiante 3** deben actualizar:

```bash
git checkout main
git pull
```

---

> **Siguiente paso:** Paso 6 — CRUD Producto (Estudiante 1).
