# Paso 3 — Crear el Proyecto y Configurar GitHub con 3 Cuentas

---

## Parte A: Crear el proyecto Blazor Server

### 1. Abrir terminal en la carpeta de proyectos

```powershell
cd C:\Users\TU_USUARIO\Desktop\proyectoscsharp
```

### 2. Crear el proyecto

```bash
dotnet new blazor -n FrontBlazorTutorial --interactivity Server
```

Esto genera la estructura completa del proyecto Blazor Server con .NET 9.

### 3. Verificar que funciona

```bash
cd FrontBlazorTutorial
dotnet run
```

Abrir en el navegador: `http://localhost:5000` (o el puerto que indique la terminal). Si aparece la página de bienvenida de Blazor, el proyecto está listo. Cerrar con `Ctrl+C`.

### 4. Abrir en VS Code

```bash
code .
```

---

## Parte B: Estructura de equipo en GitHub

Tres cuentas de GitHub, cada una con un rol:

```
┌───────────────────────────────────────────────────────────────────┐
│                        REPOSITORIO                                │
│                  FrontBlazorTutorial                               │
│                                                                   │
│  main ●────●────●────●────●────●────●──  (solo merges de PRs)    │
│            │    ▲    │    ▲    │    ▲                             │
│            │    │    │    │    │    │  Pull Requests               │
│            │    │    │    │    │    │                              │
│            ├── crud-producto ●──●  (Estudiante 1)                │
│            │         ├── crud-empresa ●──●  (Estudiante 1)       │
│            │         │                                            │
│            ├── crud-persona ●──●  (Estudiante 2)                 │
│            │         ├── crud-cliente ●──●  (Estudiante 2)       │
│            │         │                                            │
│            └── crud-usuario ●──●  (Estudiante 3)                 │
│                      └── crud-rol ●──●  (Estudiante 3)           │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
```

Nadie trabaja directamente en `main`. Cada tarea se hace en su propia rama. Cuando se termina, se crea un Pull Request, se revisa, y se hace merge a main. Después se crea una rama nueva para la siguiente tarea.

| Cuenta | Rol | Ramas | Permisos |
|--------|-----|-------|----------|
| **Estudiante 1** | Administrador del repositorio | Una rama por tarea (ej: `crud-producto`, `crud-empresa`) | Owner — crea el repo, invita, trabaja en sus ramas, revisa PRs de los demás, hace merge a main |
| **Estudiante 2** | Colaborador | Una rama por tarea (ej: `crud-persona`, `crud-cliente`) | Write — trabaja en sus ramas, crea Pull Requests hacia main |
| **Estudiante 3** | Colaborador | Una rama por tarea (ej: `crud-usuario`, `crud-rol`) | Write — trabaja en sus ramas, crea Pull Requests hacia main |

---

## Parte C: Lo que hace Estudiante 1 (Administrador)

### C1. Inicializar Git en el proyecto

Desde la carpeta `FrontBlazorTutorial`:

```bash
git init
git add .
git commit -m "Proyecto Blazor Server inicial"
```

> **Nota:** El template de Blazor ya incluye un archivo `.gitignore` que excluye automáticamente las carpetas `bin/` y `obj/` (archivos compilados que no deben subirse al repositorio). No es necesario crearlo manualmente.

### C2. Crear el repositorio en GitHub

1. Ir a https://github.com/new
2. Nombre: `FrontBlazorTutorial`
3. Visibilidad: **Private**
4. **No** marcar ninguna casilla (no README, no .gitignore)
5. Clic en **Create repository**

### C3. Subir el proyecto

```bash
git remote add origin https://github.com/TU_USUARIO/FrontBlazorTutorial.git
git branch -M main
git push -u origin main
```

### C3.1 Proteger la rama main

Proteger `main` significa que **nadie puede hacer push directo** a main — ni siquiera el dueño del repositorio. Todo cambio debe entrar por Pull Request aprobado. Esto evita que alguien suba código sin revisión.

**¿Qué pasa cuando se protege?**
- `git push origin main` falla — GitHub lo rechaza
- Solo se puede integrar código creando un PR y haciendo merge
- Se puede exigir que al menos 1 persona apruebe antes del merge

**¿Por qué se hace después del primer push?** Porque el primer push (`git push -u origin main`) necesita subir el proyecto inicial directamente. Después de eso, todo cambio va por PR.

**¿Aplica también para el dueño?** Sí. Con Branch Ruleset aplica para todos, incluyendo el dueño. Esto es lo recomendado para aprender — así todos están obligados a usar PRs. Si se necesita desactivar temporalmente, se puede hacer desde Settings.

**Pasos para proteger:**

1. Ir al repositorio en GitHub
2. Clic en **Settings** (pestaña superior)
3. En el menú izquierdo: **Rules** → **Rulesets**
4. Clic en **New ruleset** → **New branch ruleset**
5. **Ruleset Name:** `proteger-main`
6. **Enforcement status:** cambiar de `Disabled` a **Active**
7. En **Target branches:** clic en **Add target** → **Include by pattern** → escribir `main` → **Add**
8. En la sección **Rules**, marcar **Require a pull request before merging**
9. Dentro de esa opción, marcar **Require approvals** → dejar en **1**
10. Clic en **Create** (botón verde abajo)

A partir de este momento, nadie puede hacer `git push origin main` directamente. Todo debe ir por Pull Request.

### C4. Invitar a Estudiante 2 y Estudiante 3

1. Ir al repositorio en GitHub: `https://github.com/TU_USUARIO/FrontBlazorTutorial`
2. Clic en **Settings** (pestaña superior derecha)
3. En el menú izquierdo: **Collaborators** (dentro de "Access")
4. Clic en **Add people**
5. Escribir el nombre de usuario de GitHub de **Estudiante 2** → Clic en **Add**
6. Repetir para **Estudiante 3**

Los estudiantes 2 y 3 recibirán un correo con la invitación. Deben aceptarla.

### C5. Crear la primera rama de trabajo

Cada tarea se trabaja en una rama con nombre descriptivo. Por ejemplo, si la primera tarea es el CRUD de Producto:

```bash
git checkout -b crud-producto
git push -u origin crud-producto
```

Al terminar esta tarea, se crea un Pull Request hacia main. Después del merge, se vuelve a main y se crea una rama nueva para la siguiente tarea:

```bash
git checkout main
git pull
git checkout -b crud-empresa
git push -u origin crud-empresa
```

---

## Parte D: Lo que hacen Estudiante 2 y Estudiante 3

### D1. Aceptar la invitación

1. Iniciar sesión en GitHub con su cuenta
2. Ir a https://github.com/notifications
3. Aparece una notificación: **"Invitation to join [usuario]/FrontBlazorTutorial"**
4. Clic en esa notificación
5. Clic en el botón **Accept invitation**

Si no aparece en notificaciones, también se puede aceptar desde el correo electrónico asociado a la cuenta de GitHub (buscar un correo de GitHub con el asunto "invitation").

### D2. Clonar el repositorio

```bash
cd C:\Users\TU_USUARIO\Desktop\proyectoscsharp
git clone https://github.com/TU_USUARIO/FrontBlazorTutorial.git
cd FrontBlazorTutorial
```

### D3. Crear su rama de trabajo para la tarea asignada

Cada estudiante crea una rama con el nombre de su tarea. Por ejemplo:

**Estudiante 2** (si le toca CRUD Persona):
```bash
git checkout -b crud-persona
git push -u origin crud-persona
```

**Estudiante 3** (si le toca CRUD Usuario):
```bash
git checkout -b crud-usuario
git push -u origin crud-usuario
```

### D4. Verificar la rama actual

```bash
git branch
```

Debe mostrar (ejemplo para Estudiante 2):
```
  main
* crud-persona
```

El `*` indica la rama activa.

### D5. Trabajar, guardar y subir cambios

```bash
git add .
git commit -m "descripción del cambio"
git push
```

---

## Parte E: Flujo de trabajo (igual para los 3)

### 1. Trabajar en la rama y subir cambios:

```bash
git add .
git commit -m "Agregar página Producto con CRUD completo"
git push
```

### 2. Crear un Pull Request (PR) desde GitHub:

**¿Qué es un Pull Request?** Es una solicitud para integrar los cambios de una rama a main. En lugar de meter código directo a main, el PR permite:
- Ver exactamente qué archivos cambiaron (línea por línea)
- Que otro integrante del equipo revise el código antes de aprobarlo
- Tener un historial de qué se integró, cuándo y quién lo aprobó

Pasos:

1. Ir al repositorio en GitHub
2. GitHub muestra un banner amarillo: **"crud-producto had recent pushes"** → Clic en **Compare & pull request**
3. O ir a la pestaña **Pull requests** → **New pull request**
4. Base: `main` ← Compare: `crud-producto` (o el nombre de la rama)
5. Escribir título y descripción del cambio
6. Clic en **Create pull request**

### 3. Revisar y aprobar:

- Los PRs de **Estudiante 2 y 3** los revisa y aprueba **Estudiante 1**.
- Los PRs de **Estudiante 1** los puede revisar **Estudiante 2 o 3**.

Pasos para quien revisa:
1. Ir a la pestaña **Pull requests**
2. Abrir el PR
3. Revisar los cambios en la pestaña **Files changed**
4. Si todo está bien: **Review changes** → **Approve** → **Submit review**
5. Clic en **Merge pull request** → **Confirm merge**

### 4. Después del merge, preparar la siguiente tarea:

```bash
git checkout main
git pull
git checkout -b crud-siguiente-tarea
git push -u origin crud-siguiente-tarea
```

Se vuelve a main, se traen los cambios, y se crea una rama nueva para la siguiente tarea. La rama anterior ya no se usa.

---

## Parte F: Comandos de referencia rápida

### Para todos:
```bash
git status                  # Ver estado actual
git log --oneline -10       # Ver últimos 10 commits
git branch                  # Ver ramas locales
git branch -a               # Ver ramas locales y remotas
```

### Cambiar de rama:
```bash
git checkout main           # Ir a main
git checkout crud-producto  # Ir a una rama de tarea
```

### Actualizar desde remoto:
```bash
git pull                    # Traer cambios de GitHub
```

### Si hay conflictos al hacer merge:

Git marca los archivos con conflicto así:
```
<<<<<<< HEAD
código de tu rama
=======
código de main
>>>>>>> main
```

Para resolverlo:
1. Abrir el archivo en VS Code
2. VS Code muestra botones: **Accept Current**, **Accept Incoming**, **Accept Both**
3. Elegir la versión correcta
4. Guardar el archivo
5. Continuar:
```bash
git add .
git commit -m "Resolver conflicto en Producto.razor"
git push
```

---

## Resumen visual del flujo completo

```
ESTUDIANTE 1                    GITHUB                     ESTUDIANTE 2 / 3
─────────────                   ──────                     ─────────────────

1. Crea proyecto               2. Crea repo (Private)
   dotnet new blazor              github.com/new

3. git init + push ──────────→  Repo con main

4. Invita colaboradores ─────→  Invitación ──────────────→ 5. Acepta invitación

                                                           6. git clone

         ← ── ── ── LOS 3 TRABAJAN IGUAL ── ── ── →

7. git checkout -b crud-producto    7. git checkout -b crud-persona
   Trabaja, commit, push               Trabaja, commit, push

8. Crea PR crud-producto → main    8. Crea PR crud-persona → main

9. Est2/3 revisa PR de Est1        9. Est1 revisa PR de Est2/3
   Aprueba → Merge                    Aprueba → Merge

                    ← main actualizado →

10. git checkout main + git pull
11. git checkout -b siguiente-tarea   (nueva rama para nueva tarea)
```

---

> **Siguiente paso:** Paso 4 — Configurar la conexión a la API y crear el ApiService.
