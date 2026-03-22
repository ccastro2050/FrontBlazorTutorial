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
- `main` siempre debe compilar y funcionar
- Nadie hace push directo a `main` — todo entra por Pull Request
- Cada tarea tiene su propia rama
- Las ramas se borran después del merge

### 1.2 Convenciones para nombres de ramas

Usar prefijos que indiquen el tipo de trabajo:

| Prefijo | Uso | Ejemplo |
|---------|-----|---------|
| `feature/` | Nueva funcionalidad | `feature/crud-producto` |
| `fix/` | Corrección de errores | `fix/error-login` |
| `refactor/` | Mejora de código sin cambiar funcionalidad | `refactor/apiservice` |
| `docs/` | Documentación | `docs/manual-usuario` |
| `hotfix/` | Corrección urgente en producción | `hotfix/crash-al-guardar` |

**Ejemplo práctico para este proyecto:**

```
feature/crud-producto          ← Estudiante 1
feature/crud-persona           ← Estudiante 2
feature/crud-usuario           ← Estudiante 3
feature/layout-navegacion      ← Estudiante 1
feature/sp-service             ← Estudiante 1
feature/crud-factura           ← Estudiante 2
docs/actualizar-readme         ← Estudiante 3
fix/error-select-empresa       ← quien lo detecte
```

### 1.3 Convenciones para mensajes de commit

Usar mensajes claros que digan **qué** se hizo:

**Formato recomendado:**
```
tipo: descripción corta

Ejemplos:
feat: agregar pagina CRUD Producto
feat: agregar SpService para stored procedures
fix: corregir error en select de empresas vacío
refactor: extraer lógica de parseo JSON a método separado
docs: agregar manual de instalación
style: corregir indentación en Factura.razor
```

**Tipos comunes:**
| Tipo | Significado |
|------|-------------|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de error |
| `docs` | Documentación |
| `style` | Formato (no cambia lógica) |
| `refactor` | Reestructuración de código |
| `test` | Agregar o modificar pruebas |

### 1.4 Convenciones para Pull Requests

- **Título claro:** describir qué hace el PR en una oración
- **Descripción:** explicar qué se hizo, por qué y cómo probarlo
- **Un PR por tarea:** no mezclar varias funcionalidades en un PR
- **Revisar antes de aprobar:** al menos una persona revisa el código

**Ejemplo de PR bien hecho:**

```
Título: Agregar página CRUD Producto

Descripción:
- Se creó Components/Pages/Producto.razor
- Permite listar, crear, editar y eliminar productos
- Usa ApiService para conectarse a la API
- Incluye confirmación antes de eliminar

¿Cómo probarlo?
1. Ejecutar dotnet run
2. Ir a /producto
3. Verificar que se listan los productos de la BD
```

### 1.5 Flujo de trabajo diario

```
1. git checkout main
2. git pull                          ← traer lo último
3. git checkout -b feature/mi-tarea  ← crear rama
4. (escribir código)
5. git add .
6. git commit -m "feat: descripción"
7. git push -u origin feature/mi-tarea
8. Crear PR en GitHub
9. Otro miembro revisa y aprueba
10. Merge a main
11. Borrar la rama
```

---

## 2. Plan de desarrollo

### 2.1 Descripción del problema

Se necesita un **frontend web** que permita gestionar las tablas de una base de datos de facturación (productos, personas, usuarios, empresas, roles, rutas, clientes, vendedores y facturas). El frontend debe conectarse a una API REST existente (`ApiGenericaCsharp`) que expone operaciones CRUD genéricas y Stored Procedures.

### 2.2 Alcance

- Frontend Blazor Server con .NET 9
- 9 páginas CRUD (una por tabla)
- 1 página de facturación con maestro-detalle (Stored Procedures)
- Conexión a la API REST existente
- Menú de navegación lateral
- Página de inicio con diagnóstico de conexión

**Fuera del alcance:**
- Autenticación / login
- Reportes o dashboards
- Despliegue en producción

### 2.3 Objetivos

1. Construir un frontend funcional que consuma la API genérica
2. Practicar trabajo colaborativo con GitHub (ramas, PRs, merge)
3. Aplicar el patrón de inyección de dependencias en Blazor
4. Implementar operaciones maestro-detalle con Stored Procedures

### 2.4 Descripción de entregables

| No. | Entregable | Descripción |
|-----|-----------|-------------|
| 1 | Proyecto Blazor Server | Proyecto creado con `dotnet new blazor`, subido a GitHub |
| 2 | ApiService | Servicio C# que conecta el frontend con la API REST |
| 3 | Layout y navegación | Menú lateral con links a todas las páginas + página Home |
| 4 | CRUD Producto | Página con listar, crear, editar, eliminar |
| 5 | CRUD Persona | Misma estructura que Producto, campos diferentes |
| 6 | CRUD Usuario | Misma estructura, campo clave tipo password |
| 7 | CRUD Empresa | Tabla simple, 2 campos |
| 8 | CRUD Rol | Clave primaria `id` (int) en lugar de `codigo` (string) |
| 9 | CRUD Cliente | Con llaves foráneas (selects a Persona y Empresa) |
| 10 | CRUD Ruta | Clave primaria con nombre igual a la tabla |
| 11 | CRUD Vendedor | Con llave foránea a Persona |
| 12 | SpService | Servicio para ejecutar Stored Procedures |
| 13 | Factura | Página maestro-detalle con SPs |
| 14 | NavMenu completo | Menú con links a todas las páginas |

### 2.5 Metodología

Se usa **Scrum adaptado** para un equipo de 3 personas:

| Elemento Scrum | Adaptación para este proyecto |
|----------------|-------------------------------|
| Sprint | Cada paso del tutorial es un sprint (1-2 horas) |
| Sprint Planning | Al inicio de cada paso, revisar qué hace cada estudiante |
| Daily Standup | Comunicación breve antes de empezar a codificar |
| Sprint Review | Verificar que la app compila y funciona después de cada merge |
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

### 2.6 Roles

| Rol Scrum | Quién | Responsabilidades en GitHub |
|-----------|-------|----------------------------|
| Product Owner | Profesor / Tutor | Define qué se construye, prioriza historias |
| Scrum Master | Estudiante 1 | Administra el repo, revisa PRs, hace merge, resuelve conflictos |
| Desarrollador | Estudiante 1 | Trabaja en sus ramas, crea PRs |
| Desarrollador | Estudiante 2 | Trabaja en sus ramas, crea PRs |
| Desarrollador | Estudiante 3 | Trabaja en sus ramas, crea PRs |

---

## 3. Historias de usuario

### HU-01: Configuración del proyecto

| Campo | Valor |
|-------|-------|
| **Número** | 1 |
| **Nombre** | Configuración inicial del proyecto |
| **Usuario** | Profesor (Product Owner) |
| **Prioridad** | Alta |
| **Riesgo** | Bajo |
| **Horas estimadas** | 2 |
| **Iteración** | Sprint 1 (Pasos 1-3) |
| **Responsable** | Estudiante 1 |

**Descripción:**
Yo como profesor, quiero que el equipo cree un proyecto Blazor Server, lo suba a GitHub y configure el repositorio con las reglas de protección de ramas, para que los 3 estudiantes puedan colaborar de forma ordenada.

**Criterios de aceptación:**
- El proyecto compila con `dotnet run` y muestra la página de bienvenida de Blazor
- El repositorio está en GitHub con los 3 estudiantes como colaboradores
- La branch `main` está protegida (solo se puede integrar por PR)
- Cada estudiante puede clonar el repositorio y ejecutar el proyecto

---

### HU-02: Conexión con la API

| Campo | Valor |
|-------|-------|
| **Número** | 2 |
| **Nombre** | Conexión del frontend con la API REST |
| **Usuario** | Profesor (Product Owner) |
| **Prioridad** | Alta |
| **Riesgo** | Medio |
| **Horas estimadas** | 3 |
| **Iteración** | Sprint 2 (Paso 4) |
| **Responsable** | Estudiante 1 |

**Descripción:**
Yo como profesor, quiero que el frontend se conecte a la API `ApiGenericaCsharp` que corre en `localhost:5035`, para poder listar, crear, editar y eliminar registros de cualquier tabla.

**Criterios de aceptación:**
- Existe un archivo `Services/ApiService.cs` con métodos `ListarAsync`, `CrearAsync`, `ActualizarAsync`, `EliminarAsync`
- La URL de la API se configura en `appsettings.json`
- El servicio está registrado en `Program.cs`

---

### HU-03: Layout y navegación

| Campo | Valor |
|-------|-------|
| **Número** | 3 |
| **Nombre** | Layout, menú de navegación y página Home |
| **Usuario** | Profesor (Product Owner) |
| **Prioridad** | Alta |
| **Riesgo** | Bajo |
| **Horas estimadas** | 2 |
| **Iteración** | Sprint 2 (Paso 5) |
| **Responsable** | Estudiante 1 |

**Descripción:**
Yo como profesor, quiero que la aplicación tenga un menú lateral con links a todas las tablas y una página Home que muestre información de la conexión a la BD, para poder navegar fácilmente entre las secciones.

**Criterios de aceptación:**
- El menú lateral muestra links a: Producto, Persona, Usuario, Empresa, Rol, Ruta
- La página Home muestra el nombre de la BD, proveedor y versión
- Se eliminaron las páginas de ejemplo (Counter, Weather)

---

### HU-04: CRUD Producto

| Campo | Valor |
|-------|-------|
| **Número** | 4 |
| **Nombre** | Gestión de productos |
| **Usuario** | Profesor (Product Owner) |
| **Prioridad** | Alta |
| **Riesgo** | Bajo |
| **Horas estimadas** | 3 |
| **Iteración** | Sprint 3 (Paso 6) |
| **Responsable** | Estudiante 1 |
| **Rama** | `feature/crud-producto` |

**Descripción:**
Yo como profesor, quiero poder gestionar los productos (listar, crear, editar y eliminar) desde una página web, para verificar que la conexión con la API funciona correctamente.

**Criterios de aceptación:**
- La página `/producto` muestra una tabla con los productos existentes
- Puedo crear un producto nuevo con código, nombre, stock y valor unitario
- Puedo editar un producto existente (el código no se puede cambiar)
- Al eliminar, se muestra confirmación antes de borrar
- Se muestra mensaje de éxito o error después de cada operación

---

### HU-05: CRUD Persona

| Campo | Valor |
|-------|-------|
| **Número** | 5 |
| **Nombre** | Gestión de personas |
| **Usuario** | Profesor (Product Owner) |
| **Prioridad** | Media |
| **Horas estimadas** | 2 |
| **Iteración** | Sprint 4 (Paso 7) |
| **Responsable** | Estudiante 2 |
| **Rama** | `feature/crud-persona` |

**Descripción:**
Yo como profesor, quiero poder gestionar las personas (listar, crear, editar y eliminar) desde la página `/persona`.

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
| **Usuario** | Profesor (Product Owner) |
| **Prioridad** | Media |
| **Horas estimadas** | 2 |
| **Iteración** | Sprint 4 (Paso 7) |
| **Responsable** | Estudiante 3 |
| **Rama** | `feature/crud-usuario` |

**Descripción:**
Yo como profesor, quiero poder gestionar los usuarios desde la página `/usuario`. El campo clave debe mostrarse como password.

**Criterios de aceptación:**
- Campos: codigo, nombre, email, clave
- El input de clave usa `type="password"`
- Misma estructura visual que Producto

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
| **Rama** | `feature/crud-empresa` |

**Descripción:**
Yo como profesor, quiero poder gestionar empresas. Tabla simple con solo codigo y nombre.

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
| **Rama** | `feature/crud-rol` |

**Descripción:**
Yo como profesor, quiero poder gestionar roles. La clave primaria es `id` (int), no `codigo` (string).

**Criterios de aceptación:**
- Campos: id (numérico), nombre
- El input de id usa `type="number"`
- `campoId.ToString()` al pasar a la API

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
| **Rama** | `feature/crud-cliente` |
| **Depende de** | HU-05 (Persona), HU-07 (Empresa) |

**Descripción:**
Yo como profesor, quiero poder gestionar clientes. Cada cliente está asociado a una persona y opcionalmente a una empresa. Los campos de FK deben mostrarse como selects.

**Criterios de aceptación:**
- Campos: id (auto), credito, persona (select), empresa (select opcional)
- Los selects cargan datos de las tablas persona y empresa
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
| **Rama** | `feature/crud-ruta` |

**Descripción:**
Yo como profesor, quiero poder gestionar rutas. La clave primaria se llama `ruta` (igual que la tabla).

**Criterios de aceptación:**
- Campos: ruta, descripcion
- La API recibe `"ruta"` como nombre de tabla y como nombre de clave

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
| **Rama** | `feature/crud-vendedor` |
| **Depende de** | HU-05 (Persona) |

**Descripción:**
Yo como profesor, quiero poder gestionar vendedores. Cada vendedor está asociado a una persona.

**Criterios de aceptación:**
- Campos: id (auto), carnet (numérico), direccion, persona (select)
- La tabla muestra el nombre de la persona en lugar del código

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
| **Rama** | `feature/actualizar-navmenu` |

**Descripción:**
Yo como profesor, quiero que el menú lateral tenga links a TODAS las páginas, incluyendo Cliente, Vendedor y Factura.

**Criterios de aceptación:**
- NavMenu.razor tiene links a las 9 tablas + Home
- Los links nuevos funcionan correctamente

---

### HU-13: Servicio de Stored Procedures

| Campo | Valor |
|-------|-------|
| **Número** | 13 |
| **Nombre** | Servicio para ejecutar Stored Procedures |
| **Prioridad** | Alta |
| **Horas estimadas** | 2 |
| **Iteración** | Sprint 7 (Paso 10) |
| **Responsable** | Estudiante 1 |
| **Rama** | `feature/sp-service` |

**Descripción:**
Yo como profesor, quiero que exista un servicio SpService que permita ejecutar Stored Procedures a través de la API, para poder implementar la facturación.

**Criterios de aceptación:**
- Existe `Services/SpService.cs` con método `EjecutarSpAsync`
- El servicio está registrado en `Program.cs`
- Recibe nombre del SP + parámetros, retorna resultados

---

### HU-14: Factura (maestro-detalle)

| Campo | Valor |
|-------|-------|
| **Número** | 14 |
| **Nombre** | Gestión de facturas con productos |
| **Prioridad** | Alta |
| **Horas estimadas** | 8 |
| **Iteración** | Sprint 7 (Paso 10) |
| **Responsable** | Estudiante 2 |
| **Rama** | `feature/crud-factura` |
| **Depende de** | HU-13 (SpService), HU-09 (Cliente), HU-11 (Vendedor) |

**Descripción:**
Yo como profesor, quiero poder crear, ver, editar y eliminar facturas. Cada factura tiene un cliente, un vendedor y una lista dinámica de productos con cantidades.

**Criterios de aceptación:**
- Listar facturas con número, cliente, vendedor, fecha, total y cantidad de productos
- Ver detalle de una factura con sus productos (código, nombre, cantidad, valor, subtotal)
- Crear factura seleccionando cliente, vendedor y agregando productos dinámicamente
- Editar factura existente
- Eliminar factura con confirmación
- Usa Stored Procedures (no CRUD genérico)

---

## 4. Reunión inicial del equipo

Antes de empezar a codificar, el equipo debe reunirse para definir:

### 4.1 Checklist de la reunión

- [ ] **Clonar el repositorio** — verificar que los 3 pueden clonar y ejecutar `dotnet run`
- [ ] **Acordar convenciones de ramas** — definir si usan `feature/`, `crud-`, u otro prefijo
- [ ] **Acordar convenciones de commits** — definir formato de mensajes
- [ ] **Revisar las historias de usuario** — entender qué hace cada uno
- [ ] **Asignar historias a cada sprint** — ver el cronograma (sección 5)
- [ ] **Definir flujo de PRs** — quién revisa, quién aprueba, quién hace merge
- [ ] **Definir canal de comunicación** — WhatsApp, Discord, Teams, etc.
- [ ] **Definir horario de trabajo** — cuándo se conectan, cuándo se reúnen

### 4.2 Preguntas que debe responder la reunión

| Pregunta | Ejemplo de respuesta |
|----------|---------------------|
| ¿Quién administra el repo? | Estudiante 1 |
| ¿Quién revisa los PRs? | Estudiante 1 revisa los de 2 y 3. Estudiante 2 o 3 revisan los de 1 |
| ¿Qué pasa si hay conflicto? | Se resuelve en GitHub o se pide ayuda al Scrum Master |
| ¿Cómo nos avisamos que un PR está listo? | Mensaje en el grupo de WhatsApp |
| ¿Cada cuánto hacemos pull de main? | Antes de crear cada rama nueva |
| ¿Qué hacemos si alguien se atrasa? | Se redistribuyen tareas en el siguiente sprint |

---

## 5. Cronograma por sprints

| Sprint | Paso | Entregables | Est1 | Est2 | Est3 | Horas |
|--------|------|-------------|------|------|------|-------|
| **Sprint 1** | 1-3 | Proyecto + GitHub + ramas | Crear repo, invitar | Clonar, crear rama | Clonar, crear rama | 3 |
| **Sprint 2** | 4-5 | ApiService + Layout + Home | ApiService + Layout + Home | Pull y verificar | Pull y verificar | 4 |
| **Sprint 3** | 6 | CRUD Producto | CRUD Producto + PR | Pull y verificar | Pull y verificar | 3 |
| **Sprint 4** | 7 | CRUD Persona + Usuario | Revisar PRs + merge | CRUD Persona + PR | CRUD Usuario + PR | 4 |
| **Sprint 5** | 8 | CRUD Empresa + Cliente + Rol | CRUD Empresa + PR | CRUD Cliente + PR | CRUD Rol + PR | 6 |
| **Sprint 6** | 9 | CRUD Ruta + Vendedor + NavMenu | CRUD Ruta + PR | CRUD Vendedor + PR | NavMenu + PR | 4 |
| **Sprint 7** | 10 | SpService + Factura + Home | SpService + PR | CRUD Factura + PR | Home actualizado + PR | 6 |
| | | | | | **Total estimado:** | **30** |

### Distribución de carga por estudiante

| Estudiante | Historias asignadas | Horas estimadas |
|------------|-------------------|-----------------|
| Estudiante 1 | HU-01, HU-02, HU-03, HU-04, HU-07, HU-10, HU-13 | 14 |
| Estudiante 2 | HU-05, HU-09, HU-11, HU-14 | 17 |
| Estudiante 3 | HU-06, HU-08, HU-12 + Home | 6 |

**Nota:** Estudiante 1 tiene más historias pero son más simples (tablas de 2 campos). Estudiante 2 tiene menos historias pero más complejas (llaves foráneas, factura). Estudiante 3 tiene menos carga porque puede apoyar en revisión de PRs y pruebas.

---

## 6. Sprint Backlog — Tareas por desarrollador

### Sprint 4 (ejemplo detallado)

| Tarea | Responsable | Rama | Estado | Depende de |
|-------|-------------|------|--------|------------|
| Crear Persona.razor | Estudiante 2 | `feature/crud-persona` | Pendiente | Sprint 3 mergeado |
| Crear Usuario.razor | Estudiante 3 | `feature/crud-usuario` | Pendiente | Sprint 3 mergeado |
| Pull de main | Estudiante 2 | - | Pendiente | - |
| Pull de main | Estudiante 3 | - | Pendiente | - |
| Commit + push Persona | Estudiante 2 | `feature/crud-persona` | Pendiente | Persona.razor |
| Commit + push Usuario | Estudiante 3 | `feature/crud-usuario` | Pendiente | Usuario.razor |
| Crear PR Persona | Estudiante 2 | - | Pendiente | Push |
| Crear PR Usuario | Estudiante 3 | - | Pendiente | Push |
| Revisar PR Persona | Estudiante 1 | - | Pendiente | PR creado |
| Revisar PR Usuario | Estudiante 1 | - | Pendiente | PR creado |
| Merge PR Persona | Estudiante 1 | - | Pendiente | PR aprobado |
| Merge PR Usuario | Estudiante 1 | - | Pendiente | PR aprobado |
| Pull de main (los 3) | Todos | - | Pendiente | Merges |

### Sprint 5 (ejemplo con dependencias)

| Tarea | Responsable | Rama | Estado | Depende de |
|-------|-------------|------|--------|------------|
| Crear Empresa.razor | Estudiante 1 | `feature/crud-empresa` | Pendiente | - |
| Crear Rol.razor | Estudiante 3 | `feature/crud-rol` | Pendiente | - |
| Crear Cliente.razor | Estudiante 2 | `feature/crud-cliente` | Pendiente | **Empresa mergeado** |
| PR + merge Empresa | Estudiante 1 | - | Pendiente | Empresa.razor |
| PR + merge Rol | Estudiante 1 | - | Pendiente | Rol.razor |
| fetch + merge origin/main | Estudiante 2 | `feature/crud-cliente` | Pendiente | Empresa mergeado |
| PR + merge Cliente | Estudiante 1 | - | Pendiente | Cliente.razor |

**Notar:** Cliente depende de Empresa. Estudiante 2 debe esperar a que Empresa esté mergeado, luego hacer `git fetch origin && git merge origin/main` en su rama antes de subir su PR.

---

## 7. Resumen de convenciones acordadas

Este es un ejemplo de lo que el equipo debe definir en la reunión inicial:

| Convención | Acuerdo |
|------------|---------|
| **Prefijo de ramas** | `feature/`, `fix/`, `docs/` |
| **Formato de commit** | `tipo: descripción` (feat, fix, docs, refactor) |
| **Quién hace merge** | Estudiante 1 (Scrum Master) |
| **Quién revisa PRs** | Est1 revisa los de Est2 y Est3. Est2 o Est3 revisan los de Est1 |
| **Canal de comunicación** | Grupo de WhatsApp |
| **Cuándo hacer pull** | Siempre antes de crear una rama nueva |
| **Qué hacer si hay conflicto** | Resolverlo en GitHub o pedir ayuda |
| **Cuándo se reúnen** | Al inicio de cada sprint (cada paso del tutorial) |

---

> **Siguiente paso:** Paso 1 — Conceptos Básicos.
