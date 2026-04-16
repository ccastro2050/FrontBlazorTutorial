# Herramientas SDD: Spec-Kit, OpenSpec y Comparación

> Este documento describe las dos herramientas principales de Spec Driven Development
> (Spec-Kit de GitHub y OpenSpec de Fission AI), cómo se instalarían en este proyecto
> Blazor/.NET, y una comparación detallada con el enfoque manual (`sdd/`).
>
> **Nota**: Ninguna herramienta se instaló en este proyecto. Este documento es
> informativo — describe qué son, qué generarían y cuándo usar cada una.

---

## 1. Qué es Spec-Kit

[Spec-Kit](https://github.com/github/spec-kit) es la herramienta oficial de GitHub para
Spec Driven Development. Automatiza el ciclo de vida SDD con slash commands que guían
al desarrollador por las 5 fases: Constitución, Especificación, Clarificación, Plan y Tareas.

| Función | Qué hace |
|---------|----------|
| **Scaffolding** | Crea la estructura `.specify/` con templates, scripts, constituciones |
| **Guardrails** | Verifica que el código generado cumpla con la constitución |
| **Ciclo de vida** | Permite moverse entre las 5 etapas del SDD con comandos |

Su concepto clave es la **constitución**: un documento con reglas no negociables que la IA
no puede violar (tecnologías, convenciones, prohibiciones). Es ideal para proyectos nuevos
(greenfield) donde se parte de cero.

## 2. Qué es OpenSpec

[OpenSpec](https://github.com/Fission-AI/OpenSpec) es el framework open source de Fission AI
para SDD. Con 30.000+ estrellas en GitHub, es compatible con 20+ agentes de IA
(Claude Code, Cursor, Copilot, Gemini, OpenCode, etc.).

Su concepto clave son los **delta specs**: en vez de reescribir toda la especificación,
se documentan solo los cambios (ADDED, MODIFIED, REMOVED). Es ideal para proyectos
existentes (brownfield) donde ya hay código.

### 4 principios de OpenSpec

| Principio | Qué significa |
|-----------|-------------|
| **Fluido, no rígido** | No hay puertas de fase. Puedes volver atrás cuando quieras |
| **Iterativo, no waterfall** | Aprende mientras construyes, refina sobre la marcha |
| **Fácil, no complejo** | Configuración mínima, arranque en segundos |
| **Brownfield first** | Diseñado para proyectos existentes, no solo para nuevos |

---

## 3. Cómo se instalaría Spec-Kit en este proyecto (.NET/C#)

### 3.1 Prerrequisitos

| Herramienta | Versión mínima | Cómo instalar |
|------------|---------------|---------------|
| Python | 3.11+ | [python.org](https://www.python.org/downloads/) |
| uv | 0.11+ | `pip install uv` |
| Git | cualquiera | Ya instalado |

### 3.2 Comando de instalación

```powershell
cd C:\Users\fcl\OneDrive\Desktop\proyectoscsharp\FrontBlazorTutorial
$env:PYTHONIOENCODING = "utf-8"
uvx --from git+https://github.com/github/spec-kit.git specify init --here --integration claude
```

Opciones:
- `--here`: inicializar en el directorio actual
- `--integration claude`: configurar para Claude Code como agente IA

### 3.3 Qué generaría

```
FrontBlazorTutorial/
├── .specify/                              <- Carpeta principal de Spec-Kit
│   ├── memory/
│   │   └── constitution.md               <- Plantilla de constitución (por llenar)
│   ├── templates/
│   │   ├── constitution-template.md      <- Template para constituciones
│   │   ├── spec-template.md              <- Template para especificaciones
│   │   ├── plan-template.md              <- Template para planes
│   │   ├── tasks-template.md             <- Template para tareas
│   │   └── agent-file-template.md        <- Template para CLAUDE.md
│   ├── scripts/powershell/
│   │   ├── check-prerequisites.ps1       <- Verificar herramientas
│   │   ├── create-new-feature.ps1        <- Crear rama feature/
│   │   └── setup-plan.ps1                <- Configurar plan
│   ├── extensions/git/                   <- Extensión Git
│   ├── integrations/claude/              <- Integración con Claude Code
│   ├── extensions.yml
│   └── integration.json
│
└── .claude/skills/                       <- Skills de Claude Code
    ├── speckit-constitution/SKILL.md
    ├── speckit-specify/SKILL.md
    ├── speckit-plan/SKILL.md
    ├── speckit-tasks/SKILL.md
    ├── speckit-implement/SKILL.md
    ├── speckit-clarify/SKILL.md
    ├── speckit-analyze/SKILL.md
    ├── speckit-checklist/SKILL.md
    ├── speckit-git-commit/SKILL.md
    └── speckit-git-feature/SKILL.md
```

### 3.4 Comandos que estarían disponibles

| Comando | Fase SDD | Qué hace |
|---------|----------|----------|
| `/speckit-constitution` | Constitución | Establece principios no negociables |
| `/speckit-specify` | Especificación | Define requisitos y historias de usuario |
| `/speckit-clarify` | Clarificación | Hace preguntas para resolver ambigüedades |
| `/speckit-plan` | Plan | Crea plan técnico con stack elegido |
| `/speckit-tasks` | Tareas | Genera desglose de tareas ejecutables |
| `/speckit-implement` | Código | Ejecuta las tareas del plan |
| `/speckit-analyze` | Validación | Verifica consistencia entre artefactos |
| `/speckit-checklist` | Calidad | Genera checklists de calidad |

---

## 4. Cómo se instalaría OpenSpec en este proyecto

### 4.1 Prerrequisitos

| Herramienta | Versión mínima | Cómo verificar |
|------------|---------------|----------------|
| Node.js | 20.19+ | `node --version` |
| npm | (incluido) | `npm --version` |
| Git | cualquiera | `git --version` |

### 4.2 Comando de instalación

```powershell
npm install -g @fission-ai/openspec@latest
openspec --version   # Debe mostrar: 1.3.0 (o superior)
```

### 4.3 Inicializar en el proyecto

```powershell
cd C:\Users\fcl\OneDrive\Desktop\proyectoscsharp\FrontBlazorTutorial
openspec init
```

El proceso pregunta qué agente de IA usas. Seleccionar `Claude Code`.

### 4.4 Qué generaría

```
FrontBlazorTutorial/
├── openspec/
│   ├── specs/              <- Fuente de verdad (vacía al inicio)
│   └── changes/
│       └── archive/        <- Cambios archivados
│
└── .claude/
    ├── commands/opsx/       <- Comandos OPSX
    └── skills/
        ├── openspec-propose/     <- /opsx:propose
        ├── openspec-apply-change/ <- /opsx:apply
        ├── openspec-archive-change/ <- /opsx:archive
        └── openspec-explore/     <- /opsx:explore
```

### 4.5 Comandos que estarían disponibles

| Comando | Qué hace | Cuándo usarlo |
|---------|----------|--------------|
| `/opsx:explore` | Modo exploratorio libre, sin crear artefactos | Cuando no tienes claro el enfoque |
| `/opsx:propose` | Crea un change con TODOS los artefactos | Inicio rápido, la mayoría de los casos |
| `/opsx:apply` | Implementa las tareas del change | Cuando el plan está listo |
| `/opsx:archive` | Archiva el change y fusiona delta specs | Cuando todas las tareas están completas |

### 4.6 Concepto clave: Delta Specs

En vez de reescribir toda la spec, escribes SOLO lo que cambia:

```markdown
# Delta for Producto

## ADDED Requirements

### Requirement: CSV Export
El sistema DEBE permitir exportar la lista de productos a CSV.

## MODIFIED Requirements

### Requirement: Listar productos
El sistema DEBE mostrar un botón "Exportar CSV" en la tabla de productos.
```

Al archivar, los deltas se fusionan: ADDED se agrega, MODIFIED reemplaza, REMOVED se elimina.

---

## 5. Comparación completa: sdd/ (Manual) vs Spec-Kit vs OpenSpec

### 5.1 Tabla comparativa general

| Aspecto | sdd/ (Manual) | Spec-Kit (GitHub) | OpenSpec (Fission AI) |
|---------|---------------|-------------------|----------------------|
| **Quién genera** | El humano escribe todo | La IA llena templates vía /speckit-* | La IA genera artefactos vía /opsx:* |
| **Instalación** | Ninguna (solo crear archivos .md) | Python + uv + uvx | Node.js + npm |
| **Complejidad setup** | Cero | Media (uv puede fallar en Windows) | Baja (npm install y listo) |
| **Formato** | Libre (tú decides la estructura) | Formato fijo (templates oficiales) | Formato fijo (proposal/spec/design/tasks) |
| **Constitución** | Sí (01_constitucion.md) | Sí (constitution.md, formato oficial) | No (usa config.yaml con context) |
| **Diagramas Mermaid** | Sí (secuencia, clases, ER) | No (falta según video tutorial) | No |
| **SOLID, ACID, patrones** | Sí (explicados con ejemplos) | No (solo si los escribes) | No |
| **Delta specs** | No | No | Sí (ADDED, MODIFIED, REMOVED) |
| **Archivado** | No | No | Sí (archive/ con fecha) |
| **Given/When/Then** | No | Sí (spec-template.md) | Sí (formato BDD) |
| **Paralelización [P]** | Sí (manual) | Sí (tasks-template.md) | Sí (automático) |
| **Git extension** | No | Sí (/speckit-git-*) | No |
| **Analyze/Checklist** | No | Sí (/speckit-analyze) | Sí (/opsx:verify) |
| **Multiidioma** | Sí (escribes en el idioma que quieras) | No (inglés) | Sí (ES, PT, ZH, JA, FR, DE) |
| **Brownfield** | Sí (funciona con cualquier proyecto) | Limitado | Diseñado para esto |
| **Educativo** | Muy alto (tutorial detallado) | Medio (formato estándar) | Medio (formato estándar) |
| **Reproducible** | No (cada quien escribe diferente) | Sí (templates + slash commands) | Sí (propose genera todo igual) |
| **Agentes IA** | Cualquiera (es solo Markdown) | Claude, Copilot, Gemini, +10 | Claude, Copilot, Cursor, +20 |
| **Estrellas GitHub** | N/A | ~5.000 | 30.000+ |
| **Versión** | N/A | 0.7.2 (pre-release) | 1.3.0 (estable) |
| **Lenguaje del proyecto** | Agnóstico | Agnóstico | Agnóstico |

### 5.2 Tabla comparativa: qué tiene cada uno

| Documento/Feature | sdd/ (Manual) | Spec-Kit | OpenSpec |
|-------------------|:---:|:---:|:---:|
| Constitución / reglas globales | ✅ 01_constitucion.md | ✅ constitution.md | -- (usa config.yaml) |
| Especificación / spec por feature | ✅ 02_especificacion.md | ✅ specs/{feature}/spec.md | ✅ specs/{dominio}/spec.md |
| Clarificación / preguntas | ✅ 03_clarificacion.md | ✅ /speckit-clarify | ✅ /opsx:explore |
| Plan técnico | ✅ 04_plan.md | ✅ specs/{feature}/plan.md | ✅ changes/{nombre}/design.md |
| Tareas ejecutables | ✅ 05_tareas.md | ✅ specs/{feature}/tasks.md | ✅ changes/{nombre}/tasks.md |
| Modelo de datos (SQL) | ✅ data-model.md | -- (se crea manual) | -- (se crea manual) |
| Diagramas secuencia (Mermaid) | ✅ 04_plan.md secc.7 | -- | -- |
| Diagrama clases (Mermaid) | ✅ 04_plan.md secc.8 | -- | -- |
| SOLID explicado con ejemplos | ✅ 01_constitucion.md | -- | -- |
| ACID explicado | ✅ 01_constitucion.md | -- | -- |
| Patrones de diseño | ✅ 01_constitucion.md | -- | -- |
| Delta specs (cambios incrementales) | -- | -- | ✅ |
| Archivado de cambios | -- | -- | ✅ |
| Proposal (por qué + alcance) | -- | -- | ✅ changes/{}/proposal.md |
| Given/When/Then (BDD) | -- | ✅ | ✅ |
| Git integration (commits/branches) | -- | ✅ /speckit-git-* | -- |
| Herramientas SDD documentadas | ✅ 06_herramientas_sdd.md | ✅ incluido | ✅ incluido |

### 5.3 Ventajas de cada enfoque

**sdd/ (Manual) — Lo mejor para enseñar**

| Ventaja | Por qué |
|---------|---------|
| Total libertad de formato | Puedes incluir SOLID, ACID, patrones, diagramas, lo que quieras |
| No requiere instalación | Solo Markdown, funciona en cualquier editor |
| Diagramas Mermaid | Ni Spec-Kit ni OpenSpec los generan |
| Contenido educativo | Explicaciones con ejemplos, comparaciones, narrativas |
| Cualquier idioma | Escribes en español directamente |
| Sin dependencia de herramienta | Si Spec-Kit o OpenSpec desaparecen, tu sdd/ sigue |
| Agnóstico de stack | Funciona igual para Blazor, Flask, Java, PHP |

**Spec-Kit — Lo mejor para estructura y validación**

| Ventaja | Por qué |
|---------|---------|
| Constitución como concepto formal | Reglas no negociables que la IA respeta |
| Templates estándar | Todos los specs tienen el mismo formato |
| /speckit-analyze | Valida que spec, plan y tasks estén alineados |
| /speckit-checklist | Genera checklist de calidad automático |
| Git extension | Commits y branches estandarizados |
| Fases claras | Fácil de enseñar: 1.Constitution 2.Specify 3.Plan 4.Tasks 5.Implement |

**OpenSpec — Lo mejor para proyectos existentes**

| Ventaja | Por qué |
|---------|---------|
| Delta specs | Solo documentas LO QUE CAMBIA, no reescribes todo |
| /opsx:propose | Genera TODO de una vez (proposal + spec + design + tasks) |
| /opsx:explore | Piensas la idea antes de comprometerte |
| Archivado | Trazabilidad completa con fechas |
| npm install | Fácil de instalar (los estudiantes ya tienen Node.js) |
| Multiidioma | Specs en español nativo |
| 30.000+ estrellas | Comunidad activa, actualizaciones frecuentes |

### 5.4 Desventajas de cada enfoque

| Enfoque | Desventaja | Impacto |
|---------|-----------|---------|
| **Manual (sdd/)** | No es reproducible (cada quien escribe diferente) | Difícil estandarizar en equipos grandes |
| **Manual (sdd/)** | No tiene validación automática | No sabes si spec y código están alineados |
| **Manual (sdd/)** | Requiere disciplina del humano | Si no escribes, no existe |
| **Spec-Kit** | No tiene delta specs | Reescribir spec completa para cada cambio |
| **Spec-Kit** | Pre-release (v0.7.2) | Posibles cambios breaking |
| **Spec-Kit** | uv puede fallar en Windows | Instalación complicada para estudiantes |
| **Spec-Kit** | Sin diagramas Mermaid | Falta visual |
| **OpenSpec** | No tiene constitución | Reglas globales no tienen lugar dedicado |
| **OpenSpec** | No tiene Git extension | No integra commits |
| **OpenSpec** | Requiere Node.js 20.19+ | Versión reciente |

---

## 6. Recomendación para este proyecto y similares

### Usar el enfoque que mejor se adapte a cada fase:

| Fase del proyecto | Herramienta | Por qué |
|-------------------|-------------|---------|
| **Inicio (greenfield)** | Spec-Kit | Constitution + estructura inicial + fases claras |
| **Agregar features (brownfield)** | OpenSpec | Delta specs + propose rápido + archivado |
| **Evaluación** | Spec-Kit | Analyze + Checklist para verificar completitud |
| **Trabajo en equipo** | Spec-Kit | Git extension para commits/branches estándar |
| **Exploración de ideas** | OpenSpec | `/opsx:explore` sin comprometerse |
| **Documentación educativa** | sdd/ (manual) | SOLID, ACID, diagramas Mermaid (ninguna herramienta los genera) |

### Para proyectos similares (tutoriales universitarios)

| Escenario | Recomendación |
|-----------|--------------|
| Proyecto nuevo, 1 estudiante | Spec-Kit solo (constitution + specs por feature) |
| Proyecto nuevo, 3+ estudiantes | Spec-Kit (constitution + git extension) + OpenSpec (delta specs) |
| Proyecto existente, agregar features | OpenSpec solo (delta specs + propose + archive) |
| Curso de Diseño de Software | sdd/ manual (para enseñar conceptos: SOLID, ACID, patrones, diagramas) |
| Producción real | OpenSpec (brownfield, rápido, multiidioma) |

---

## 7. Referencias

- Spec-Kit repo: [github.com/github/spec-kit](https://github.com/github/spec-kit)
- Spec-Kit documentación: [spec-driven.md](https://github.com/github/spec-kit/blob/main/spec-driven.md)
- OpenSpec repo: [github.com/Fission-AI/OpenSpec](https://github.com/Fission-AI/OpenSpec)
- OpenSpec guía: [webreactiva.com/blog/openspec](https://www.webreactiva.com/blog/openspec)
- Blog Microsoft: [Diving Into SDD With Spec Kit](https://developer.microsoft.com/blog/spec-driven-development-spec-kit)
- Video SDD conceptual: [youtu.be/p2WA672HrdI](https://youtu.be/p2WA672HrdI)
- Video Spec-Kit tutorial: [youtu.be/QzSCmSFKvko](https://youtu.be/QzSCmSFKvko)
