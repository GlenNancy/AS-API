using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using As.Api.Data;
using Microsoft.EntityFrameworkCore;
using As.Api.Services;
using As.Api.Settings;


var builder = WebApplication.CreateBuilder(args);

// CORS liberado
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// configuração de DB - aqui exemplo SQLite (dev). Troque para SQL Server/Postgres na nuvem.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// jwt config
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CHANGE_THIS_TO_STRONG_KEY";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "AsApi";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});


builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

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
