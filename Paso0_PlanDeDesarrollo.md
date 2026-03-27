# Paso 0 — Plan de Desarrollo y Buenas Prácticas

**Este paso se hace ANTES de escribir una sola línea de código.**

Independiente de la metodología de desarrollo que se use (Scrum, Kanban, RUP, ágil, por prototipos o híbridos), todo proyecto debe comenzar con un **plan de desarrollo** que defina qué se va a hacer, quién lo hace, cómo se va a trabajar y cuándo se entrega.

---

## 1. Buenas prácticas para trabajo colaborativo en GitHub

### 1.1 Estrategia de ramas (Branching Strategy)

Para equipos pequeños (2-5 personas), se recomienda **GitHub Flow**:

```
main (siempre estable, código que funciona)
  ├── feature/crud-producto    (Estudiante 1 trabaja aquí)
  ├── feature/crud-persona     (Estudiante 2 trabaja aquí)
  └── feature/crud-usuario     (Estudiante 3 trabaja aquí)
```

**Reglas:**
- `main` siempre debe funcionar (se puede ejecutar `dotnet run` sin errores)
- Nadie hace push directo a `main` — todo se fusiona desde la terminal con `git fetch` + `git merge`
- Cada tarea tiene su propia rama
- Las ramas se borran después del merge

### 1.2 Convenciones para nombres de ramas

Usar prefijos que indiquen el tipo de trabajo:

| Prefijo | Uso | Ejemplo |
|---------|-----|---------|
| `feature/` | Nueva funcionalidad | `feature/crud-producto` |
| `fix/` | Corrección de errores | `fix/error-api-connection` |
| `refactor/` | Mejora de código sin cambiar funcionalidad | `refactor/api-service` |
| `docs/` | Documentación | `docs/manual-usuario` |
| `hotfix/` | Corrección urgente en producción | `hotfix/crash-al-guardar` |

**Ejemplo práctico para este proyecto:**

```
feature/crud-producto          ← Estudiante 1
feature/crud-persona           ← Estudiante 2
feature/crud-usuario           ← Estudiante 3
feature/layout-navegacion      ← Estudiante 1
feature/crud-factura           ← Estudiante 2
docs/actualizar-readme         ← Estudiante 3
fix/error-select-empresa       ← quien lo detecte (corrección de bug)
fix/api-timeout                ← quien lo detecte (corrección de bug)
refactor/extraer-servicio      ← mejora de código sin cambiar funcionalidad
docs/actualizar-readme         ← Estudiante 3 (documentación)
```

> **Nota:** El prefijo de la rama coincide con el tipo del commit. Si la rama es `feature/crud-producto`, los commits dentro de esa rama empiezan con `feat:`. Si la rama es `fix/error-select`, los commits empiezan con `fix:`.

### 1.3 Convenciones para mensajes de commit

Usar mensajes claros que digan **qué** se hizo:

**Formato recomendado:**
```
tipo: descripción corta
```

**Tipos comunes:**
| Tipo | Significado | Ejemplo |
|------|-------------|---------|
| `feat` | Nueva funcionalidad | `feat: agregar página CRUD Producto` |
| `fix` | Corrección de error | `fix: corregir error en select de empresas vacío` |
| `docs` | Documentación | `docs: agregar manual de instalación` |
| `style` | Formato (no cambia lógica) | `style: corregir indentación en Producto.razor` |
| `refactor` | Reestructuración de código | `refactor: extraer lógica de parseo a función separada` |
| `test` | Agregar o modificar pruebas | `test: agregar pruebas para ApiService` |

**Más ejemplos adaptados a Blazor:**
```
feat: agregar ApiService.cs para conexión con API
feat: agregar MainLayout.razor y NavMenu.razor
feat: agregar página CRUD Persona con formulario
feat: agregar página Factura con stored procedures
fix: corregir HttpClient que no enviaba body en PUT
fix: corregir @bind que no actualizaba el select
refactor: extraer formulario de Producto a componente separado
style: aplicar formato consistente en usings de Program.cs
docs: actualizar README con instrucciones de instalación
```

### 1.4 Convenciones para fusión de ramas

- Estudiante 1 fusiona las ramas de los demás desde la terminal con `git fetch origin` + `git merge origin/rama`
- Cada tarea va en su propia rama y se sube con `git push`
- Antes de fusionar, verificar que compila con `dotnet build`

### 1.5 Flujo de trabajo diario

```
1. git checkout main
2. git pull                          ← traer lo último
3. git checkout -b feature/mi-tarea  ← crear rama
4. (escribir código)
5. git add .
6. git commit -m "feat: descripción"
7. git push -u origin feature/mi-tarea
8. Estudiante 1 fusiona desde la terminal:
   git fetch origin
   git merge origin/feature/mi-tarea
   git push origin main
```

### 1.6 Cómo fusiona Estudiante 1

Cuando un estudiante sube su rama con `git push`, Estudiante 1 fusiona desde la terminal:

```bash
git checkout main
git fetch origin
git merge origin/nombre-de-la-rama
git push origin main
```

- `git fetch origin` trae todas las ramas del remoto sin modificar el código local.
- `git merge origin/nombre-de-la-rama` fusiona esa rama en `main`.
- `git push origin main` sube `main` actualizado a GitHub.

> Si al hacer merge aparece una pantalla de vim pidiendo un mensaje, escribir `:wq` y presionar Enter.

---

## 2. Plan de desarrollo

### 2.1 Descripción del problema

Se necesita un **frontend web** que permita gestionar las tablas de una base de datos de facturación (productos, personas, usuarios, empresas, roles, rutas, clientes, vendedores y facturas). El frontend se construye con **Blazor Server (.NET 9)** y se conecta a una API REST existente (`ApiGenericaCsharp`) que expone operaciones CRUD genéricas y Stored Procedures.

### 2.2 Tecnologías

| Tecnología | Uso |
|-----------|-----|
| **C# / .NET 9** | Lenguaje y plataforma |
| **Blazor Server** | Framework web (componentes interactivos) |
| **Razor** | Sintaxis para mezclar HTML con C# |
| **HttpClient** | Clase para hacer peticiones HTTP a la API |
| **Bootstrap 5** | Framework CSS para diseño visual (incluido en el template) |
| **Git / GitHub** | Control de versiones y colaboración |

### 2.3 Estructura del proyecto

```
FrontBlazorTutorial/
├── Program.cs                           ← Punto de entrada, configura servicios
├── appsettings.json                     ← Configuración (URL de la API)
├── FrontBlazorTutorial.csproj           ← Archivo de proyecto .NET
├── Services/
│   └── ApiService.cs                    ← Servicio que conecta con la API REST
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor             ← Layout principal (estructura)
│   │   └── NavMenu.razor                ← Menú de navegación lateral
│   ├── Pages/
│   │   ├── Home.razor                   ← Página de inicio
│   │   ├── Producto.razor               ← Página CRUD Producto
│   │   ├── Persona.razor                ← Página CRUD Persona
│   │   └── ...                          ← Una página por tabla
│   └── _Imports.razor                   ← Imports globales
├── wwwroot/                             ← Archivos estáticos (CSS, JS)
├── bin/                                 ← Compilados (NO se sube a GitHub)
└── obj/                                 ← Archivos temporales (NO se sube a GitHub)
```

### 2.4 Alcance

- Frontend Blazor Server con .NET 9
- 9 páginas CRUD (una por tabla)
- 1 página de facturación con maestro-detalle (Stored Procedures)
- Conexión a la API REST existente (ApiGenericaCsharp)
- Menú de navegación lateral
- Página de inicio con diagnóstico de conexión

**Fuera del alcance:**
- Autenticación / login
- Reportes o dashboards
- Despliegue en producción

### 2.5 Objetivos

1. Construir un frontend funcional en Blazor que consuma la API genérica
2. Practicar trabajo colaborativo con GitHub (ramas, merge desde terminal)
3. Aplicar el patrón de separación de responsabilidades (Pages / Services)
4. Implementar operaciones maestro-detalle con Stored Procedures

### 2.6 Descripción de entregables

| No. | Entregable | Descripción |
|-----|-----------|-------------|
| 1 | Proyecto Blazor | Proyecto creado con estructura, subido a GitHub |
| 2 | ApiService.cs | Servicio C# que conecta el frontend con la API REST usando HttpClient |
| 3 | Layout y navegación | MainLayout + NavMenu + página Home |
| 4 | CRUD Producto | Página .razor con listar, crear, editar, eliminar |
| 5 | CRUD Persona | Misma estructura que Producto, campos diferentes |
| 6 | CRUD Usuario | Misma estructura, campo clave tipo password |
| 7 | CRUD Empresa | Tabla simple, 2 campos |
| 8 | CRUD Rol | Clave primaria `id` (int) en lugar de `codigo` (string) |
| 9 | CRUD Cliente | Con llaves foráneas (selects a Persona y Empresa) |
| 10 | CRUD Ruta | Clave primaria con nombre igual a la tabla |
| 11 | CRUD Vendedor | Con llave foránea a Persona |
| 12 | Factura | Página maestro-detalle con Stored Procedures |
| 13 | NavMenu completo | Menú con links a todas las páginas |

### 2.7 Metodología

Se usa **Scrum adaptado** para un equipo de 3 personas:

| Elemento Scrum | Adaptación para este proyecto |
|----------------|-------------------------------|
| Sprint | Cada paso del tutorial es un sprint (1-2 horas) |
| Sprint Planning | Al inicio de cada paso, revisar qué hace cada estudiante |
| Daily Standup | Comunicación breve antes de empezar a codificar |
| Sprint Review | Verificar que la app corre (`dotnet run`) después de cada merge |
| Product Backlog | Lista de historias de usuario (sección 3) |
| Sprint Backlog | Tareas asignadas por estudiante en cada sprint (sección 5) |

**¿Por qué Scrum y no otra?**

| Metodología | ¿Aplica aquí? | Razón |
|-------------|---------------|-------|
| Scrum | Sí (adaptado) | Sprints cortos, entregas incrementales, roles claros |
| Kanban | Parcialmente | Bueno para visualizar tareas, pero no tiene sprints |
| RUP | No | Demasiado formal para un proyecto de 3 personas |
| Cascada | No | No permite cambios durante el desarrollo |
| Prototipos | Parcialmente | Cada CRUD es un prototipo funcional |
| Híbrido | Sí | Scrum + elementos de Kanban es lo más práctico |

### 2.8 Roles

| Rol Scrum | Quién | Responsabilidades en GitHub |
|-----------|-------|----------------------------|
| Product Owner | Profesor / Tutor | Define qué se construye, prioriza historias |
| Scrum Master | Estudiante 1 | Administra el repo, fusiona ramas desde la terminal, resuelve conflictos |
| Desarrollador | Estudiante 1 | Trabaja en sus ramas, sube sus ramas |
| Desarrollador | Estudiante 2 | Trabaja en sus ramas, sube sus ramas |
| Desarrollador | Estudiante 3 | Trabaja en sus ramas, sube sus ramas |

---

## 3. Historias de usuario

### HU-01: Configuración del proyecto

| Campo | Valor |
|-------|-------|
| **Número** | 1 |
| **Nombre** | Configuración inicial del proyecto Blazor |
| **Usuario** | Profesor (Product Owner) |
| **Prioridad** | Alta |
| **Riesgo** | Bajo |
| **Horas estimadas** | 2 |
| **Iteración** | Sprint 1 (Pasos 1-3) |
| **Responsable** | Estudiante 1 |

**Descripción:**
Yo como profesor, quiero que el equipo cree un proyecto Blazor Server, lo suba a GitHub y configure el repositorio con los 3 estudiantes como colaboradores.

**Criterios de aceptación:**
- El proyecto se ejecuta con `dotnet run` y muestra una página en el navegador
- El repositorio está en GitHub con los 3 estudiantes como colaboradores
- Cada tarea tiene su propia rama y se fusiona desde la terminal
- Cada estudiante puede clonar y ejecutar
- El `.gitignore` excluye `bin/` y `obj/`

---

### HU-02: Conexión con la API

| Campo | Valor |
|-------|-------|
| **Número** | 2 |
| **Nombre** | Servicio de conexión con la API REST |
| **Prioridad** | Alta |
| **Riesgo** | Medio |
| **Horas estimadas** | 3 |
| **Iteración** | Sprint 2 (Paso 4) |
| **Responsable** | Estudiante 1 |

**Descripción:**
Yo como profesor, quiero que el frontend se conecte a la API `ApiGenericaCsharp`, para poder listar, crear, editar y eliminar registros de cualquier tabla.

**Criterios de aceptación:**
- Existe `Services/ApiService.cs` con clase `ApiService`
- Métodos: `Listar(tabla)`, `Crear(tabla, datos)`, `Actualizar(tabla, clave, valor, datos)`, `Eliminar(tabla, clave, valor)`
- La URL de la API se configura en `appsettings.json`
- Usa `HttpClient` para las peticiones HTTP
- El servicio está registrado en `Program.cs`

---

### HU-03: Layout y navegación

| Campo | Valor |
|-------|-------|
| **Número** | 3 |
| **Nombre** | Layout, menú de navegación y página Home |
| **Prioridad** | Alta |
| **Horas estimadas** | 2 |
| **Iteración** | Sprint 2 (Paso 5) |
| **Responsable** | Estudiante 1 |

**Descripción:**
Yo como profesor, quiero que la aplicación tenga un layout con menú lateral, una página Home con info de conexión a la BD, y que todas las páginas hereden del layout base.

**Criterios de aceptación:**
- Existe `MainLayout.razor` con estructura Bootstrap
- Existe `NavMenu.razor` con links a todas las tablas
- La página Home muestra proveedor, base de datos y versión
- Existe `Home.razor` con `@page "/"`

---

### HU-04: CRUD Producto

| Campo | Valor |
|-------|-------|
| **Número** | 4 |
| **Nombre** | Gestión de productos |
| **Prioridad** | Alta |
| **Horas estimadas** | 3 |
| **Iteración** | Sprint 3 (Paso 6) |
| **Responsable** | Estudiante 1 |
| **Rama** | `crud-producto` |

**Descripción:**
Yo como profesor, quiero poder gestionar los productos (listar, crear, editar y eliminar) desde una página web.

**Criterios de aceptación:**
- Existe `Components/Pages/Producto.razor` con tabla y formulario
- Puedo crear un producto nuevo con código, nombre, stock y valor unitario
- Puedo editar un producto existente (el código no se puede cambiar)
- Al eliminar, se muestra confirmación con JavaScript
- Se muestra mensaje de éxito o error

---

### HU-05: CRUD Persona

| Campo | Valor |
|-------|-------|
| **Número** | 5 |
| **Nombre** | Gestión de personas |
| **Prioridad** | Media |
| **Horas estimadas** | 2 |
| **Iteración** | Sprint 4 (Paso 7) |
| **Responsable** | Estudiante 2 |
| **Rama** | `crud-persona` |

**Descripción:**
Yo como profesor, quiero poder gestionar las personas desde `/persona`.

**Criterios de aceptación:**
- Campos: codigo, nombre, email, telefono
- Misma estructura visual que Producto
- Confirmación antes de eliminar y actualizar

---

### HU-06: CRUD Usuario

| Campo | Valor |
|-------|-------|
| **Número** | 6 |
| **Nombre** | Gestión de usuarios |
| **Prioridad** | Media |
| **Horas estimadas** | 2 |
| **Iteración** | Sprint 4 (Paso 7) |
| **Responsable** | Estudiante 3 |
| **Rama** | `crud-usuario` |

**Descripción:**
Yo como profesor, quiero poder gestionar los usuarios desde `/usuario`. El campo clave debe mostrarse como password.

**Criterios de aceptación:**
- Campos: codigo, nombre, email, clave
- El input de clave usa `type="password"`
- Misma estructura que Producto

---

### HU-07: CRUD Empresa

| Campo | Valor |
|-------|-------|
| **Número** | 7 |
| **Nombre** | Gestión de empresas |
| **Prioridad** | Media |
| **Horas estimadas** | 1 |
| **Iteración** | Sprint 5 (Paso 8) |
| **Responsable** | Estudiante 1 |
| **Rama** | `crud-empresa` |

**Descripción:**
Tabla simple con solo codigo y nombre.

**Criterios de aceptación:**
- Campos: codigo, nombre
- CRUD completo con confirmaciones

---

### HU-08: CRUD Rol

| Campo | Valor |
|-------|-------|
| **Número** | 8 |
| **Nombre** | Gestión de roles |
| **Prioridad** | Media |
| **Horas estimadas** | 1 |
| **Iteración** | Sprint 5 (Paso 8) |
| **Responsable** | Estudiante 3 |
| **Rama** | `crud-rol` |

**Descripción:**
La clave primaria es `id` (int), no `codigo` (string).

**Criterios de aceptación:**
- Campos: id (numérico), nombre
- El input de id usa `type="number"`

---

### HU-09: CRUD Cliente (con llaves foráneas)

| Campo | Valor |
|-------|-------|
| **Número** | 9 |
| **Nombre** | Gestión de clientes |
| **Prioridad** | Media |
| **Horas estimadas** | 4 |
| **Iteración** | Sprint 5 (Paso 8) |
| **Responsable** | Estudiante 2 |
| **Rama** | `crud-cliente` |
| **Depende de** | HU-05 (Persona), HU-07 (Empresa) |

**Descripción:**
Cada cliente está asociado a una persona y opcionalmente a una empresa. Los campos FK deben mostrarse como selects que cargan datos de las tablas persona y empresa.

**Criterios de aceptación:**
- Campos: id (auto), credito, persona (select), empresa (select opcional)
- La página carga las listas de personas y empresas
- La tabla muestra nombres en lugar de códigos
- El id no se envía al crear (lo genera la BD)

---

### HU-10: CRUD Ruta

| Campo | Valor |
|-------|-------|
| **Número** | 10 |
| **Nombre** | Gestión de rutas |
| **Prioridad** | Baja |
| **Horas estimadas** | 1 |
| **Iteración** | Sprint 6 (Paso 9) |
| **Responsable** | Estudiante 1 |
| **Rama** | `crud-ruta` |

**Descripción:**
La clave primaria se llama `ruta` (igual que la tabla).

**Criterios de aceptación:**
- Campos: ruta, descripción

---

### HU-11: CRUD Vendedor

| Campo | Valor |
|-------|-------|
| **Número** | 11 |
| **Nombre** | Gestión de vendedores |
| **Prioridad** | Media |
| **Horas estimadas** | 3 |
| **Iteración** | Sprint 6 (Paso 9) |
| **Responsable** | Estudiante 2 |
| **Rama** | `crud-vendedor` |
| **Depende de** | HU-05 (Persona) |

**Descripción:**
Cada vendedor está asociado a una persona.

**Criterios de aceptación:**
- Campos: id (auto), carnet (numérico), direccion, persona (select)
- La tabla muestra el nombre de la persona

---

### HU-12: Menú completo

| Campo | Valor |
|-------|-------|
| **Número** | 12 |
| **Nombre** | Actualizar menú de navegación |
| **Prioridad** | Baja |
| **Horas estimadas** | 1 |
| **Iteración** | Sprint 6 (Paso 9) |
| **Responsable** | Estudiante 3 |
| **Rama** | `actualizar-navmenu` |

**Descripción:**
El menú lateral debe tener links a TODAS las páginas, incluyendo Cliente, Vendedor y Factura.

**Criterios de aceptación:**
- `NavMenu.razor` tiene links a las 9 tablas + Home

---

### HU-13: Factura (maestro-detalle)

| Campo | Valor |
|-------|-------|
| **Número** | 13 |
| **Nombre** | Gestión de facturas con productos |
| **Prioridad** | Alta |
| **Horas estimadas** | 8 |
| **Iteración** | Sprint 7 (Paso 10) |
| **Responsable** | Estudiante 2 |
| **Rama** | `crud-factura` |
| **Depende de** | HU-09 (Cliente), HU-11 (Vendedor) |

**Descripción:**
Cada factura tiene un cliente, un vendedor y una lista dinámica de productos con cantidades. Usa Stored Procedures a través de la API.

**Criterios de aceptación:**
- Listar facturas con número, cliente, vendedor, fecha, total
- Ver detalle de una factura con sus productos
- Crear factura seleccionando cliente, vendedor y agregando productos dinámicamente (JavaScript)
- Editar y eliminar factura con confirmación
- Usa endpoint `/api/procedimientos/ejecutarsp`

---

## 4. Reunión inicial del equipo

Antes de empezar a codificar, el equipo debe reunirse para definir:

### 4.1 Checklist de la reunión

- [ ] **Clonar el repositorio** — verificar que los 3 pueden clonar y ejecutar `dotnet run`
- [ ] **Acordar convenciones de ramas** — definir si usan `feature/`, `crud-`, u otro prefijo
- [ ] **Acordar convenciones de commits** — definir formato de mensajes
- [ ] **Revisar las historias de usuario** — entender qué hace cada uno
- [ ] **Asignar historias a cada sprint** — ver el cronograma (sección 5)
- [ ] **Definir flujo de merge** — quién fusiona las ramas desde la terminal
- [ ] **Definir canal de comunicación** — WhatsApp, Discord, Teams, etc.
- [ ] **Definir horario de trabajo** — cuándo se conectan, cuándo se reúnen

### 4.2 Preguntas que debe responder la reunión

| Pregunta | Ejemplo de respuesta |
|----------|---------------------|
| ¿Quién administra el repo? | Estudiante 1 |
| ¿Quién fusiona las ramas? | Estudiante 1 fusiona desde la terminal |
| ¿Qué pasa si hay conflicto? | Se resuelve en GitHub o se pide ayuda al Scrum Master |
| ¿Cómo nos avisamos que una rama está lista? | Mensaje en el grupo de WhatsApp |
| ¿Cada cuánto hacemos pull de main? | Antes de crear cada rama nueva |
| ¿Qué hacemos si alguien se atrasa? | Se redistribuyen tareas en el siguiente sprint |

---

## 5. Cronograma por sprints

| Sprint | Paso | Entregables | Est1 | Est2 | Est3 | Horas |
|--------|------|-------------|------|------|------|-------|
| **Sprint 1** | 1-3 | Proyecto + GitHub + ramas | Crear repo, invitar | Clonar, crear rama | Clonar, crear rama | 3 |
| **Sprint 2** | 4-5 | ApiService + Layout + Home | ApiService + Layout + Home | Pull y verificar | Pull y verificar | 4 |
| **Sprint 3** | 6 | CRUD Producto | CRUD Producto | Pull y verificar | Pull y verificar | 3 |
| **Sprint 4** | 7 | CRUD Persona + Usuario | Fusionar ramas | CRUD Persona | CRUD Usuario | 4 |
| **Sprint 5** | 8 | CRUD Empresa + Cliente + Rol | CRUD Empresa | CRUD Cliente | CRUD Rol | 6 |
| **Sprint 6** | 9 | CRUD Ruta + Vendedor + NavMenu | CRUD Ruta | CRUD Vendedor | NavMenu | 4 |
| **Sprint 7** | 10 | Factura + Home | Home actualizado | CRUD Factura | Revisar + verificar | 6 |
| | | | | | **Total estimado:** | **30** |

### Distribución de carga por estudiante

| Estudiante | Historias asignadas | Horas estimadas |
|------------|-------------------|-----------------|
| Estudiante 1 | HU-01, HU-02, HU-03, HU-04, HU-07, HU-10 | 12 |
| Estudiante 2 | HU-05, HU-09, HU-11, HU-13 | 17 |
| Estudiante 3 | HU-06, HU-08, HU-12 + Home | 6 |

**Nota:** Estudiante 1 tiene más historias pero son más simples. Estudiante 2 tiene menos historias pero más complejas (llaves foráneas, factura). Estudiante 3 tiene menos carga y puede apoyar en pruebas y verificación.

---

## 6. Sprint Backlog — Tareas por desarrollador

### Sprint 4 (ejemplo detallado)

| Tarea | Responsable | Rama | Estado | Depende de |
|-------|-------------|------|--------|------------|
| Crear `Components/Pages/Persona.razor` | Estudiante 2 | `crud-persona` | Pendiente | Sprint 3 mergeado |
| Crear `Components/Pages/Usuario.razor` | Estudiante 3 | `crud-usuario` | Pendiente | Sprint 3 mergeado |
| Commit + push Persona | Estudiante 2 | `crud-persona` | Pendiente | Archivos creados |
| Commit + push Usuario | Estudiante 3 | `crud-usuario` | Pendiente | Archivos creados |
| Estudiante 1 fusiona Persona: fetch + merge | Estudiante 1 | - | Pendiente | Push Persona |
| Estudiante 1 fusiona Usuario: fetch + merge | Estudiante 1 | - | Pendiente | Push Usuario |
| Pull de main (los 3) | Todos | - | Pendiente | Merges |

### Sprint 5 (ejemplo con dependencias)

| Tarea | Responsable | Rama | Estado | Depende de |
|-------|-------------|------|--------|------------|
| Crear CRUD Empresa | Estudiante 1 | `crud-empresa` | Pendiente | - |
| Crear CRUD Rol | Estudiante 3 | `crud-rol` | Pendiente | - |
| Crear CRUD Cliente | Estudiante 2 | `crud-cliente` | Pendiente | **Empresa mergeado** |
| Estudiante 1 fusiona Empresa: fetch + merge | Estudiante 1 | - | Pendiente | Empresa creado |
| Estudiante 1 fusiona Rol: fetch + merge | Estudiante 1 | - | Pendiente | Rol creado |
| fetch + merge origin/main | Estudiante 2 | `crud-cliente` | Pendiente | Empresa mergeado |
| Estudiante 1 fusiona Cliente: fetch + merge | Estudiante 1 | - | Pendiente | Cliente creado |

**Notar:** Cliente depende de Empresa. Estudiante 2 debe esperar a que Empresa esté mergeado, luego hacer `git fetch origin && git merge origin/main` en su rama antes de subir sus cambios.

---

## 7. Resumen de convenciones acordadas

Este es un ejemplo de lo que el equipo debe definir en la reunión inicial:

| Convención | Acuerdo |
|------------|---------|
| **Prefijo de ramas** | `feature/`, `fix/`, `docs/` |
| **Formato de commit** | `tipo: descripción` (feat, fix, docs, refactor) |
| **Quién hace merge** | Estudiante 1 (Scrum Master) |
| **Quién fusiona ramas** | Estudiante 1 fusiona desde la terminal con fetch + merge |
| **Canal de comunicación** | Grupo de WhatsApp |
| **Cuándo hacer pull** | Siempre antes de crear una rama nueva |
| **Qué hacer si hay conflicto** | Resolverlo en GitHub o pedir ayuda |
| **Cuándo se reúnen** | Al inicio de cada sprint (cada paso del tutorial) |

---

> **Siguiente paso:** Paso 1 — Conceptos Básicos de Blazor.
