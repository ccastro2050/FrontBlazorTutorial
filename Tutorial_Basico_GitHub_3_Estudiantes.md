# Tutorial Basico: Proyecto + GitHub con 3 Estudiantes

Objetivo: trabajar en equipo sin complicarse.

- Estudiante 1 crea el proyecto principal, crea su rama y sube su archivo.
- Estudiante 2 crea su rama y sube su archivo.
- Estudiante 3 crea su rama y sube su archivo.
- Estudiante 1 fusiona todas las ramas en `main` desde la terminal.

**Sin Pull Requests. Sin botones en GitHub. Todo desde la terminal.**

---

## 1) Estudiante 1: crear proyecto y repositorio

### 1.1 Crear proyecto en local

```powershell
cd C:\Users\TU_USUARIO\Desktop\proyectoscsharp
dotnet new blazor -n FrontBlazorTutorial --interactivity Server
cd FrontBlazorTutorial
```

### 1.2 Inicializar Git y primer commit (solo base del proyecto)

```bash
git init
git add .
git commit -m "Inicial: proyecto base"
```

### 1.3 Crear repo en GitHub

1. Ir a [https://github.com/new](https://github.com/new)
2. Nombre: `FrontBlazorTutorial`
3. Private o Public (como prefieran)
4. No marcar README ni .gitignore
5. Create repository

### 1.4 Subir `main` por primera vez

```bash
git remote add origin https://github.com/TU_USUARIO/FrontBlazorTutorial.git
git branch -M main
git push -u origin main
```

### 1.5 Invitar a Estudiante 2 y 3

En GitHub:

1. Repo -> **Settings**
2. **Collaborators** / **Access**
3. **Add people**
4. Invitar usuarios de Estudiante 2 y Estudiante 3

Ellos deben aceptar la invitacion.

### 1.6 Estudiante 1 crea su rama y sube su archivo

```bash
git checkout -b rama-estudiante1
```

```powershell
New-Item -Path "ESTUDIANTE1.txt" -ItemType File
Set-Content -Path "ESTUDIANTE1.txt" -Value "Archivo creado por Estudiante 1"
```

```bash
git add ESTUDIANTE1.txt
git commit -m "Agregar archivo de Estudiante 1"
git push origin rama-estudiante1
```

---

## 2) Estudiante 2: clonar, crear rama y subir su archivo

### 2.1 Clonar repo

```powershell
cd C:\Users\TU_USUARIO\Desktop\proyectoscsharp
git clone https://github.com/TU_USUARIO_EST1/FrontBlazorTutorial.git
cd FrontBlazorTutorial
```

### 2.2 Crear su rama

```bash
git checkout -b rama-estudiante2
```

### 2.3 Crear su archivo y subir cambios

```powershell
New-Item -Path "ESTUDIANTE2.txt" -ItemType File
Set-Content -Path "ESTUDIANTE2.txt" -Value "Archivo creado por Estudiante 2"
```

```bash
git add ESTUDIANTE2.txt
git commit -m "Agregar archivo de Estudiante 2"
git push origin rama-estudiante2
```

---

## 3) Estudiante 3: clonar, crear rama y subir su archivo

### 3.1 Clonar repo (si aun no lo tiene)

```powershell
cd C:\Users\TU_USUARIO\Desktop\proyectoscsharp
git clone https://github.com/TU_USUARIO_EST1/FrontBlazorTutorial.git
cd FrontBlazorTutorial
```

### 3.2 Crear su rama

```bash
git checkout -b rama-estudiante3
```

### 3.3 Crear su archivo y subir cambios

```powershell
New-Item -Path "ESTUDIANTE3.txt" -ItemType File
Set-Content -Path "ESTUDIANTE3.txt" -Value "Archivo creado por Estudiante 3"
```

```bash
git add ESTUDIANTE3.txt
git commit -m "Agregar archivo de Estudiante 3"
git push origin rama-estudiante3
```

---

## 4) Estudiante 1 fusiona todas las ramas en `main`

Solo Estudiante 1 hace esto. Los demas esperan.

```bash
git checkout main
git fetch origin
```

- `git checkout main` vuelve a la rama `main`.
- `git fetch origin` trae todas las ramas del remoto (rama-estudiante1, rama-estudiante2, rama-estudiante3).

```bash
git merge origin/rama-estudiante1
git merge origin/rama-estudiante2
git merge origin/rama-estudiante3
```

- Cada `git merge` fusiona una rama con `main`. Como cada estudiante modifico un archivo diferente, no hay conflictos.

> Si aparece una pantalla de vim pidiendo un mensaje de merge, escribir `:wq` y presionar Enter.

```bash
git push origin main
```

- Sube `main` actualizado a GitHub con los 3 archivos.

---

## 5) Verificacion final (cualquiera)

```bash
git checkout main
git pull origin main
git ls-files
```

Deben ver los tres archivos de estudiantes en el listado:

- Proyecto Blazor inicial
- `ESTUDIANTE1.txt`
- `ESTUDIANTE2.txt`
- `ESTUDIANTE3.txt`

---

## 6) Mini reglas

- Nadie trabaja directo en `main`.
- Cada uno usa su propia rama.
- Cada cambio va con commit claro.
- Estudiante 1 fusiona las ramas desde la terminal con `git fetch` + `git merge`.
- Si algo falla, revisar `git status` primero.
