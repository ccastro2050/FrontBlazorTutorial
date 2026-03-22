# Paso 2 — Herramientas

Antes de escribir código, estas son las 4 herramientas que se necesitan instaladas en Windows.

---

## 1. Git

**Qué es:** Sistema de control de versiones. Registra cada cambio que se hace en el código, permite volver a versiones anteriores, y es la base para subir código a GitHub.

**Qué hace en este proyecto:**
- Guardar el historial de cambios del proyecto (`git commit`)
- Crear ramas para trabajar sin afectar el código principal (`git branch`)
- Subir el proyecto a GitHub (`git push`)
- Descargar proyectos existentes (`git clone`)

**Comandos que se van a usar:**
```bash
git init                    # Inicializar un repositorio nuevo
git add .                   # Preparar archivos para guardar
git commit -m "mensaje"     # Guardar cambios con descripción
git push                    # Subir cambios a GitHub
git clone <url>             # Descargar un repositorio
git status                  # Ver qué archivos cambiaron
```

**Descargar:** https://git-scm.com/download/win

Durante la instalación, dejar todas las opciones por defecto. La opción importante es que agregue Git al PATH del sistema (viene marcada por defecto).

**Verificar instalación** (abrir cualquier terminal):
```bash
git --version
```

---

## 2. Visual Studio Code (VS Code)

**Qué es:** Editor de código gratuito de Microsoft. Ligero, rápido, y con extensiones para cualquier lenguaje.

**Qué hace en este proyecto:**
- Editar archivos `.razor`, `.cs`, `.json`, `.css`, `.md`
- Terminal integrada para ejecutar comandos sin salir del editor
- IntelliSense: autocompletado y detección de errores en tiempo real
- Explorador de archivos del proyecto en el panel izquierdo

**Extensiones recomendadas** (instalar desde el panel de extensiones con `Ctrl+Shift+X`):

| Extensión | Para qué sirve |
|-----------|----------------|
| **C# Dev Kit** | Soporte completo de C#, IntelliSense, depuración |
| **Blazor WASM Debugging** | Depuración de componentes Blazor |
| **.NET Install Tool** | Detecta y gestiona versiones de .NET |

**Descargar:** https://code.visualstudio.com/download

Elegir la versión **"User Installer" para Windows x64**.

**Verificar instalación** (abrir terminal):
```bash
code --version
```

**Abrir un proyecto desde terminal:**
```bash
code .                      # Abre la carpeta actual en VS Code
code MiProyecto/            # Abre una carpeta específica
```

---

## 3. PowerShell

**Qué es:** Terminal de Windows. Es donde se ejecutan los comandos de .NET, Git, y se levanta el proyecto.

**Qué hace en este proyecto:**
- Ejecutar `dotnet new`, `dotnet run`, `dotnet build`
- Ejecutar comandos de Git
- Navegar entre carpetas del proyecto

**Versiones:**
- **Windows PowerShell 5.1** — ya viene instalado en Windows 10/11. Es suficiente.
- **PowerShell 7+** — versión moderna, opcional pero recomendada.

Para instalar PowerShell 7 (opcional):
```bash
winget install Microsoft.PowerShell
```

O descargar desde: https://github.com/PowerShell/PowerShell/releases

**Verificar versión:**
```powershell
$PSVersionTable.PSVersion
```

**Tip:** VS Code tiene terminal integrada (`Ctrl+ñ` o `` Ctrl+` ``). Esa terminal usa PowerShell por defecto en Windows, así que no es necesario abrir una ventana aparte.

**Comandos básicos de navegación:**
```powershell
cd C:\Users\fcl\Desktop\proyectos   # Ir a una carpeta
ls                                    # Listar archivos
mkdir MiCarpeta                       # Crear carpeta
```

---

## 4. GitHub

**Qué es:** Plataforma web para almacenar repositorios Git en la nube. Permite compartir código, colaborar, y tener un respaldo remoto del proyecto.

**Qué hace en este proyecto:**
- Almacenar el código del frontend y la API en la nube
- Tener un respaldo accesible desde cualquier máquina
- Compartir el proyecto con otros

**Crear cuenta:** https://github.com

**Crear un repositorio nuevo:**
1. Ir a https://github.com/new
2. Nombre del repositorio: `FrontBlazorTutorial`
3. Dejar en **Public** o **Private** según preferencia
4. **No** marcar "Add a README" (se crea desde el proyecto local)
5. Clic en **Create repository**

**Conectar el proyecto local con GitHub:**
```bash
git remote add origin https://github.com/TU_USUARIO/FrontBlazorTutorial.git
git branch -M main
git push -u origin main
```

**Autenticación:** La primera vez que se haga `push`, Git pide credenciales. GitHub ya no acepta contraseña directa, se necesita uno de estos métodos:

| Método | Cómo configurarlo |
|--------|--------------------|
| **HTTPS + Token** | Crear un Personal Access Token en GitHub → Settings → Developer settings → Personal access tokens. Usar el token como contraseña. |
| **Git Credential Manager** | Se instala automáticamente con Git para Windows. Abre una ventana del navegador para autenticarse la primera vez. |

---

## Herramienta adicional: .NET SDK

No está en la lista original, pero es obligatorio. Es el kit de desarrollo que incluye el comando `dotnet` para crear, compilar y ejecutar proyectos Blazor.

**Descargar:** https://dotnet.microsoft.com/download/dotnet/9.0

Instalar el **SDK** (no el Runtime). La versión del proyecto es **.NET 9**.

**Verificar instalación:**
```bash
dotnet --version
```

Debe mostrar algo como `9.x.xxx`.

**Comandos que se van a usar:**
```bash
dotnet new blazor -n MiProyecto    # Crear proyecto Blazor nuevo
dotnet run                          # Ejecutar el proyecto
dotnet build                        # Compilar sin ejecutar
dotnet watch                        # Ejecutar con recarga automática
```

---

## Checklist de verificación

Abrir una terminal (PowerShell o la terminal de VS Code) y ejecutar:

```bash
git --version          # ✓ git version 2.x.x
dotnet --version       # ✓ 9.x.xxx
code --version         # ✓ 1.x.x
```

Si los tres responden, todo está listo.

---

> **Siguiente paso:** Paso 3 — Crear el proyecto Blazor Server y configurar la conexión a la API.
