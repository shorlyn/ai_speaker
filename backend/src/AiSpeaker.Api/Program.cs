using System.Net.Http.Headers;
using AiSpeaker.Api.Data;
using AiSpeaker.Api.Modules.Chat.Services;
using AiSpeaker.Api.Modules.Conversation.Services;
using AiSpeaker.Api.Modules.Device.Services;
using AiSpeaker.Api.Modules.Provider.Abstractions;
using AiSpeaker.Api.Modules.Provider.Fake;
using AiSpeaker.Api.Modules.Provider.Ollama;
using AiSpeaker.Api.Modules.Provider.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection("Llm"));

var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "app_data");
Directory.CreateDirectory(dataDirectory);

var connectionString = builder.Configuration.GetConnectionString("AiSpeaker");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = $"Data Source={Path.Combine(dataDirectory, "ai-speaker.db")}";
}
else
{
    connectionString = connectionString.Replace("|DataDirectory|", dataDirectory, StringComparison.OrdinalIgnoreCase);
}

builder.Services.AddDbContext<AiSpeakerDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddSingleton<IAsrProvider, FakeAsrProvider>();
builder.Services.AddSingleton<ITtsProvider, FakeTtsProvider>();

var llmProvider = builder.Configuration.GetValue<string>("Llm:Provider") ?? "Ollama";
if (string.Equals(llmProvider, "Fake", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<ILlmProvider, FakeLlmProvider>();
}
else
{
builder.Services.AddHttpClient<ILlmProvider, OllamaLlmProvider>((sp, client) =>
{
    var ollamaOptions = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
    client.BaseAddress = new Uri(EnsureTrailingSlash(ollamaOptions.BaseUrl));
    client.Timeout = TimeSpan.FromSeconds(10);

    if (!string.IsNullOrWhiteSpace(ollamaOptions.ApiKey))
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ollamaOptions.ApiKey);
    }
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AiSpeakerDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();
app.UseAuthorization();
app.MapControllers();
app.Run();

static string EnsureTrailingSlash(string url)
{
    if (string.IsNullOrWhiteSpace(url))
    {
        return url;
    }

    return url.EndsWith("/") ? url : url + "/";
}
