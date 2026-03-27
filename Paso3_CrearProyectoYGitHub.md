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
│  main ●────●────●────●────●────●────●──  (solo merges desde terminal) │
│            │    ▲    │    ▲    │    ▲                             │
│            │    │    │    │    │    │                              │
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

Nadie trabaja directamente en `main`. Cada tarea se hace en su propia rama. Cuando se termina, Estudiante 1 fusiona la rama desde la terminal con `git fetch` + `git merge`. Después se crea una rama nueva para la siguiente tarea.

| Cuenta | Rol | Ramas | Permisos |
|--------|-----|-------|----------|
| **Estudiante 1** | Administrador del repositorio | Una rama por tarea (ej: `crud-producto`, `crud-empresa`) | Owner — crea el repo, invita, trabaja en sus ramas, fusiona las ramas de los demás desde la terminal |
| **Estudiante 2** | Colaborador | Una rama por tarea (ej: `crud-persona`, `crud-cliente`) | Write — trabaja en sus ramas, sube con git push |
| **Estudiante 3** | Colaborador | Una rama por tarea (ej: `crud-usuario`, `crud-rol`) | Write — trabaja en sus ramas, sube con git push |

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

Al terminar esta tarea, Estudiante 1 fusiona desde la terminal. Después del merge, se vuelve a main y se crea una rama nueva para la siguiente tarea:

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

### 2. Estudiante 1 fusiona desde la terminal:

```bash
git checkout main
git fetch origin
git merge origin/nombre-de-la-rama
git push origin main
```

- `git fetch origin` trae todas las ramas del remoto.
- `git merge origin/nombre-de-la-rama` fusiona esa rama en `main`.
- `git push origin main` sube `main` actualizado a GitHub.

> Si aparece una pantalla de vim pidiendo un mensaje de merge, escribir `:wq` y presionar Enter.

### 3. Después del merge, preparar la siguiente tarea:

```bash
git checkout main
git pull
git checkout -b crud-siguiente-tarea
```

Se vuelve a main, se traen los cambios, y se crea una rama nueva para la siguiente tarea.

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

8. git push                            8. git push

9. Est1: git fetch + git merge desde terminal
   git push origin main

                    ← main actualizado →

10. git checkout main + git pull
11. git checkout -b siguiente-tarea   (nueva rama para nueva tarea)
```

---

> **Siguiente paso:** Paso 4 — Configurar la conexión a la API y crear el ApiService.
