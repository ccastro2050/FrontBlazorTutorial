/*
 * AuthService.cs - Servicio de autenticacion y autorizacion para Blazor Server.
 *
 * QUE SE NECESITA PARA LOGIN Y CONTROL DE ACCESO:
 * =================================================
 *
 * ARCHIVOS QUE SE CREAN (nuevos):
 *   Services/AuthService.cs              <- ESTE ARCHIVO: toda la logica de auth
 *   Components/Pages/Login.razor         <- Formulario de login (email + contrasena)
 *   Components/Pages/CambiarContrasena.razor <- Formulario para cambiar contrasena
 *   Components/Pages/RecuperarContrasena.razor <- Recuperar contrasena por email SMTP
 *   Components/Pages/SinAcceso.razor     <- Pagina error 403 (no tiene permiso)
 *   Components/Layout/EmptyLayout.razor  <- Layout vacio para login (sin sidebar)
 *
 * ARCHIVOS QUE SE MODIFICAN (existentes):
 *   Program.cs                           <- Agregar: builder.Services.AddScoped<AuthService>();
 *   Components/App.razor                 <- Agregar: @rendermode="InteractiveServer" en <Routes>
 *   Components/Layout/MainLayout.razor   <- Agregar: @inject AuthService, OnAfterRenderAsync
 *                                           (verifica sesion, redirige a /login, boton logout)
 *   appsettings.json                     <- Agregar: seccion "Smtp" para recuperar contrasena
 *
 * TABLAS QUE SE NECESITAN EN LA BD (5):
 *   usuario      <- email (PK) + contrasena (BCrypt)
 *   rol          <- id + nombre (Administrador, Vendedor, etc)
 *   rol_usuario  <- vincula usuario con roles (N:M)
 *   ruta         <- id + ruta (/producto, /cliente, etc)
 *   rutarol      <- vincula roles con rutas (N:M)
 *
 * LOS 3 CONCEPTOS CLAVE DE SEGURIDAD:
 * =====================================
 * 1. AUTENTICACION = ¿Quien eres?
 *    -> Login con email + contrasena
 *    -> La API verifica con BCrypt (hash irreversible)
 *    -> Si es correcto, devuelve un token JWT
 *    PARA QUE: confirmar que el usuario es quien dice ser
 *
 * 2. AUTORIZACION = ¿Que puedes hacer?
 *    -> Se cargan los ROLES del usuario (ej: "Vendedor", "Admin")
 *    -> Se cargan las RUTAS que esos roles pueden acceder (ej: "/producto", "/cliente")
 *    -> MainLayout verifica con TieneAcceso() antes de mostrar cada pagina
 *    PARA QUE: controlar que paginas puede ver cada usuario segun su rol
 *
 * 3. ENCRIPTACION = ¿Como se protege?
 *    -> Contrasenas: BCrypt (hash irreversible en la BD, nadie puede leerlas)
 *    -> Sesion: ProtectedSessionStorage (encriptado en el navegador, no manipulable)
 *    -> Transporte: HTTPS (los datos viajan encriptados por la red)
 *    PARA QUE: proteger la informacion en reposo (BD), en el navegador y en transito
 *
 * PROCESO COMPLETO PASO A PASO (VERSION ACTUAL — ConsultasController):
 * =====================================================================
 * 1. Usuario abre la app -> MainLayout llama Restaurar() -> no hay sesion -> redirige a /login
 * 2. Usuario escribe email + contrasena en Login.razor
 * 3. Login.razor llama AuthService.Login() que hace:
 *    a. PrecargarEstructura(): GET /api/estructuras/basedatos
 *       PARA QUE: descubrir como se llaman las columnas PK y FK de cada tabla
 *       (asi funciona con cualquier BD sin hardcodear nombres de columnas)
 *    b. PostJson("autenticacion/token"): POST con email + contrasena
 *       PARA QUE: la API verifica la contrasena con BCrypt y retorna OK o error
 *    c. CargarDatosRolesYRutas(): POST /api/consultas/ejecutarconsultaparametrizada
 *       UNA SOLA consulta SQL con JOINs que trae: nombre usuario, roles y rutas
 *       PARA QUE: saber quien es, que roles tiene y a que paginas puede acceder
 *       VENTAJA: 1 llamada HTTP en vez de 5 (ver "FORMA ANTERIOR" abajo)
 *    d. Guardar en ProtectedSessionStorage
 *       PARA QUE: recordar la sesion si el usuario refresca (F5)
 * 4. Redirige a "/" (inicio)
 * 5. En cada pagina, MainLayout verifica _auth.TieneAcceso(ruta)
 *    PARA QUE: si el usuario intenta acceder a una ruta no permitida -> /sin-acceso (403)
 *
 * FORMA ANTERIOR (sin ConsultasController — 5 llamadas HTTP separadas):
 * =====================================================================
 * Antes, el paso 3c se hacia con 5 GETs separados al CRUD generico:
 *   3c. CargarDatosUsuario(): GET /api/usuario?limite=999999
 *       -> Traia TODOS los usuarios y filtraba en memoria por email
 *   3d. CargarRoles():
 *       -> GET /api/rol_usuario?limite=999999 (TODOS los roles-usuario)
 *       -> GET /api/rol?limite=999999 (TODOS los roles)
 *       -> Filtraba en memoria: solo los que coincidian con el email
 *   3e. CargarRutasPermitidas():
 *       -> GET /api/rutarol?limite=999999 (TODAS las rutas-rol)
 *       -> GET /api/ruta?limite=999999 (TODAS las rutas)
 *       -> Filtraba en memoria: solo las rutas de los roles del usuario
 *
 * PROBLEMA: Cada GET traia la tabla COMPLETA (todos los registros) y luego
 * filtraba en C#. Si habia 1000 usuarios, traia los 1000 solo para buscar 1.
 * Ademas eran 5 llamadas HTTP (latencia de red x 5).
 *
 * SOLUCION ACTUAL: Una sola consulta SQL con JOINs que filtra en la BD:
 *   POST /api/consultas/ejecutarconsultaparametrizada
 *   SELECT r.nombre AS nombre_rol, ruta_t.ruta
 *   FROM usuario u
 *   JOIN rol_usuario rolu ON u.email = rolu.fkemail
 *   JOIN rol r ON rolu.fkidrol = r.id
 *   JOIN rutarol rr ON r.id = rr.fkidrol
 *   JOIN ruta ruta_t ON rr.fkidruta = ruta_t.id
 *   WHERE u.email = @email
 *
 * COMPARACION:
 *   | Aspecto          | Antes (5 GETs)           | Ahora (1 SQL)        |
 *   |------------------|--------------------------|----------------------|
 *   | Llamadas HTTP    | 5                        | 1                    |
 *   | Datos traidos    | Tablas COMPLETAS         | Solo filas del user  |
 *   | Filtro           | En memoria (C#)          | En BD (SQL WHERE)    |
 *   | Controlador      | EntidadesController      | ConsultasController  |
 *   | Endpoint         | GET /api/{tabla}          | POST /api/consultas  |
 *
 * TABLAS INVOLUCRADAS:
 * ====================
 * - usuario:     email (PK), contrasena (hash BCrypt)
 *                PARA QUE: almacenar credenciales de acceso
 * - rol:         id (PK), nombre (ej: "Administrador", "Vendedor")
 *                PARA QUE: definir tipos de usuario del sistema
 * - rol_usuario: fkemail -> usuario, fkidrol -> rol
 *                PARA QUE: asignar roles a usuarios (un usuario puede tener varios roles)
 * - ruta:        id (PK), ruta (ej: "/producto", "/cliente")
 *                PARA QUE: registrar las paginas del sistema
 * - rutarol:     fkidrol -> rol, fkidruta -> ruta
 *                PARA QUE: definir que paginas puede acceder cada rol
 *
 * DIAGRAMA:
 *   usuario --< rol_usuario >-- rol --< rutarol >-- ruta
 *   (quien)    (tiene roles)   (que rol)  (puede ver)  (que pagina)
 *
 * OPTIMIZACIONES:
 * ===============
 * - PrecargarEstructura(): UNA sola llamada cachea TODAS las tablas (no una por una)
 * - ConsultasController: UNA sola consulta SQL trae roles + rutas (no 5 GETs separados)
 * - La consulta SQL se arma DINAMICAMENTE con los nombres de FK/PK descubiertos
 * - _cache: los resultados de estructura se guardan para no repetir consultas
 */

using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FrontBlazorTutorial.Services;

public class AuthService
{
    // ── Propiedades publicas (se usan en las paginas Razor) ──
    public string? Usuario { get; private set; }           // Email del usuario logueado
    public string? NombreUsuario { get; private set; }     // Nombre para mostrar en la barra
    public List<string> Roles { get; private set; } = new();  // Lista de roles: ["Admin", "Vendedor"]
    public HashSet<string> RutasPermitidas { get; private set; } = new(); // Rutas que puede acceder
    public string? Token { get; private set; }               // Token JWT para enviar en cada request
    public bool EstaAutenticado => !string.IsNullOrEmpty(Usuario);  // true si hay sesion
    public bool DebeCambiarContrasena { get; set; }        // true si debe cambiar al entrar

    // ── Dependencias privadas ────────────────────────────────
    private readonly ProtectedSessionStorage _session;  // Almacenamiento encriptado del navegador
    private readonly string _apiUrl;                    // URL de la API (ej: http://localhost:5035)
    private readonly HttpClient _http;                  // Cliente HTTP para llamar a la API
    private readonly Dictionary<string, object> _cache = new(); // Cache de estructura BD

    /// <summary>
    /// Constructor. Blazor lo inyecta automaticamente (Dependency Injection).
    /// No necesita ApiService - usa HttpClient directo para ser independiente.
    /// Lee la URL de appsettings.json: busca "ApiUrl" o "ApiBaseUrl".
    /// </summary>
    public AuthService(ProtectedSessionStorage session, IConfiguration config)
    {
        _session = session;
        // Busca ApiUrl primero, si no existe busca ApiBaseUrl (compatible con ambos nombres)
        _apiUrl = config["ApiUrl"] ?? config["ApiBaseUrl"] ?? "http://127.0.0.1:7034";
        _http = new HttpClient { BaseAddress = new Uri(_apiUrl) };
    }

    // ══════════════════════════════════════════════════════════
    // HELPERS HTTP: Metodos internos para llamar a la API
    // ══════════════════════════════════════════════════════════
    //
    // ¿Por que no usar ApiService?
    // Porque ApiService puede tener firmas diferentes segun el proyecto
    // (Listar vs ListarAsync, parametros distintos, etc).
    // Estos helpers usan HttpClient directo para ser 100% independientes.
    // Asi el AuthService funciona en CUALQUIER proyecto Blazor sin importar
    // como este implementado su ApiService.
    //
    // Son solo 2 metodos:
    //   - Listar(): GET /api/{tabla} -> trae registros
    //   - PostJson(): POST /api/{endpoint} -> envia datos (para autenticar)
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Trae todos los registros de una tabla desde la API.
    /// Equivale a: GET /api/{tabla}?limite=999999
    ///
    /// La API retorna: {"datos": [{"email":"juan@...", "contrasena":"$2a$..."}, ...]}
    /// Este metodo extrae el array "datos" y lo convierte a una lista de diccionarios.
    ///
    /// Se usa para: buscar usuario por email, traer roles, traer rutas, etc.
    /// </summary>
    private async Task<List<Dictionary<string, object?>>> Listar(string tabla, int limite = 999999)
    {
        try
        {
            // Llamar a la API: GET /api/usuario?limite=999999 (por ejemplo)
            var json = await _http.GetStringAsync($"/api/{tabla}?limite={limite}");

            // Parsear el JSON de respuesta
            // La API retorna: {"datos": [{...}, {...}], "total": 5}
            using var doc = JsonDocument.Parse(json);

            var result = new List<Dictionary<string, object?>>();

            // Extraer el array "datos" del JSON
            if (doc.RootElement.TryGetProperty("datos", out var datos))
                // Recorrer cada registro del array
                foreach (var item in datos.EnumerateArray())
                {
                    // Convertir cada registro JSON a un diccionario C#
                    // Ejemplo: {"email": "juan@mail.com", "contrasena": "$2a$12$..."}
                    //       -> dict["email"] = "juan@mail.com", dict["contrasena"] = "$2a$12$..."
                    var dict = new Dictionary<string, object?>();
                    foreach (var prop in item.EnumerateObject())
                        // Si el valor es null en JSON, guardarlo como null en C#
                        dict[prop.Name] = prop.Value.ValueKind == JsonValueKind.Null
                            ? null : prop.Value.ToString();
                    result.Add(dict);
                }
            return result;
        }
        // Si la API no esta corriendo o hay error de red, retornar lista vacia
        catch { return new(); }
    }

    /// <summary>
    /// Envia un POST con JSON a la API.
    /// Se usa para autenticacion: POST /api/autenticacion/token
    ///
    /// Envia un diccionario como JSON y la API responde con:
    ///   - 200 OK: credenciales correctas -> retorna (true, "OK")
    ///   - 401: credenciales incorrectas -> retorna (false, "mensaje de error")
    ///
    /// No se usa para CRUD (crear registros) — solo para autenticar.
    /// </summary>
    private async Task<(bool ok, string msg)> PostJson(string endpoint, object datos)
    {
        try
        {
            // Convertir el objeto C# a JSON string
            // Ejemplo: {"tabla":"usuario","campoUsuario":"email",...}
            var content = new StringContent(
                JsonSerializer.Serialize(datos),
                System.Text.Encoding.UTF8, "application/json");

            // Enviar POST a la API: POST /api/autenticacion/token
            var resp = await _http.PostAsync($"/api/{endpoint}", content);

            // Si la API respondio 200 OK -> extraer el token JWT de la respuesta
            if (resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                // La API devuelve: {"token": "eyJhbG...", "usuario": "email", ...}
                if (doc.RootElement.TryGetProperty("token", out var tokenEl))
                    Token = tokenEl.GetString();
                return (true, "OK");
            }

            // Si respondio error (401, 500, etc) -> extraer el mensaje de error
            // La API retorna: {"estado": 401, "mensaje": "Contrasena incorrecta."}
            var errBody = await resp.Content.ReadAsStringAsync();
            using var errDoc = JsonDocument.Parse(errBody);
            var msg = errDoc.RootElement.TryGetProperty("mensaje", out var m)
                ? m.GetString() ?? "Error" : "Error";
            return (false, msg);
        }
        // Si la API no esta corriendo o hay error de red
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>
    /// Envia una consulta SQL parametrizada a ConsultasController.
    /// Se usa para cargar roles y rutas en UNA sola llamada en vez de 5 GETs.
    ///
    /// Endpoint: POST /api/consultas/ejecutarconsultaparametrizada
    /// Envia: {"consulta": "SELECT ...", "parametros": {"email": "valor"}}
    /// Recibe: {"resultados": [{...}, {...}], "total": N}
    ///
    /// DIFERENCIA CON Listar():
    /// - Listar() llama GET /api/{tabla} -> trae toda la tabla, filtra en C#
    /// - PostConsulta() llama POST /api/consultas -> ejecuta SQL con WHERE, filtra en BD
    ///
    /// VENTAJA: La BD hace el JOIN y el filtro, solo viajan los datos necesarios.
    /// </summary>
    private async Task<List<Dictionary<string, object?>>> PostConsulta(
        string consulta, Dictionary<string, object?> parametros)
    {
        try
        {
            // Armar el body del POST: {"consulta": "SELECT ...", "parametros": {...}}
            var body = new Dictionary<string, object?>
            {
                ["consulta"] = consulta,
                ["parametros"] = parametros
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                System.Text.Encoding.UTF8, "application/json");

            // POST /api/consultas/ejecutarconsultaparametrizada
            var resp = await _http.PostAsync("/api/consultas/ejecutarconsultaparametrizada", content);

            if (!resp.IsSuccessStatusCode) return new();

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var result = new List<Dictionary<string, object?>>();

            // La respuesta tiene "resultados" (no "datos" como en EntidadesController)
            // ConsultasController retorna: {"resultados": [...], "total": N}
            // EntidadesController retorna: {"datos": [...], "total": N}
            if (doc.RootElement.TryGetProperty("resultados", out var resultados) ||
                doc.RootElement.TryGetProperty("Resultados", out resultados))
            {
                foreach (var item in resultados.EnumerateArray())
                {
                    var dict = new Dictionary<string, object?>();
                    foreach (var prop in item.EnumerateObject())
                        dict[prop.Name] = prop.Value.ValueKind == JsonValueKind.Null
                            ? null : prop.Value.ToString();
                    result.Add(dict);
                }
            }
            return result;
        }
        catch { return new(); }
    }

    // ══════════════════════════════════════════════════════════
    // DESCUBRIMIENTO DINAMICO DE PKs Y FKs
    // ══════════════════════════════════════════════════════════
    //
    // En vez de hardcodear que el FK de rol_usuario hacia usuario se llama "fkemail",
    // consultamos la API: "que columna de rol_usuario apunta a usuario?"
    // La API responde con la estructura de la BD (PKs, FKs, tipos, etc).
    //
    // UNA SOLA LLAMADA: GET /api/estructuras/basedatos trae TODA la estructura.
    // Se cachea para no repetir. Esto es mucho mas rapido que una llamada por tabla.
    //
    // COMPATIBILIDAD: Postgres y SqlServer devuelven formatos ligeramente
    // diferentes (column_name vs nombre, is_primary_key vs es_primary_key).
    // El codigo normaliza ambos formatos.
    // ══════════════════════════════════════════════════════════

    private bool _estructuraCargada;

    /// <summary>
    /// Carga la estructura de TODA la BD en una sola llamada HTTP.
    /// Extrae PKs y FKs de cada tabla y los cachea en _cache.
    /// Solo se ejecuta una vez (la primera vez que se necesita).
    /// </summary>
    private async Task PrecargarEstructura()
    {
        if (_estructuraCargada) return;
        try
        {
            var json = await _http.GetStringAsync("/api/estructuras/basedatos");
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("tablas", out var tablas)) return;

            foreach (var t in tablas.EnumerateArray())
            {
                // Postgres usa "table_name", SqlServer usa "nombre"
                var nombre = (t.TryGetProperty("table_name", out var tn) ? tn.GetString()
                    : t.TryGetProperty("nombre", out var nm) ? nm.GetString() : "") ?? "";
                var columnas = new List<Dictionary<string, object?>>();

                if (t.TryGetProperty("columnas", out var colArr))
                    foreach (var col in colArr.EnumerateArray())
                    {
                        var dict = new Dictionary<string, object?>();
                        foreach (var prop in col.EnumerateObject())
                            dict[prop.Name] = prop.Value.ValueKind == JsonValueKind.Null
                                ? null : prop.Value.ToString();
                        // Normalizar: SqlServer usa "nombre"/"es_primary_key"
                        //             Postgres usa "column_name"/"is_primary_key"
                        if (!dict.ContainsKey("column_name") && dict.ContainsKey("nombre"))
                            dict["column_name"] = dict["nombre"];
                        if (!dict.ContainsKey("is_primary_key") && dict.ContainsKey("es_primary_key"))
                            dict["is_primary_key"] = dict["es_primary_key"]?.ToString() == "True" ? "YES" : "NO";
                        columnas.Add(dict);
                    }

                _cache[$"estructura_{nombre}"] = columnas;

                // Extraer PK (ej: usuario -> "email", rol -> "id")
                foreach (var col in columnas)
                    if (col.GetValueOrDefault("is_primary_key")?.ToString() == "YES")
                    { _cache[$"pk_{nombre}"] = col["column_name"]!.ToString()!; break; }

                // Extraer FKs desde el array foreign_keys del JSON
                // Ejemplo: rol_usuario tiene FK "fkemail" que apunta a tabla "usuario"
                //          -> se cachea como "rol_usuario->usuario" = "fkemail"
                if (t.TryGetProperty("foreign_keys", out var fkArr))
                    foreach (var fk in fkArr.EnumerateArray())
                    {
                        var colName = fk.GetProperty("column_name").GetString() ?? "";
                        var fkTable = fk.GetProperty("foreign_table_name").GetString() ?? "";
                        if (!string.IsNullOrEmpty(fkTable))
                            _cache[$"{nombre}->{fkTable}"] = colName;
                    }

                // Fallback para SqlServer: buscar FKs en las columnas y en fk_constraint_name
                foreach (var col in columnas)
                {
                    var ftn = col.GetValueOrDefault("foreign_table_name")?.ToString();
                    var constraint = col.GetValueOrDefault("fk_constraint_name")?.ToString() ?? "";
                    var colName = col.GetValueOrDefault("column_name")?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(ftn) && !string.IsNullOrEmpty(colName))
                    {
                        var key = $"{nombre}->{ftn}";
                        if (!_cache.ContainsKey(key))
                            _cache[key] = colName;
                    }
                    if (!string.IsNullOrEmpty(constraint) && !string.IsNullOrEmpty(colName))
                    {
                        foreach (var tbl in tablas.EnumerateArray())
                        {
                            var tblName = tbl.GetProperty("table_name").GetString() ?? "";
                            if (constraint.Contains(tblName, StringComparison.OrdinalIgnoreCase))
                            {
                                var key = $"{nombre}->{tblName}";
                                if (!_cache.ContainsKey(key))
                                    _cache[key] = colName;
                            }
                        }
                    }
                }
            }
            _estructuraCargada = true;
        }
        catch { }
    }

    /// <summary>
    /// Descubre que columna de tablaOrigen es FK hacia tablaDestino.
    /// Ejemplo: ObtenerFK("rol_usuario", "usuario") -> "fkemail"
    ///          ObtenerFK("rutarol", "rol") -> "fkidrol"
    /// </summary>
    private async Task<string?> ObtenerFK(string tablaOrigen, string tablaDestino)
    {
        await PrecargarEstructura();
        var key = $"{tablaOrigen}->{tablaDestino}";
        return _cache.TryGetValue(key, out var val) ? (string)val : null;
    }

    /// <summary>
    /// Descubre que columna es la PK de una tabla.
    /// Ejemplo: ObtenerPK("usuario") -> "email"
    ///          ObtenerPK("rol") -> "id"
    /// </summary>
    private async Task<string> ObtenerPK(string tabla)
    {
        await PrecargarEstructura();
        var key = $"pk_{tabla}";
        return _cache.TryGetValue(key, out var val) ? (string)val : "id";
    }

    // ══════════════════════════════════════════════════════════
    // LOGIN: Autenticar y cargar toda la sesion
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Proceso completo de login:
    ///   1. Precargar estructura BD + autenticar con BCrypt (EN PARALELO)
    ///   2. Cargar datos + roles + rutas con UNA SOLA consulta SQL
    ///   3. Guardar todo en ProtectedSessionStorage
    ///
    /// OPTIMIZACION:
    /// - Paso 1 usa Task.WhenAll (estructura + auth en paralelo)
    /// - Paso 2 usa ConsultasController (1 SQL en vez de 5 GETs)
    /// - Resultado: login mas rapido, menos trafico de red
    /// </summary>
    public async Task<(bool ok, string msg)> Login(string email, string contrasena)
    {
        try
        {
            // ── PASO 1: AUTENTICACION (¿Quien eres?) ──
            // Dos cosas en paralelo para ahorrar tiempo:
            //   a) Precargar estructura de la BD (descubrir PKs y FKs)
            //   b) Enviar credenciales a la API para verificar con BCrypt
            var pkTask = PrecargarEstructura();
            var authTask = PostJson("autenticacion/token", new Dictionary<string, object?>
            {
                ["tabla"] = "usuario",           // En que tabla buscar
                ["campoUsuario"] = "email",      // Que columna es el login
                ["campoContrasena"] = "contrasena", // Que columna es la contrasena
                ["usuario"] = email,             // El email ingresado por el usuario
                ["contrasena"] = contrasena       // La contrasena ingresada (texto plano)
                // La API compara esta contrasena contra el hash BCrypt de la BD
            });
            // Task.WhenAll ejecuta ambas tareas AL MISMO TIEMPO (no una despues de otra)
            await Task.WhenAll(pkTask, authTask);

            // Verificar si la autenticacion fue exitosa
            var (ok, msg) = authTask.Result;
            if (!ok) return (false, "Credenciales incorrectas");

            // Si llego aqui, el usuario ES quien dice ser (autenticacion exitosa)
            Usuario = email;

            // ── PASO 2: AUTORIZACION (¿Que puedes hacer?) ──
            // UNA SOLA llamada a ConsultasController:
            //   POST /api/consultas/ejecutarconsultaparametrizada
            //   Consulta SQL con JOINs que trae: nombre, roles y rutas del usuario
            //
            // ANTES eran 5 llamadas GET separadas:
            //   GET /api/usuario, GET /api/rol_usuario, GET /api/rol,
            //   GET /api/rutarol, GET /api/ruta
            // Ahora es 1 sola consulta que filtra en la BD (mas rapido y eficiente)
            await CargarDatosRolesYRutas(email);

            // Si no tiene roles, no puede entrar (no sabemos que puede hacer)
            if (Roles.Count == 0) return (false, "El usuario no tiene roles asignados.");

            // ── PASO 3: ENCRIPTACION (guardar sesion protegida) ──
            // Guardar en ProtectedSessionStorage del navegador.
            // Los datos se encriptan automaticamente (el usuario no puede manipularlos).
            // Persisten mientras el tab este abierto (F5 no los borra, cerrar tab si).
            await _session.SetAsync("usuario", Usuario);
            await _session.SetAsync("nombre_usuario", NombreUsuario ?? email);
            // El token JWT se guarda para enviarlo en cada request a la API
            // Si la API tiene [Authorize], sin este token las peticiones fallan con 401
            if (!string.IsNullOrEmpty(Token))
                await _session.SetAsync("token", Token);
            // Los roles y rutas se guardan como texto separado por comas
            // Ejemplo: "Administrador,Vendedor" y "/producto,/cliente,/factura"
            await _session.SetAsync("roles", string.Join(",", Roles));
            await _session.SetAsync("rutas_permitidas", string.Join(",", RutasPermitidas));

            return (true, "OK");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ══════════════════════════════════════════════════════════
    // CARGAR DATOS, ROLES Y RUTAS CON UNA SOLA CONSULTA SQL
    // ══════════════════════════════════════════════════════════
    //
    // ANTES: 5 llamadas GET separadas a EntidadesController (GET /api/{tabla})
    //   1. GET /api/usuario          -> traer TODOS los usuarios, buscar el nombre
    //   2. GET /api/rol_usuario      -> traer TODOS los rol_usuario, filtrar por email
    //   3. GET /api/rol              -> traer TODOS los roles, mapear ids a nombres
    //   4. GET /api/rutarol          -> traer TODOS los rutarol, filtrar por rol
    //   5. GET /api/ruta             -> traer TODAS las rutas, mapear ids a paths
    //   Problema: traia tablas COMPLETAS y filtraba en C# (ineficiente)
    //
    // AHORA: 1 sola llamada POST a ConsultasController
    //   POST /api/consultas/ejecutarconsultaparametrizada
    //   La BD hace los JOINs y el WHERE, solo viajan las filas del usuario
    //
    // La consulta SQL se arma DINAMICAMENTE usando los nombres de FK/PK
    // descubiertos por PrecargarEstructura(). Ejemplo con bdfacturas:
    //
    //   SELECT r.nombre AS nombre_rol, ruta_t.ruta
    //   FROM usuario u
    //   JOIN rol_usuario rolu ON u.email = rolu.fkemail        <- FK descubierto
    //   JOIN rol r ON rolu.fkidrol = r.id                      <- FK descubierto
    //   JOIN rutarol rr ON r.id = rr.fkidrol                   <- FK descubierto
    //   JOIN ruta ruta_t ON rr.fkidruta = ruta_t.id            <- FK descubierto
    //   WHERE u.email = @email
    //
    // Los nombres "fkemail", "fkidrol", "fkidruta" NO estan hardcodeados.
    // Se descubren de la estructura de la BD. Si otra BD usa "id_usuario"
    // en vez de "fkemail", funciona igual.
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Carga nombre del usuario, roles y rutas permitidas en UNA SOLA consulta SQL.
    ///
    /// Usa ConsultasController (POST /api/consultas/ejecutarconsultaparametrizada)
    /// en vez de 5 llamadas GET separadas al CRUD generico.
    ///
    /// La consulta SQL se arma dinamicamente con los FK/PK descubiertos
    /// por PrecargarEstructura(). Hace JOINs de 5 tablas y filtra por email.
    ///
    /// Resultado: cada fila tiene {nombre_rol, ruta}.
    /// Se extraen roles unicos y rutas unicas de las filas.
    /// </summary>
    private async Task CargarDatosRolesYRutas(string email)
    {
        Roles.Clear();
        RutasPermitidas.Clear();
        NombreUsuario = email; // Default: usar email si no tiene campo "nombre"

        try
        {
            // ── Paso 1: Descubrir nombres de FK/PK de la estructura ──
            // PrecargarEstructura() ya se ejecuto antes (en Login).
            // Estos metodos leen del _cache, no hacen llamadas HTTP.
            var pkUsuario = await ObtenerPK("usuario");          // ej: "email"
            var pkRol = await ObtenerPK("rol");                  // ej: "id"
            var pkRuta = await ObtenerPK("ruta");                // ej: "id"
            var fkEmail = await ObtenerFK("rol_usuario", "usuario"); // ej: "fkemail"
            var fkRolEnRolUsuario = await ObtenerFK("rol_usuario", "rol"); // ej: "fkidrol"
            var fkRolEnRutarol = await ObtenerFK("rutarol", "rol");   // ej: "fkidrol"
            var fkRutaEnRutarol = await ObtenerFK("rutarol", "ruta"); // ej: "fkidruta"

            // Si faltan FKs criticos, no se puede armar la consulta
            if (fkEmail == null || fkRolEnRolUsuario == null || fkRolEnRutarol == null)
            {
                // Fallback: intentar con los GETs individuales (metodo viejo)
                await CargarDatosUsuarioFallback(email);
                await CargarRolesFallback(email);
                await CargarRutasPermitidasFallback();
                return;
            }

            // ── Paso 2: Armar la consulta SQL dinamicamente ──
            // Los nombres de columnas vienen de la estructura, no estan hardcodeados.
            // Esto permite que funcione con cualquier BD que tenga las 5 tablas.
            //
            // Ejemplo con bdfacturas_sqlserver_local:
            //   pkUsuario = "email", pkRol = "id", pkRuta = "id"
            //   fkEmail = "fkemail", fkRolEnRolUsuario = "fkidrol"
            //   fkRolEnRutarol = "fkidrol", fkRutaEnRutarol = "fkidruta"
            //
            // Genera:
            //   SELECT r.nombre AS nombre_rol, ruta_t.ruta
            //   FROM usuario u
            //   JOIN rol_usuario rolu ON u.email = rolu.fkemail
            //   JOIN rol r ON rolu.fkidrol = r.id
            //   JOIN rutarol rr ON r.id = rr.fkidrol
            //   JOIN ruta ruta_t ON rr.fkidruta = ruta_t.id
            //   WHERE u.email = @email

            var sql = $@"SELECT r.nombre AS nombre_rol, ruta_t.ruta
FROM usuario u
JOIN rol_usuario rolu ON u.{pkUsuario} = rolu.{fkEmail}
JOIN rol r ON rolu.{fkRolEnRolUsuario} = r.{pkRol}
JOIN rutarol rr ON r.{pkRol} = rr.{fkRolEnRutarol}
JOIN ruta ruta_t ON rr.{fkRutaEnRutarol} = ruta_t.{pkRuta}
WHERE u.{pkUsuario} = @email";

            // ── Paso 3: Ejecutar la consulta via ConsultasController ──
            // POST /api/consultas/ejecutarconsultaparametrizada
            // El parametro @email previene inyeccion SQL (parametrizado)
            var resultados = await PostConsulta(sql, new Dictionary<string, object?>
            {
                ["email"] = email
            });

            // ── Paso 4: Extraer roles y rutas de los resultados ──
            // Cada fila tiene: {nombre_rol: "Contador", ruta: "/cliente"}
            // Un usuario con 2 roles y 5 rutas puede tener 10+ filas (producto cartesiano)
            // Usamos HashSet/Contains para evitar duplicados
            foreach (var fila in resultados)
            {
                // Extraer rol (puede repetirse en varias filas)
                var nombreRol = fila.GetValueOrDefault("nombre_rol")?.ToString() ?? "";
                if (!string.IsNullOrEmpty(nombreRol) && !Roles.Contains(nombreRol))
                    Roles.Add(nombreRol);

                // Extraer ruta (puede repetirse si varios roles tienen acceso)
                var ruta = fila.GetValueOrDefault("ruta")?.ToString() ?? "";
                if (!string.IsNullOrEmpty(ruta))
                    RutasPermitidas.Add(ruta); // HashSet ignora duplicados automaticamente
            }

            // ── Paso 5: Cargar nombre del usuario (campo opcional) ──
            // El nombre no viene en la consulta de roles/rutas porque la tabla usuario
            // puede no tener campo "nombre" (depende de la BD).
            // Se busca con un GET simple a /api/usuario (solo 1 registro nos interesa).
            await CargarDatosUsuarioFallback(email);
        }
        catch
        {
            NombreUsuario = email;
        }
    }

    /// <summary>
    /// Busca el nombre y debe_cambiar_contrasena del usuario.
    /// Usa GET /api/usuario (CRUD generico) porque el campo "nombre"
    /// es opcional y puede no existir en todas las BDs.
    /// Este es el unico GET que queda del enfoque anterior.
    /// </summary>
    private async Task CargarDatosUsuarioFallback(string email)
    {
        try
        {
            var pkUsuario = await ObtenerPK("usuario");
            var usuarios = await Listar("usuario");
            foreach (var u in usuarios)
            {
                var val = u.GetValueOrDefault(pkUsuario)?.ToString() ?? "";
                if (val.Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    NombreUsuario = u.GetValueOrDefault("nombre")?.ToString() ?? email;
                    var debeCambiar = u.GetValueOrDefault("debe_cambiar_contrasena")?.ToString();
                    DebeCambiarContrasena = debeCambiar == "True" || debeCambiar == "true" || debeCambiar == "1";
                    break;
                }
            }
        }
        catch { NombreUsuario = email; }
    }

    // ══════════════════════════════════════════════════════════
    // FALLBACK: Metodos del enfoque anterior (5 GETs separados)
    // ══════════════════════════════════════════════════════════
    //
    // Estos metodos se usan SOLO si ConsultasController no esta disponible
    // (por ejemplo, si faltan FKs en la estructura o si la API no tiene
    // el endpoint de consultas). Son los mismos metodos del enfoque anterior.
    //
    // FORMA VIEJA: 5 llamadas GET -> traer tablas completas -> filtrar en C#
    // FORMA NUEVA: 1 llamada POST -> consulta SQL con JOINs -> filtrar en BD
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// [FALLBACK] Carga roles con 2 GETs separados (enfoque anterior).
    /// Solo se usa si ConsultasController falla o no descubre los FKs.
    /// </summary>
    private async Task CargarRolesFallback(string email)
    {
        Roles.Clear();
        try
        {
            var fkEmail = await ObtenerFK("rol_usuario", "usuario");
            var fkRol = await ObtenerFK("rol_usuario", "rol");
            if (fkEmail == null || fkRol == null) return;
            var pkRol = await ObtenerPK("rol");

            var t1 = Listar("rol_usuario");
            var t2 = Listar("rol");
            await Task.WhenAll(t1, t2);
            var rolUsuarios = t1.Result;
            var roles = t2.Result;

            var rolMap = new Dictionary<string, string>();
            foreach (var r in roles)
                rolMap[r.GetValueOrDefault(pkRol)?.ToString() ?? ""] =
                    r.GetValueOrDefault("nombre")?.ToString() ?? "";

            foreach (var ru in rolUsuarios)
            {
                var ruEmail = ru.GetValueOrDefault(fkEmail)?.ToString() ?? "";
                if (ruEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    var rolId = ru.GetValueOrDefault(fkRol)?.ToString() ?? "";
                    if (rolMap.TryGetValue(rolId, out var nombreRol) && !Roles.Contains(nombreRol))
                        Roles.Add(nombreRol);
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// [FALLBACK] Carga rutas con 3 GETs separados (enfoque anterior).
    /// Solo se usa si ConsultasController falla o no descubre los FKs.
    /// </summary>
    private async Task CargarRutasPermitidasFallback()
    {
        RutasPermitidas.Clear();
        try
        {
            var fkRolEnRutarol = await ObtenerFK("rutarol", "rol");
            var fkRutaEnRutarol = await ObtenerFK("rutarol", "ruta");
            if (fkRolEnRutarol == null) return;
            var pkRol = await ObtenerPK("rol");
            var pkRuta = await ObtenerPK("ruta");

            var t1 = Listar("rutarol");
            var t2 = Listar("rol");
            var t3 = Listar("ruta");
            await Task.WhenAll(t1, t2, t3);
            var rutasRol = t1.Result;
            var rolesData = t2.Result;
            var rutasData = t3.Result;

            var rolIds = rolesData
                .Where(r => Roles.Contains(r.GetValueOrDefault("nombre")?.ToString() ?? ""))
                .Select(r => r.GetValueOrDefault(pkRol)?.ToString() ?? "")
                .ToHashSet();

            var rutaMap = new Dictionary<string, string>();
            foreach (var r in rutasData)
                rutaMap[r.GetValueOrDefault(pkRuta)?.ToString() ?? ""] =
                    r.GetValueOrDefault("ruta")?.ToString() ?? "";

            foreach (var rr in rutasRol)
            {
                var rolId = rr.GetValueOrDefault(fkRolEnRutarol)?.ToString() ?? "";
                if (!rolIds.Contains(rolId)) continue;
                var ruta = "";
                if (fkRutaEnRutarol != null)
                {
                    var rutaId = rr.GetValueOrDefault(fkRutaEnRutarol)?.ToString() ?? "";
                    rutaMap.TryGetValue(rutaId, out ruta);
                }
                if (string.IsNullOrEmpty(ruta))
                    ruta = rr.GetValueOrDefault("fkruta")?.ToString()
                        ?? rr.GetValueOrDefault("ruta")?.ToString() ?? "";
                if (!string.IsNullOrEmpty(ruta))
                    RutasPermitidas.Add(ruta);
            }
        }
        catch { }
    }

    // ══════════════════════════════════════════════════════════
    // SESION: Restaurar y cerrar
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Restaura la sesion desde ProtectedSessionStorage.
    /// Se llama en MainLayout.OnAfterRenderAsync al cargar cualquier pagina.
    /// Si el usuario ya habia hecho login (y no cerro el tab), restaura sus datos.
    /// Esto evita pedir login de nuevo al refrescar con F5.
    /// </summary>
    public async Task Restaurar()
    {
        try
        {
            var userResult = await _session.GetAsync<string>("usuario");
            if (userResult.Success && !string.IsNullOrEmpty(userResult.Value))
            {
                Usuario = userResult.Value;
                var nombreResult = await _session.GetAsync<string>("nombre_usuario");
                if (nombreResult.Success) NombreUsuario = nombreResult.Value;
                // Restaurar token JWT para que ApiService lo envie en cada request
                var tokenResult = await _session.GetAsync<string>("token");
                if (tokenResult.Success) Token = tokenResult.Value;
                var rolesResult = await _session.GetAsync<string>("roles");
                if (rolesResult.Success && !string.IsNullOrEmpty(rolesResult.Value))
                    Roles = rolesResult.Value.Split(',').ToList();
                var rutasResult = await _session.GetAsync<string>("rutas_permitidas");
                if (rutasResult.Success && !string.IsNullOrEmpty(rutasResult.Value))
                    RutasPermitidas = rutasResult.Value.Split(',').ToHashSet();
            }
        }
        catch { }
    }

    /// <summary>
    /// Cierra la sesion. Limpia todas las propiedades y borra del navegador.
    /// </summary>
    public async Task Logout()
    {
        Usuario = null;
        NombreUsuario = null;
        Token = null;
        Roles.Clear();
        RutasPermitidas.Clear();
        DebeCambiarContrasena = false;
        await _session.DeleteAsync("usuario");
        await _session.DeleteAsync("nombre_usuario");
        await _session.DeleteAsync("token");
        await _session.DeleteAsync("roles");
        await _session.DeleteAsync("rutas_permitidas");
    }

    // ══════════════════════════════════════════════════════════
    // CAMBIAR CONTRASENA
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Actualiza la contrasena del usuario en la BD.
    /// El parametro ?camposEncriptar=contrasena le dice a la API que
    /// encripte con BCrypt antes de guardar (hash irreversible).
    /// </summary>
    public async Task<(bool ok, string msg)> CambiarContrasena(string nueva)
    {
        if (string.IsNullOrEmpty(Usuario)) return (false, "No hay sesion activa.");
        try
        {
            var pkUsuario = await ObtenerPK("usuario");
            var content = new StringContent(
                JsonSerializer.Serialize(new Dictionary<string, string> { ["contrasena"] = nueva }),
                System.Text.Encoding.UTF8, "application/json");
            // PUT /api/usuario/{pk}/{valor}?camposEncriptar=contrasena
            var resp = await _http.PutAsync(
                $"/api/usuario/{pkUsuario}/{Usuario}?camposEncriptar=contrasena",
                content);
            if (resp.IsSuccessStatusCode)
            {
                DebeCambiarContrasena = false;
                return (true, "Contrasena actualizada.");
            }
            var body = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var msg = doc.RootElement.TryGetProperty("mensaje", out var m) ? m.GetString() ?? "Error" : "Error";
            return (false, msg);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    // ══════════════════════════════════════════════════════════
    // RECUPERAR CONTRASENA
    // ══════════════════════════════════════════════════════════

    // Set en memoria para rastrear usuarios que deben cambiar contrasena.
    // Cuando se recupera, el email se agrega aqui.
    // En el proximo login, se fuerza el cambio.
    private static readonly HashSet<string> _emailsDebeCambiar = new();

    /// <summary>
    /// Recupera la contrasena de un usuario:
    ///   1. Verifica que el email exista en la BD
    ///   2. Genera contrasena temporal aleatoria (8 chars)
    ///   3. La guarda encriptada con BCrypt en la BD
    ///   4. Marca el email para forzar cambio en el proximo login
    ///   5. Envia la temporal por correo SMTP (si esta configurado)
    /// </summary>
    public async Task<(bool ok, string msg)> RecuperarContrasena(string email, IConfiguration config)
    {
        try
        {
            // Verificar que el usuario existe
            var pkUsuario = await ObtenerPK("usuario");
            var usuarios = await Listar("usuario");
            var existe = usuarios.Any(u =>
                (u.GetValueOrDefault(pkUsuario)?.ToString() ?? "").Equals(email, StringComparison.OrdinalIgnoreCase));
            if (!existe) return (false, "No se encontro una cuenta con ese correo.");

            // Generar contrasena temporal: 1 mayuscula + 1 minuscula + 1 digito + 5 aleatorios
            var rng = new Random();
            var upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var lower = "abcdefghijklmnopqrstuvwxyz";
            var digits = "0123456789";
            var all = upper + lower + digits;
            var pwd = new string(new[] { upper[rng.Next(upper.Length)], lower[rng.Next(lower.Length)],
                digits[rng.Next(digits.Length)] }.Concat(Enumerable.Range(0, 5)
                .Select(_ => all[rng.Next(all.Length)])).ToArray());

            // Guardar encriptada con BCrypt
            var (okPwd, msgPwd) = await CambiarContrasenaInterno(email, pwd);
            if (!okPwd) return (false, msgPwd);

            // Marcar para forzar cambio
            _emailsDebeCambiar.Add(email.ToLower());

            // Enviar por correo SMTP
            var smtpUser = config["Smtp:User"] ?? "";
            var smtpPass = config["Smtp:Pass"] ?? "";
            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                return (true, $"Contrasena restablecida pero SMTP no configurado. Temporal: {pwd}");

            try
            {
                var smtpHost = config["Smtp:Host"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(config["Smtp:Port"] ?? "587");
                var smtpFrom = config["Smtp:From"] ?? smtpUser;

                using var smtp = new System.Net.Mail.SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };
                var body = $@"
                <html><body style='font-family:Arial;color:#333;max-width:600px;margin:0 auto'>
                    <div style='background:#0d6efd;color:white;padding:20px;text-align:center;border-radius:8px 8px 0 0'>
                        <h2 style='margin:0'>Recuperacion de Contrasena</h2>
                    </div>
                    <div style='padding:30px;background:#f8f9fa;border:1px solid #dee2e6;border-top:none;border-radius:0 0 8px 8px'>
                        <p>Su nueva contrasena temporal es:</p>
                        <div style='background:white;border:2px solid #0d6efd;border-radius:8px;padding:15px;text-align:center;margin:20px 0'>
                            <span style='font-size:24px;font-weight:bold;letter-spacing:3px;color:#0d6efd'>{pwd}</span>
                        </div>
                        <p><strong>Al ingresar, el sistema le pedira crear una nueva contrasena.</strong></p>
                    </div>
                </body></html>";

                var mail = new System.Net.Mail.MailMessage(smtpFrom, email, "Recuperacion de contrasena", body)
                { IsBodyHtml = true };
                await smtp.SendMailAsync(mail);
                return (true, "Se envio una contrasena temporal a su correo.");
            }
            catch (Exception ex)
            {
                return (true, $"Contrasena restablecida pero no se pudo enviar el correo: {ex.Message}");
            }
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    /// <summary>Verifica si el email esta marcado para forzar cambio (por recuperacion).</summary>
    public bool DebeForcarCambio(string email)
    {
        return _emailsDebeCambiar.Remove(email.ToLower());
    }

    private async Task<(bool ok, string msg)> CambiarContrasenaInterno(string email, string nueva)
    {
        var pkUsuario = await ObtenerPK("usuario");
        var content = new StringContent(
            JsonSerializer.Serialize(new Dictionary<string, string> { ["contrasena"] = nueva }),
            System.Text.Encoding.UTF8, "application/json");
        var resp = await _http.PutAsync(
            $"/api/usuario/{pkUsuario}/{email}?camposEncriptar=contrasena", content);
        if (resp.IsSuccessStatusCode) return (true, "OK");
        return (false, "Error al actualizar contrasena.");
    }

    // ══════════════════════════════════════════════════════════
    // VERIFICAR ACCESO A RUTA
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Verifica si el usuario puede acceder a una ruta.
    /// Se usa en MainLayout para redirigir a /sin-acceso si no tiene permiso.
    /// - La ruta "/" (inicio) siempre es accesible.
    /// - Si no hay rutas configuradas (sistema nuevo) permite todo.
    /// - Verifica la ruta exacta o si es sub-ruta (ej: /producto permite /producto/editar).
    /// </summary>
    public bool TieneAcceso(string ruta)
    {
        if (ruta == "/") return true;
        if (RutasPermitidas.Count == 0) return true;
        return RutasPermitidas.Any(r => ruta == r || ruta.StartsWith(r + "/"));
    }
}
