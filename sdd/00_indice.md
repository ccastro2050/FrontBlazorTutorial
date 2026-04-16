# Spec Driven Development (SDD) - FrontBlazorTutorial

## Qué es SDD

Spec Driven Development invierte la estructura de poder en el desarrollo de software: **las especificaciones son el artefacto principal y el código es su expresión**. En vez de que el código sea la fuente de verdad, la documentación lo es.

> "Necesitamos pasar a una forma estandarizada de decirle a la IA: 'estas son las reglas de mi proyecto, no puedes saltártelas'. Eso es el SDD."

### Referencias

- Repositorio oficial Spec-Kit: [github.com/github/spec-kit](https://github.com/github/spec-kit)
- Repositorio oficial OpenSpec: [github.com/Fission-AI/OpenSpec](https://github.com/Fission-AI/OpenSpec)
- Documento técnico: [spec-driven.md](https://github.com/github/spec-kit/blob/main/spec-driven.md)
- Blog Microsoft: [Diving Into Spec-Driven Development With GitHub Spec Kit](https://developer.microsoft.com/blog/spec-driven-development-spec-kit)
- Video conceptual: [La forma CORRECTA de programar con IA en 2026: SDD](https://youtu.be/p2WA672HrdI)
- Video tutorial: [GitHub Spec Kit - Tutorial completo con ejemplo práctico](https://youtu.be/QzSCmSFKvko)
- Guía OpenSpec: [webreactiva.com/blog/openspec](https://www.webreactiva.com/blog/openspec)

---

## Las fases del SDD (según Spec-Kit)

| # | Fase | Comando Spec-Kit | Qué se hace | Documento |
|---|------|-----------------|-------------|-----------|
| 1 | **Constitución** | `/speckit.constitution` | Reglas no negociables: tecnologías, convenciones, prohibiciones | [01_constitucion.md](01_constitucion.md) |
| 2 | **Especificación** | `/speckit.specify` | Qué quieres construir, qué problema resuelve, para quién | [02_especificacion.md](02_especificacion.md) |
| 3 | **Clarificación** | `/speckit.clarify` | La IA hace preguntas sobre lo que olvidaste | [03_clarificacion.md](03_clarificacion.md) |
| 4 | **Plan** | `/speckit.plan` | Plan técnico: estructura, dependencias, diagramas | [04_plan.md](04_plan.md) |
| 5 | **Tareas y Código** | `/speckit.tasks` + `/speckit.implement` | Lista de tareas ejecutables, la IA escribe el código | [05_tareas.md](05_tareas.md) |

### Artefactos adicionales

| Artefacto | Qué contiene | Documento |
|-----------|-------------|-----------|
| **data-model.md** | Diagrama ER, SQL completo PostgreSQL/SqlServer, diccionario de datos | [data-model.md](data-model.md) |
| **Diagramas de secuencia** | Login, CRUD listar, acceso denegado (Mermaid) | [04_plan.md](04_plan.md) sección 7 |
| **Diagrama de clases** | ApiService, AuthService, MainLayout, NavMenu (Mermaid) | [04_plan.md](04_plan.md) sección 8 |
| **Herramientas SDD** | Spec-Kit + OpenSpec: qué son, cómo instalar, comparación | [06_herramientas_sdd.md](06_herramientas_sdd.md) |

### Principio central

> "Lo más importante del SDD es que la documentación es un entregable que se versiona, y el código es el resultado de esta documentación."

---

## Estructura de archivos SDD en este proyecto

```
FrontBlazorTutorial/
├── sdd/                                <- Carpeta SDD
│   ├── 00_indice.md                    <- ESTE ARCHIVO
│   ├── 01_constitucion.md              <- Reglas no negociables
│   ├── 02_especificacion.md            <- Qué se construye
│   ├── 03_clarificacion.md             <- Preguntas y decisiones
│   ├── 04_plan.md                      <- Plan técnico + diagramas Mermaid
│   ├── 05_tareas.md                    <- Tareas ejecutables
│   ├── 06_herramientas_sdd.md          <- Spec-Kit + OpenSpec (sin instalar)
│   └── data-model.md                   <- SQL completo + diccionario de datos
├── Paso0_PlanDeDesarrollo.md           <- Plan de desarrollo
├── Paso1 a Paso11                      <- Tutorial paso a paso
├── Paso12_LoginYControlDeAcceso.md     <- Login, JWT, BCrypt, middleware
└── Services/, Components/, ...         <- Código fuente (RESULTADO de las specs)
```

---

## Proyecto: FrontBlazorTutorial

| Aspecto | Valor |
|---------|-------|
| Nombre | FrontBlazorTutorial |
| Tipo | Frontend web educativo (tutorial paso a paso) |
| Stack | C# / .NET 9.0 / Blazor Server / Razor / Bootstrap 5 |
| API | ApiGenericaCsharp en `http://localhost:5035` |
| BD | PostgreSQL 17 (compatible con SqlServer) |
| Puerto | 5003 |
| Repositorio | GitHub |
| Audiencia | Estudiantes de Diseño de Software (universitarios) |
| Metodología | Spec Driven Development |

---

## Versionamiento de la documentación SDD

En SDD, la documentación se versiona **igual que el código** — vive dentro del repositorio
Git y se commitea junto con los cambios de código. No es un Google Doc aparte ni un PDF
que se envía por correo. Es un archivo Markdown que evoluciona con el proyecto.

> "La especificación no es un documento estático. Es un entregable vivo que se versiona
> junto con el código y refleja el estado actual del sistema."
>
> — [spec-driven.md, GitHub Spec-Kit](https://github.com/github/spec-kit/blob/main/spec-driven.md)

### Cómo se versiona en la práctica

| Acción en el proyecto | Qué se commitea | Ejemplo de commit |
|-----------------------|-----------------|-------------------|
| Crear una feature nueva | `sdd/` (spec, plan, tasks) **PRIMERO**, luego el código | `docs: spec login con JWT y control de acceso` |
| Cambiar una decisión técnica | Editar `01_constitucion.md` | `docs: cambiar de 5 GETs a ConsultasController` |
| Agregar una tabla a la BD | Editar `data-model.md` + `02_especificacion.md` | `docs: agregar tabla workflow al modelo ER` |
| Completar una tarea | Marcar `[x]` en `05_tareas.md` + commit del código | `feat: CRUD producto + marcar tarea completada` |
| Corregir un error en la spec | Editar el .md correspondiente | `fix: corregir cardinalidad cliente-factura en spec` |

### Orden correcto: documentación primero, código después

```
1. Escribir/actualizar la spec  →  git add sdd/  →  git commit -m "docs: ..."
2. Implementar el código         →  git add ...   →  git commit -m "feat: ..."
3. Verificar contra la spec      →  marcar [x]    →  git commit -m "docs: tarea completada"
```

### Ver historial de cambios de la documentación

```powershell
# Ver todos los cambios en la constitución
git log --oneline sdd/01_constitucion.md

# Ver qué cambió en la especificación
git diff HEAD~1 sdd/02_especificacion.md

# Ver quién modificó el plan y cuándo
git blame sdd/04_plan.md
```

### Regla: la spec SIEMPRE refleja el estado actual

Si el código cambió pero la spec no, hay **deuda de especificación**. Esto es tan grave como
la deuda técnica — significa que la documentación miente sobre cómo funciona el sistema.

| Situación | Estado | Acción |
|-----------|--------|--------|
| Spec dice X, código hace X | Correcto | Nada |
| Spec dice X, código hace Y | Deuda de spec | Actualizar la spec |
| Spec dice X, código no existe | Tarea pendiente | Implementar |
| No hay spec, código existe | Deuda de spec | Documentar retroactivamente |

### Con herramientas SDD (opcional)

| Herramienta | Cómo ayuda con el versionamiento |
|-------------|--------------------------------|
| **Spec-Kit** | `/speckit-git-commit` genera commits estandarizados vinculados a la spec |
| **OpenSpec** | `/opsx:archive` mueve changes completados a `archive/` con fecha automática |
| **Git normal** | `git add sdd/ && git commit` — funciona perfectamente sin herramientas |
