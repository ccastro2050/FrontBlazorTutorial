/*
 * AuthService.cs - Servicio de autenticacion para Blazor Server.
 *
 * COMO FUNCIONA LA AUTENTICACION:
 * ================================
 * 1. El usuario escribe email + contrasena en Login.razor
 * 2. Este servicio envia los datos a la API generica C# (POST /api/autenticacion/token)
 * 3. La API verifica la contrasena con BCrypt (hash irreversible en la BD)
 * 4. Si es correcta, devuelve un token JWT
 * 5. Se consultan los ROLES del usuario y las RUTAS que puede acceder
 * 6. Todo se guarda en ProtectedSessionStorage (cookie encriptada del navegador)
 *
 * TABLAS INVOLUCRADAS:
 * ====================
 * - usuario:     email (PK), contrasena (hash BCrypt)
 * - rol:         id (PK), nombre (ej: "Administrador", "Vendedor")
 * - rol_usuario: vincula usuario con roles (fkemail -> usuario, fkidrol -> rol)
 * - ruta:        id (PK), ruta (ej: "/producto", "/cliente")
 * - rutarol:     vincula roles con rutas (fkidrol -> rol, fkidruta -> ruta)
 *
 * DIAGRAMA:
 *   usuario --< rol_usuario >-- rol --< rutarol >-- ruta
 *
 * DESCUBRIMIENTO DINAMICO:
 * ========================
 * Este servicio NO hardcodea nombres de columnas (fkemail, fkidrol, etc).
 * Los descubre consultando GET /api/estructuras/basedatos que retorna
 * la estructura completa de la BD (columnas, PKs, FKs).
 * Asi funciona con cualquier BD (Postgres, SqlServer, MySQL).
 *
 * OPTIMIZACION:
 * =============
 * - Una sola llamada a /api/estructuras/basedatos cachea TODAS las tablas
 * - Las llamadas de datos se hacen en paralelo (Task.WhenAll)
 * - La sesion se guarda en ProtectedSessionStorage (persiste al refrescar F5)
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

    /// <summary>
    /// Trae todos los registros de una tabla.
    /// Equivale a: GET /api/{tabla}?limite=999999
    /// Retorna lista de diccionarios: [{"email":"juan@...", "nombre":"Juan"}, ...]
    /// </summary>
    private async Task<List<Dictionary<string, object?>>> Listar(string tabla, int limite = 999999)
    {
        try
        {
            var json = await _http.GetStringAsync($"/api/{tabla}?limite={limite}");
            using var doc = JsonDocument.Parse(json);
            var result = new List<Dictionary<string, object?>>();
            if (doc.RootElement.TryGetProperty("datos", out var datos))
                foreach (var item in datos.EnumerateArray())
                {
                    var dict = new Dictionary<string, object?>();
                    foreach (var prop in item.EnumerateObject())
                        dict[prop.Name] = prop.Value.ValueKind == JsonValueKind.Null
                            ? null : prop.Value.ToString();
                    result.Add(dict);
                }
            return result;
        }
        catch { return new(); }
    }

    /// <summary>
    /// Envia un POST con JSON a la API.
    /// Se usa para autenticacion: POST /api/autenticacion/token
    /// </summary>
    private async Task<(bool ok, string msg)> PostJson(string endpoint, object datos)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(datos),
                System.Text.Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"/api/{endpoint}", content);
            if (resp.IsSuccessStatusCode) return (true, "OK");
            var body = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var msg = doc.RootElement.TryGetProperty("mensaje", out var m)
                ? m.GetString() ?? "Error" : "Error";
            return (false, msg);
        }
        catch (Exception ex) { return (false, ex.Message); }
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
    ///   2. Cargar datos del usuario + roles (EN PARALELO)
    ///   3. Cargar rutas permitidas
    ///   4. Guardar todo en ProtectedSessionStorage
    ///
    /// Las llamadas en paralelo (Task.WhenAll) hacen el login mas rapido.
    /// </summary>
    public async Task<(bool ok, string msg)> Login(string email, string contrasena)
    {
        try
        {
            // Paso 1: Precargar estructura + autenticar EN PARALELO
            var pkTask = PrecargarEstructura();
            var authTask = PostJson("autenticacion/token", new Dictionary<string, object?>
            {
                ["tabla"] = "usuario",
                ["campoUsuario"] = "email",
                ["campoContrasena"] = "contrasena",
                ["usuario"] = email,
                ["contrasena"] = contrasena
            });
            await Task.WhenAll(pkTask, authTask);

            var (ok, msg) = authTask.Result;
            if (!ok) return (false, "Credenciales incorrectas");

            Usuario = email;

            // Paso 2: Cargar datos del usuario + roles EN PARALELO
            var datosTask = CargarDatosUsuario(email);
            var rolesTask = CargarRoles(email);
            await Task.WhenAll(datosTask, rolesTask);
            if (Roles.Count == 0) return (false, "El usuario no tiene roles asignados.");

            // Paso 3: Cargar rutas permitidas (necesita Roles ya cargados)
            await CargarRutasPermitidas();

            // Paso 4: Guardar en ProtectedSessionStorage
            // ProtectedSessionStorage encripta los datos en el navegador.
            // Persiste mientras el tab este abierto (se pierde al cerrar el tab).
            await _session.SetAsync("usuario", Usuario);
            await _session.SetAsync("nombre_usuario", NombreUsuario ?? email);
            await _session.SetAsync("roles", string.Join(",", Roles));
            await _session.SetAsync("rutas_permitidas", string.Join(",", RutasPermitidas));

            return (true, "OK");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Carga nombre y flag debe_cambiar_contrasena del usuario.
    /// </summary>
    private async Task CargarDatosUsuario(string email)
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

    /// <summary>
    /// Carga los roles del usuario consultando rol_usuario + rol.
    /// Usa FKs dinamicos para saber que columnas relacionan las tablas.
    /// Las dos consultas (rol_usuario y rol) se hacen EN PARALELO.
    /// </summary>
    private async Task CargarRoles(string email)
    {
        Roles.Clear();
        try
        {
            // Descubrir FKs: que columna de rol_usuario apunta a usuario? y a rol?
            var fkEmail = await ObtenerFK("rol_usuario", "usuario");
            var fkRol = await ObtenerFK("rol_usuario", "rol");
            if (fkEmail == null || fkRol == null) return;
            var pkRol = await ObtenerPK("rol");

            // Traer datos EN PARALELO
            var t1 = Listar("rol_usuario");
            var t2 = Listar("rol");
            await Task.WhenAll(t1, t2);
            var rolUsuarios = t1.Result;
            var roles = t2.Result;

            // Crear mapa: id_rol -> nombre_rol (ej: "1" -> "Administrador")
            var rolMap = new Dictionary<string, string>();
            foreach (var r in roles)
                rolMap[r.GetValueOrDefault(pkRol)?.ToString() ?? ""] =
                    r.GetValueOrDefault("nombre")?.ToString() ?? "";

            // Filtrar: solo los roles que pertenecen a este usuario
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
    /// Carga las rutas (paginas) que el usuario puede acceder segun sus roles.
    /// Consulta rutarol + rol + ruta EN PARALELO.
    /// El resultado se usa en MainLayout para verificar acceso.
    /// </summary>
    private async Task CargarRutasPermitidas()
    {
        RutasPermitidas.Clear();
        try
        {
            var fkRolEnRutarol = await ObtenerFK("rutarol", "rol");
            var fkRutaEnRutarol = await ObtenerFK("rutarol", "ruta");
            if (fkRolEnRutarol == null) return;
            var pkRol = await ObtenerPK("rol");
            var pkRuta = await ObtenerPK("ruta");

            // Traer 3 tablas EN PARALELO
            var t1 = Listar("rutarol");
            var t2 = Listar("rol");
            var t3 = Listar("ruta");
            await Task.WhenAll(t1, t2, t3);
            var rutasRol = t1.Result;
            var rolesData = t2.Result;
            var rutasData = t3.Result;

            // IDs de los roles del usuario
            var rolIds = rolesData
                .Where(r => Roles.Contains(r.GetValueOrDefault("nombre")?.ToString() ?? ""))
                .Select(r => r.GetValueOrDefault(pkRol)?.ToString() ?? "")
                .ToHashSet();

            // Mapa: id_ruta -> path (ej: "1" -> "/producto")
            var rutaMap = new Dictionary<string, string>();
            foreach (var r in rutasData)
                rutaMap[r.GetValueOrDefault(pkRuta)?.ToString() ?? ""] =
                    r.GetValueOrDefault("ruta")?.ToString() ?? "";

            // Filtrar: rutas asignadas a los roles del usuario
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
        Roles.Clear();
        RutasPermitidas.Clear();
        DebeCambiarContrasena = false;
        await _session.DeleteAsync("usuario");
        await _session.DeleteAsync("nombre_usuario");
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
