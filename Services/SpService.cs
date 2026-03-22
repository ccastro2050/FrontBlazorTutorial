using System.Net.Http.Json;
using System.Text.Json;

namespace FrontBlazorTutorial.Services
{
    public class SpService
    {
        private readonly HttpClient _http;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SpService(HttpClient http)
        {
            _http = http;
        }

        public async Task<(bool exito, List<Dictionary<string, object?>> resultados, string mensaje)>
            EjecutarSpAsync(string nombreSP, Dictionary<string, object?>? parametros = null)
        {
            try
            {
                var payload = new Dictionary<string, object?> { ["nombreSP"] = nombreSP };
                if (parametros != null)
                {
                    foreach (var kvp in parametros)
                        payload[kvp.Key] = kvp.Value;
                }

                var respuesta = await _http.PostAsJsonAsync("/api/procedimientos/ejecutarsp", payload);
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? ""
                    : contenido.TryGetProperty("Mensaje", out JsonElement msg2)
                        ? msg2.GetString() ?? ""
                        : "";

                if (!respuesta.IsSuccessStatusCode)
                {
                    string detalle = contenido.TryGetProperty("detalle", out JsonElement det)
                        ? det.GetString() ?? mensaje
                        : mensaje;
                    return (false, new(), detalle);
                }

                var resultados = new List<Dictionary<string, object?>>();

                JsonElement datosArray;
                if (contenido.TryGetProperty("resultados", out datosArray) ||
                    contenido.TryGetProperty("Resultados", out datosArray))
                {
                    if (datosArray.ValueKind == JsonValueKind.Array)
                    {
                        resultados = ConvertirDatos(datosArray);
                    }
                }

                return (true, resultados, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, new(), $"Error de conexion: {ex.Message}");
            }
        }

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
                        JsonValueKind.Number => propiedad.Value.TryGetInt32(out int i) ? i : propiedad.Value.GetDouble(),
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
