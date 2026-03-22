using FrontBlazorTutorial.Components;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar HttpClient para conectarse a la API
// La URL base se lee de appsettings.json
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5035";
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// Registrar los servicios de la API
builder.Services.AddScoped<FrontBlazorTutorial.Services.ApiService>();

var app = builder.Build();

// Configurar el pipeline HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
