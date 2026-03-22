using System.Net.Http.Json;
using System.Text.Json;

namespace FrontBlazorTutorial.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        // ──────────────────────────────────────────────
        // LISTAR: GET /api/{tabla}
        // ──────────────────────────────────────────────
        public async Task<List<Dictionary<string, object?>>> ListarAsync(
            string tabla, int? limite = null)
        {
            try
            {
                string url = $"/api/{tabla}";
                if (limite.HasValue)
                    url += $"?limite={limite.Value}";

                var respuesta = await _http.GetFromJsonAsync<JsonElement>(url, _jsonOptions);

                if (respuesta.TryGetProperty("datos", out JsonElement datos))
                {
                    return ConvertirDatos(datos);
                }

                return new List<Dictionary<string, object?>>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al listar {tabla}: {ex.Message}");
                return new List<Dictionary<string, object?>>();
            }
        }

        // ──────────────────────────────────────────────
        // CREAR: POST /api/{tabla}
        // ──────────────────────────────────────────────
        public async Task<(bool exito, string mensaje)> CrearAsync(
            string tabla, Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            try
            {
                string url = $"/api/{tabla}";
                if (!string.IsNullOrEmpty(camposEncriptar))
                    url += $"?camposEncriptar={camposEncriptar}";

                var respuesta = await _http.PostAsJsonAsync(url, datos);
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(
                    _jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        // ACTUALIZAR: PUT /api/{tabla}/{clave}/{valor}
        // ──────────────────────────────────────────────
        public async Task<(bool exito, string mensaje)> ActualizarAsync(
            string tabla, string nombreClave, string valorClave,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            try
            {
                string url = $"/api/{tabla}/{nombreClave}/{valorClave}";
                if (!string.IsNullOrEmpty(camposEncriptar))
                    url += $"?camposEncriptar={camposEncriptar}";

                var respuesta = await _http.PutAsJsonAsync(url, datos);
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(
                    _jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        // ELIMINAR: DELETE /api/{tabla}/{clave}/{valor}
        // ──────────────────────────────────────────────
        public async Task<(bool exito, string mensaje)> EliminarAsync(
            string tabla, string nombreClave, string valorClave)
        {
            try
            {
                var respuesta = await _http.DeleteAsync(
                    $"/api/{tabla}/{nombreClave}/{valorClave}");
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(
                    _jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        // DIAGNOSTICO: GET /api/diagnostico/conexion
        // ──────────────────────────────────────────────
        public async Task<Dictionary<string, string>?> ObtenerDiagnosticoAsync()
        {
            try
            {
                var respuesta = await _http.GetFromJsonAsync<JsonElement>(
                    "/api/diagnostico/conexion", _jsonOptions);

                if (respuesta.TryGetProperty("servidor", out JsonElement servidor))
                {
                    var info = new Dictionary<string, string>();
                    foreach (var prop in servidor.EnumerateObject())
                    {
                        info[prop.Name] = prop.Value.ToString();
                    }
                    return info;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // ──────────────────────────────────────────────
        // Convierte JsonElement a lista de diccionarios
        // ──────────────────────────────────────────────
        private List<Dictionary<string, object?>> ConvertirDatos(JsonElement datos)
        {
            var lista = new List<Dictionary<string, object?>>();

            foreach (var fila in datos.EnumerateArray())
            {
                var diccionario = new Dictionary<string, object?>();

                foreach (var propiedad in fila.EnumerateObject())
                {
                    diccionario[propiedad.Name] = propiedad.Value.ValueKind switch
                    {
                        JsonValueKind.String => propiedad.Value.GetString(),
                        JsonValueKind.Number => propiedad.Value.TryGetInt32(out int i)
                            ? i : propiedad.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => propiedad.Value.GetRawText()
                    };
                }

                lista.Add(diccionario);
            }

            return lista;
        }
    }
}
