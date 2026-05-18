using ChatApp.Components;
using ChatApp.Data;
using ChatApp.Repositories;
using ChatApp.Services;
using DotNetEnv;
using GenAI.CSharp.Services;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using MudBlazor;
using MudBlazor.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5028", "https://0.0.0.0:7002");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5028")
});

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();

builder.Services.AddScoped<ChatService>();
builder.Services.AddGenAiLesson();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite("Data Source=escola_sabatina.db"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ChatRepository>();
builder.Services.AddScoped<ChatStateContainer>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport(o => o.Stateless = false)
    .AddGenAiTools();

builder.Services.AddSingleton(sp =>
{
    var transport = new HttpClientTransport(new ()
    {
        Endpoint = new Uri("http://localhost:5028/mcp")
    });
    var options = new McpClientOptions
    {
        Capabilities = new ClientCapabilities
        {
            Sampling = new() // <-- ISSO AQUI resolve o InvalidOperationException
        }
    };
    return McpClient.CreateAsync(transport, options).GetAwaiter().GetResult();
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.MapMcp("/mcp");

app.Run();
