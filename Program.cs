using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using As.Api.Data;
using As.Api.Services;
using As.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

// --- configurações (json, env vars) ---
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(); // importante: lê as env vars do Render

// --- pegar connection string (prefer env var ConnectionStrings__DefaultConnection) ---
var conn = builder.Configuration["ConnectionStrings:DefaultConnection"];

if (string.IsNullOrEmpty(conn))
{
    // alternativa: Render pode fornecer DATABASE_URL (url postgres). Convertendo dinamicamente:
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL") ?? builder.Configuration["DATABASE_URL"];
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        conn = ConvertDatabaseUrl(databaseUrl);
    }
}

if (string.IsNullOrEmpty(conn))
{
    throw new Exception("Connection string não encontrada. Defina ConnectionStrings__DefaultConnection ou DATABASE_URL.");
}

// --- registrar um único DbContext (usaremos Postgres no Render) ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(conn));

// --- CORS (você já tinha) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// --- JWT config ---
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CHANGE_THIS_TO_STRONG_KEY";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "AsApi";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

// --- Email settings e DI (ok) ---
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddScoped<IAccessService, AccessService>();

var app = builder.Build();

// --- aplicar migrations (opcional, útil no deploy automático) ---
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao aplicar migrations");
        // não throw para evitar crash automático se preferir — mas para debug pode rethrow
        throw;
    }
}

// --- configurar portas (Render usa PORT) ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Clear();
app.Urls.Add($"http://0.0.0.0:{port}");

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static string ConvertDatabaseUrl(string databaseUrl)
{
    // Converte postgres://user:pass@host:port/dbname para string Npgsql
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var host = uri.Host;
    var port = uri.Port;
    var db = uri.AbsolutePath.TrimStart('/');
    var user = userInfo[0];
    var pass = userInfo.Length > 1 ? userInfo[1] : "";

    return $"Host={host};Port={port};Database={db};Username={user};Password={pass};Ssl Mode=Require;Trust Server Certificate=true;";
}
