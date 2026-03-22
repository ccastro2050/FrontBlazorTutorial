# Paso 11 — Resumen del Flujo Completo

Este documento es un resumen paso a paso de **todo** lo que hizo cada estudiante a lo largo del tutorial. Sirve como hoja de referencia ("cheat sheet") para repasar el proyecto completo.

**Tecnologia:** Blazor Server, C#, .NET 9
**API:** ApiGenericaCsharp en `http://localhost:5035`
**Comando para ejecutar:** `dotnet run`
**Comando para compilar:** `dotnet build`

---

## Estudiante 1 (Administrador / Scrum Master)

Estudiante 1 es el administrador del repositorio. Crea el proyecto, invita a los colaboradores, revisa y aprueba los PRs de los demas, y hace merge a main.

---

### Paso 0 — Plan de Desarrollo

No requiere codigo. Se revisa el plan de desarrollo, se asignan las historias de usuario y se definen las convenciones de trabajo (ramas, commits, PRs).

---

### Paso 3 — Crear el Proyecto y Configurar GitHub

**Rama:** trabaja directamente en `main` (es el commit inicial)

**Archivos creados:** Todo el proyecto inicial generado por `dotnet new blazor`

```powershell
# --- Crear el proyecto ---
cd C:\Users\TU_USUARIO\Desktop\proyectoscsharp                  # ir a la carpeta de proyectos
dotnet new blazor -n FrontBlazorTutorial --interactivity Server  # crear proyecto Blazor Server
cd FrontBlazorTutorial                                           # entrar al proyecto
dotnet run                                                       # verificar que funciona (Ctrl+C para cerrar)

# --- Inicializar Git y subir a GitHub ---
git init                                                         # inicializar repositorio git
git add .                                                        # agregar todos los archivos al staging
git commit -m "Proyecto Blazor Server inicial"                   # primer commit
git remote add origin https://github.com/TU_USUARIO/FrontBlazorTutorial.git  # conectar con GitHub
git branch -M main                                               # renombrar rama a main
git push -u origin main                                          # subir a GitHub

# --- Invitar colaboradores ---
# En GitHub: Settings > Collaborators > Add people
# Agregar a Estudiante 2 y Estudiante 3
```

---

### Paso 4 — Conexion a la API y ApiService

**Rama:** `api-service` (o trabaja en `main` si es el unico desarrollador en este punto)

**Archivos creados/modificados:**
- `Services/ApiService.cs` (NUEVO)
- `Program.cs` (MODIFICADO — registrar HttpClient y ApiService)
- `appsettings.json` (MODIFICADO — agregar ApiBaseUrl)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios
git checkout -b api-service                # crear rama para el ApiService

# --- Crear carpeta Services ---
mkdir Services                             # crear carpeta para los servicios

# --- Crear archivo Services/ApiService.cs ---
# Contiene: ListarAsync, CrearAsync, ActualizarAsync, EliminarAsync, ObtenerDiagnosticoAsync
# Cada metodo conecta con la API REST usando HttpClient

# --- Modificar appsettings.json ---
# Agregar: "ApiBaseUrl": "http://localhost:5035"

# --- Modificar Program.cs ---
# Agregar: builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
# Agregar: builder.Services.AddScoped<FrontBlazorTutorial.Services.ApiService>();

# --- Verificar y subir ---
dotnet build                               # verificar que compila sin errores
git add .                                  # agregar cambios al staging
git commit -m "feat: agregar ApiService y configurar conexion a la API"  # commit
git push -u origin api-service             # subir rama a GitHub

# --- En GitHub: crear PR api-service -> main, aprobar y merge ---
```

---

### Paso 5 — Layout, Navegacion y Home

**Rama:** `layout-navegacion-home`

**Archivos creados/modificados:**
- `Components/Layout/MainLayout.razor` (MODIFICADO — cambiar titulo)
- `Components/Layout/NavMenu.razor` (MODIFICADO — reemplazar menu completo)
- `Components/Pages/Home.razor` (MODIFICADO — agregar diagnostico de conexion)
- `Components/Pages/Counter.razor` (ELIMINADO)
- `Components/Pages/Weather.razor` (ELIMINADO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios
git checkout -b layout-navegacion-home     # crear rama para layout y home

# --- Modificar MainLayout.razor ---
# Cambiar el link "About" por: <span>Frontend Blazor - API GenericaCsharp</span>

# --- Modificar NavMenu.razor ---
# Reemplazar todo el contenido con el menu lateral que incluye:
# Home, Producto, Persona, Usuario, Empresa, Rol, Ruta

# --- Modificar Home.razor ---
# Agregar seccion de diagnostico que muestra info de conexion a la BD
# Usa @inject ApiService Api y llama a ObtenerDiagnosticoAsync()

# --- Eliminar paginas de ejemplo ---
# Eliminar Counter.razor y Weather.razor

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: configurar layout, navegacion y pagina Home"  # commit
git push -u origin layout-navegacion-home  # subir rama

# --- En GitHub: crear PR layout-navegacion-home -> main, aprobar y merge ---
```

---

### Paso 6 — CRUD Producto

**Rama:** `crud-producto-pagina`

**Archivos creados:**
- `Components/Pages/Producto.razor` (NUEVO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios
git checkout -b crud-producto-pagina       # crear rama para CRUD Producto

# --- Crear Components/Pages/Producto.razor ---
# Pagina con @page "/producto"
# Funcionalidad: listar, crear, editar, eliminar productos
# Campos: codigo (string), nombre (string), stock (int), valorunitario (double)
# Usa: @inject ApiService Api, @inject IJSRuntime JS
# Incluye: confirmacion antes de eliminar y actualizar, mensajes de exito/error
# Tiene: formulario que se muestra/oculta, tabla de datos, limite de registros

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: agregar pagina CRUD Producto"  # commit
git push -u origin crud-producto-pagina    # subir rama

# --- En GitHub: crear PR crud-producto-pagina -> main, aprobar y merge ---
```

---

### Paso 8 — CRUD Empresa

**Rama:** `crud-empresa`

**Archivos creados:**
- `Components/Pages/Empresa.razor` (NUEVO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios
git checkout -b crud-empresa               # crear rama para CRUD Empresa

# --- Crear Components/Pages/Empresa.razor ---
# Pagina con @page "/empresa"
# Tabla simple con 2 campos: codigo (string), nombre (string)
# Misma estructura que Producto pero mas sencilla

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: agregar pagina CRUD Empresa"  # commit
git push -u origin crud-empresa            # subir rama

# --- En GitHub: crear PR crud-empresa -> main, aprobar y merge ---
# IMPORTANTE: hacer merge de Empresa ANTES que Cliente (Est2 depende de esta tabla)
```

---

### Paso 9 — CRUD Ruta

**Rama:** `crud-ruta`

**Archivos creados:**
- `Components/Pages/Ruta.razor` (NUEVO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios
git checkout -b crud-ruta                  # crear rama para CRUD Ruta

# --- Crear Components/Pages/Ruta.razor ---
# Pagina con @page "/ruta"
# Campos: ruta (string, clave primaria), descripcion (string)
# Particularidad: la clave primaria se llama "ruta", igual que la tabla

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: agregar pagina CRUD Ruta"  # commit
git push -u origin crud-ruta               # subir rama

# --- En GitHub: crear PR crud-ruta -> main, aprobar y merge ---
```

---

### Paso 10 — SpService (Stored Procedures)

**Rama:** `sp-service`

**Archivos creados/modificados:**
- `Services/SpService.cs` (NUEVO)
- `Program.cs` (MODIFICADO — registrar SpService)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios
git checkout -b sp-service                 # crear rama para SpService

# --- Crear Services/SpService.cs ---
# Contiene: EjecutarSpAsync(nombreSP, parametros)
# Endpoint: POST /api/procedimientos/ejecutarsp
# Retorna: (bool exito, List<Dictionary<string,object?>> resultados, string mensaje)
# Tiene su propio ConvertirDatos igual que ApiService

# --- Modificar Program.cs ---
# Agregar: builder.Services.AddScoped<FrontBlazorTutorial.Services.SpService>();

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: agregar SpService para stored procedures"  # commit
git push -u origin sp-service              # subir rama

# --- En GitHub: crear PR sp-service -> main, aprobar y merge ---
# IMPORTANTE: hacer merge ANTES de que Est2 suba su PR de Factura
```

---

## Estudiante 2

Estudiante 2 se enfoca en las tablas que tienen llaves foraneas y la factura (la pagina mas compleja).

---

### Paso 3 — Clonar el Repositorio

```powershell
# --- Aceptar invitacion ---
# En GitHub: ir a Notifications, aceptar invitacion al repositorio

# --- Clonar ---
cd C:\Users\TU_USUARIO\Desktop\proyectoscsharp                  # ir a la carpeta de proyectos
git clone https://github.com/USUARIO_EST1/FrontBlazorTutorial.git  # clonar el repositorio
cd FrontBlazorTutorial                                           # entrar al proyecto
dotnet run                                                       # verificar que funciona
```

---

### Paso 7 — CRUD Persona

**Rama:** `crud-persona`

**Archivos creados:**
- `Components/Pages/Persona.razor` (NUEVO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios (incluye Producto mergeado)
git checkout -b crud-persona               # crear rama para CRUD Persona

# --- Crear Components/Pages/Persona.razor ---
# Pagina con @page "/persona"
# Campos: codigo (string), nombre (string), email (string), telefono (string)
# Misma estructura que Producto, cambian los campos y la tabla
# Incluye confirmacion antes de eliminar y actualizar

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: agregar pagina CRUD Persona"  # commit
git push -u origin crud-persona            # subir rama

# --- En GitHub: crear PR crud-persona -> main ---
# Estudiante 1 revisa, aprueba y hace merge
```

---

### Paso 8 — CRUD Cliente (con llaves foraneas)

**Rama:** `crud-cliente`

**Depende de:** Empresa (Est1) y Persona (Est2) deben estar mergeados en main

**Archivos creados:**
- `Components/Pages/Cliente.razor` (NUEVO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios (incluye Empresa y Persona)
git checkout -b crud-cliente               # crear rama para CRUD Cliente

# --- Crear Components/Pages/Cliente.razor ---
# Pagina con @page "/cliente"
# Campos: id (int, auto), credito (double), fkcodpersona (select), fkcodempresa (select opcional)
# Diferencias con los CRUDs anteriores:
#   - Carga datos de persona y empresa con Api.ListarAsync("persona") y Api.ListarAsync("empresa")
#   - Muestra selects (<select>) en lugar de inputs para las FK
#   - El id no se envia al crear (lo genera la BD)
#   - La tabla muestra nombres en lugar de codigos de FK

# --- Si Empresa aun no esta mergeada, esperar o traer cambios despues ---
git fetch origin                           # descargar cambios de GitHub sin aplicarlos
git merge origin/main                      # traer lo nuevo de main a esta rama

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: agregar pagina CRUD Cliente con llaves foraneas"  # commit
git push -u origin crud-cliente            # subir rama

# --- En GitHub: crear PR crud-cliente -> main ---
# Estudiante 1 revisa, aprueba y hace merge
```

---

### Paso 9 — CRUD Vendedor (con llave foranea)

**Rama:** `crud-vendedor`

**Depende de:** Persona debe estar mergeada en main

**Archivos creados:**
- `Components/Pages/Vendedor.razor` (NUEVO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios
git checkout -b crud-vendedor              # crear rama para CRUD Vendedor

# --- Crear Components/Pages/Vendedor.razor ---
# Pagina con @page "/vendedor"
# Campos: id (int, auto), carnet (int), direccion (string), fkcodpersona (select)
# Similar a Cliente: carga personas para el select
# El id no se envia al crear (lo genera la BD)
# La tabla muestra el nombre de la persona en lugar del codigo

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: agregar pagina CRUD Vendedor"  # commit
git push -u origin crud-vendedor           # subir rama

# --- En GitHub: crear PR crud-vendedor -> main ---
# Estudiante 1 revisa, aprueba y hace merge
```

---

### Paso 10 — CRUD Factura (Maestro-Detalle con SPs)

**Rama:** `crud-factura`

**Depende de:** SpService (Est1), Cliente (Est2), Vendedor (Est2) deben estar mergeados

**Archivos creados:**
- `Components/Pages/Factura.razor` (NUEVO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios (incluye SpService)
git checkout -b crud-factura               # crear rama para Factura

# --- Si SpService aun no esta mergeado, esperar ---
# --- Si ya se creo la rama pero SpService se mergeo despues ---
git fetch origin                           # descargar cambios de GitHub sin aplicarlos
git merge origin/main                      # traer SpService a esta rama

# --- Crear Components/Pages/Factura.razor ---
# Pagina con @page "/factura"
# Usa @inject SpService Sp (NO ApiService)
# Pagina maestro-detalle:
#   MAESTRO: factura (numero, cliente, vendedor, fecha, total)
#   DETALLE: productos de la factura (codigo, nombre, cantidad, valor, subtotal)
#
# Stored Procedures que usa:
#   - sp_listar_facturas: listar todas las facturas
#   - sp_ver_factura: ver detalle de una factura con sus productos
#   - sp_crear_factura: crear factura con sus productos
#   - sp_editar_factura: editar factura existente
#   - sp_eliminar_factura: eliminar factura y sus productos
#
# Funcionalidad especial:
#   - Selects para cliente y vendedor
#   - Lista dinamica de productos (agregar/quitar filas)
#   - Vista de detalle que muestra los productos de una factura
#   - Calculo de subtotales y total

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: agregar pagina Factura con stored procedures"  # commit
git push -u origin crud-factura            # subir rama

# --- En GitHub: crear PR crud-factura -> main ---
# Estudiante 1 revisa, aprueba y hace merge
```

---

## Estudiante 3

Estudiante 3 trabaja en las tablas mas simples y en tareas de soporte (NavMenu, Home).

---

### Paso 3 — Clonar el Repositorio

```powershell
# --- Aceptar invitacion ---
# En GitHub: ir a Notifications, aceptar invitacion al repositorio

# --- Clonar ---
cd C:\Users\TU_USUARIO\Desktop\proyectoscsharp                  # ir a la carpeta de proyectos
git clone https://github.com/USUARIO_EST1/FrontBlazorTutorial.git  # clonar el repositorio
cd FrontBlazorTutorial                                           # entrar al proyecto
dotnet run                                                       # verificar que funciona
```

---

### Paso 7 — CRUD Usuario

**Rama:** `crud-usuario`

**Archivos creados:**
- `Components/Pages/Usuario.razor` (NUEVO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios (incluye Producto mergeado)
git checkout -b crud-usuario               # crear rama para CRUD Usuario

# --- Crear Components/Pages/Usuario.razor ---
# Pagina con @page "/usuario"
# Campos: codigo (string), nombre (string), email (string), clave (string)
# Diferencia: el input de clave usa type="password"
# Misma estructura que Producto/Persona

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: agregar pagina CRUD Usuario"  # commit
git push -u origin crud-usuario            # subir rama

# --- En GitHub: crear PR crud-usuario -> main ---
# Estudiante 1 revisa, aprueba y hace merge
```

---

### Paso 8 — CRUD Rol

**Rama:** `crud-rol`

**Archivos creados:**
- `Components/Pages/Rol.razor` (NUEVO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios
git checkout -b crud-rol                   # crear rama para CRUD Rol

# --- Crear Components/Pages/Rol.razor ---
# Pagina con @page "/rol"
# Campos: id (int), nombre (string)
# Diferencia: la clave primaria es "id" (int), no "codigo" (string)
# El input de id usa type="number"
# Al llamar a la API, se usa id.ToString() para convertir a string

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: agregar pagina CRUD Rol"  # commit
git push -u origin crud-rol                # subir rama

# --- En GitHub: crear PR crud-rol -> main ---
# Estudiante 1 revisa, aprueba y hace merge
```

---

### Paso 9 — Actualizar NavMenu

**Rama:** `actualizar-navmenu`

**Archivos modificados:**
- `Components/Layout/NavMenu.razor` (MODIFICADO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios
git checkout -b actualizar-navmenu         # crear rama para actualizar el menu

# --- Modificar Components/Layout/NavMenu.razor ---
# Agregar links a las paginas nuevas: Cliente, Vendedor, Factura
# Cada link usa <NavLink> con el href correspondiente

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: agregar links de Cliente, Vendedor y Factura al NavMenu"  # commit
git push -u origin actualizar-navmenu      # subir rama

# --- En GitHub: crear PR actualizar-navmenu -> main ---
# Estudiante 1 revisa, aprueba y hace merge
```

---

### Paso 10 — Actualizar Home

**Rama:** `actualizar-home`

**Archivos modificados:**
- `Components/Pages/Home.razor` (MODIFICADO)

```powershell
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios
git checkout -b actualizar-home            # crear rama para actualizar Home

# --- Modificar Components/Pages/Home.razor ---
# Actualizar la lista de tablas disponibles:
# Antes: Producto, Persona, Usuario, Empresa, Rol, Ruta
# Despues: Producto, Persona, Usuario, Empresa, Rol, Ruta, Cliente, Vendedor, Factura

# --- Verificar y subir ---
dotnet build                               # verificar que compila
git add .                                  # agregar cambios
git commit -m "feat: actualizar Home con lista completa de tablas"  # commit
git push -u origin actualizar-home         # subir rama

# --- En GitHub: crear PR actualizar-home -> main ---
# Estudiante 1 revisa, aprueba y hace merge
```

---

## Cronologia de Pull Requests

Esta tabla muestra el orden en que se crearon y mergearon los PRs a lo largo de todo el proyecto:

| # | Sprint | Paso | Rama | Descripcion | Responsable | Depende de |
|---|--------|------|------|-------------|-------------|------------|
| 1 | Sprint 2 | 4 | `api-service` | ApiService + conexion a la API | Est1 | - |
| 2 | Sprint 2 | 5 | `layout-navegacion-home` | Layout, NavMenu y Home | Est1 | PR #1 |
| 3 | Sprint 3 | 6 | `crud-producto-pagina` | CRUD Producto | Est1 | PR #2 |
| 4 | Sprint 4 | 7 | `crud-persona` | CRUD Persona | Est2 | PR #3 |
| 5 | Sprint 4 | 7 | `crud-usuario` | CRUD Usuario | Est3 | PR #3 |
| 6 | Sprint 5 | 8 | `crud-empresa` | CRUD Empresa | Est1 | PR #3 |
| 7 | Sprint 5 | 8 | `crud-rol` | CRUD Rol | Est3 | PR #3 |
| 8 | Sprint 5 | 8 | `crud-cliente` | CRUD Cliente (con FKs) | Est2 | PR #4, #6 |
| 9 | Sprint 6 | 9 | `crud-ruta` | CRUD Ruta | Est1 | - |
| 10 | Sprint 6 | 9 | `crud-vendedor` | CRUD Vendedor (con FK) | Est2 | PR #4 |
| 11 | Sprint 6 | 9 | `actualizar-navmenu` | NavMenu completo | Est3 | - |
| 12 | Sprint 7 | 10 | `sp-service` | SpService para SPs | Est1 | - |
| 13 | Sprint 7 | 10 | `actualizar-home` | Home actualizado | Est3 | - |
| 14 | Sprint 7 | 10 | `crud-factura` | Factura maestro-detalle | Est2 | PR #12 |

---

## Estado Final del Proyecto

### Estructura de archivos completa

```
FrontBlazorTutorial/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor              ← Est1 (Paso 5) - estructura visual principal
│   │   ├── MainLayout.razor.css          ← estilos del layout (viene con el template)
│   │   ├── NavMenu.razor                 ← Est1 (Paso 5) + Est3 (Paso 9) - menu lateral
│   │   └── NavMenu.razor.css             ← estilos del menu (viene con el template)
│   ├── Pages/
│   │   ├── Home.razor                    ← Est1 (Paso 5) + Est3 (Paso 10) - pagina de inicio
│   │   ├── Error.razor                   ← viene con el template
│   │   ├── Producto.razor                ← Est1 (Paso 6) - CRUD Producto
│   │   ├── Persona.razor                 ← Est2 (Paso 7) - CRUD Persona
│   │   ├── Usuario.razor                 ← Est3 (Paso 7) - CRUD Usuario
│   │   ├── Empresa.razor                 ← Est1 (Paso 8) - CRUD Empresa
│   │   ├── Cliente.razor                 ← Est2 (Paso 8) - CRUD Cliente (con FKs)
│   │   ├── Rol.razor                     ← Est3 (Paso 8) - CRUD Rol
│   │   ├── Ruta.razor                    ← Est1 (Paso 9) - CRUD Ruta
│   │   ├── Vendedor.razor                ← Est2 (Paso 9) - CRUD Vendedor (con FK)
│   │   └── Factura.razor                 ← Est2 (Paso 10) - Factura maestro-detalle
│   ├── App.razor                         ← viene con el template
│   ├── Routes.razor                      ← viene con el template
│   └── _Imports.razor                    ← viene con el template
├── Services/
│   ├── ApiService.cs                     ← Est1 (Paso 4) - CRUD generico via API
│   └── SpService.cs                      ← Est1 (Paso 10) - Stored Procedures via API
├── Properties/
│   └── launchSettings.json               ← configuracion de puertos
├── wwwroot/                              ← archivos estaticos (CSS, JS, imagenes)
├── Program.cs                            ← Est1 (Paso 4, 10) - punto de entrada, registro de servicios
├── appsettings.json                      ← Est1 (Paso 4) - configuracion (ApiBaseUrl)
├── appsettings.Development.json          ← configuracion de desarrollo
├── FrontBlazorTutorial.csproj            ← archivo de proyecto .NET
└── FrontBlazorTutorial.sln               ← archivo de solucion
```

### Servicios registrados en Program.cs

| Servicio | Linea en Program.cs | Funcion |
|----------|---------------------|---------|
| `HttpClient` | `builder.Services.AddScoped(sp => new HttpClient { ... })` | Cliente HTTP con URL base de la API |
| `ApiService` | `builder.Services.AddScoped<ApiService>()` | CRUD generico (GET, POST, PUT, DELETE) |
| `SpService` | `builder.Services.AddScoped<SpService>()` | Ejecutar Stored Procedures |

### Paginas y sus URLs

| Pagina | URL | Tabla | Tipo | Responsable |
|--------|-----|-------|------|-------------|
| Home | `/` | - | Diagnostico | Est1 + Est3 |
| Producto | `/producto` | producto | CRUD simple | Est1 |
| Persona | `/persona` | persona | CRUD simple | Est2 |
| Usuario | `/usuario` | usuario | CRUD simple | Est3 |
| Empresa | `/empresa` | empresa | CRUD simple (2 campos) | Est1 |
| Cliente | `/cliente` | cliente | CRUD con FKs | Est2 |
| Rol | `/rol` | rol | CRUD con id int | Est3 |
| Ruta | `/ruta` | ruta | CRUD simple | Est1 |
| Vendedor | `/vendedor` | vendedor | CRUD con FK | Est2 |
| Factura | `/factura` | - (usa SPs) | Maestro-detalle | Est2 |

### Resumen de carga por estudiante

| Estudiante | Archivos creados | Archivos modificados | PRs |
|------------|-----------------|---------------------|-----|
| **Est1** | ApiService.cs, SpService.cs, Producto.razor, Empresa.razor, Ruta.razor | Program.cs, appsettings.json, MainLayout.razor, NavMenu.razor, Home.razor | 7 |
| **Est2** | Persona.razor, Cliente.razor, Vendedor.razor, Factura.razor | - | 4 |
| **Est3** | Usuario.razor, Rol.razor | NavMenu.razor, Home.razor | 4 |

---

## Comandos Git de Referencia Rapida

```powershell
# --- Comandos que se usan SIEMPRE antes de empezar una tarea nueva ---
git checkout main                          # volver a la rama principal
git pull                                   # descargar ultimos cambios
git checkout -b nombre-rama                # crear rama nueva para la tarea

# --- Comandos que se usan AL TERMINAR una tarea ---
dotnet build                               # verificar que compila
git add .                                  # agregar todos los cambios
git commit -m "feat: descripcion del cambio"  # crear commit con mensaje descriptivo
git push -u origin nombre-rama             # subir rama a GitHub
# Luego: crear PR en GitHub, pedir revision, merge

# --- Comandos para traer cambios de main a una rama existente ---
git fetch origin                           # descargar cambios de GitHub sin aplicarlos
git merge origin/main                      # aplicar cambios de main a la rama actual

# --- Comandos de consulta ---
git status                                 # ver estado actual (archivos modificados, rama)
git log --oneline -10                      # ver ultimos 10 commits
git branch                                 # ver ramas locales
git branch -a                              # ver todas las ramas (locales y remotas)
git diff                                   # ver cambios no agregados al staging
```

---

> Este archivo es el resumen completo del tutorial. Cada estudiante puede usarlo para repasar lo que hizo, verificar que no falta nada, o como guia para proyectos futuros.
