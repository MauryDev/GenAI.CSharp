using ChatApp.Components;
using ChatApp.Data;
using ChatApp.Repositories;
using ChatApp.Services;
using DotNetEnv;
using GenAI.CSharp.Services;
using Microsoft.AspNetCore.Components;
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

builder.Services.AddScoped(sp => {
    var navManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient
    {
        BaseAddress = new Uri(navManager.BaseUri)
    };

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

builder.Services.AddScoped(sp =>
{
    var navManager = sp.GetRequiredService<NavigationManager>();

    // Constrói a URL do MCP baseada na URL atual do aplicativo
    var mcpEndpoint = new Uri(new Uri(navManager.BaseUri), "mcp");

    var transport = new HttpClientTransport(new()
    {
        Endpoint = mcpEndpoint
    });

    var options = new McpClientOptions
    {
        Capabilities = new ClientCapabilities
        {
            Sampling = new()
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
