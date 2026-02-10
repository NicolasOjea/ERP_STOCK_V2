using Pos.Application.Abstractions;
using Pos.Application.Common;
using Pos.Infrastructure;
using Pos.WebApi.Auth;
using Pos.WebApi.Context;
using Pos.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Railway/Heroku-style DATABASE_URL support
var configuredConn = builder.Configuration.GetConnectionString("Default");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL")
    ?? builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration["DATABASE_PUBLIC_URL"];

if (!string.IsNullOrWhiteSpace(configuredConn))
{
    if (configuredConn.StartsWith("postgres", StringComparison.OrdinalIgnoreCase))
    {
        var mapped = MapDatabaseUrl(configuredConn);
        if (!string.IsNullOrWhiteSpace(mapped))
        {
            builder.Configuration["ConnectionStrings:Default"] = mapped;
        }
    }
    else if (string.Equals(configuredConn, "DATABASE_URL", StringComparison.OrdinalIgnoreCase)
        || string.Equals(configuredConn, "DATABASE_PUBLIC_URL", StringComparison.OrdinalIgnoreCase))
    {
        var mapped = MapDatabaseUrl(databaseUrl);
        if (!string.IsNullOrWhiteSpace(mapped))
        {
            builder.Configuration["ConnectionStrings:Default"] = mapped;
        }
    }
}

if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Default"))
    || builder.Configuration.GetConnectionString("Default")!.Contains("localhost", StringComparison.OrdinalIgnoreCase))
{
    var mapped = MapDatabaseUrl(databaseUrl);
    if (!string.IsNullOrWhiteSpace(mapped))
    {
        builder.Configuration["ConnectionStrings:Default"] = mapped;
    }
}

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
if (corsOrigins == null || corsOrigins.Length == 0)
{
    corsOrigins = new[] { "http://localhost:5173", "http://127.0.0.1:5173" };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPosAuthentication(builder.Configuration);
builder.Services.AddScoped<IRequestContext, RequestContext>();

var app = builder.Build();

app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

app.UseRouting();
app.UseCors("AppCors");

app.UseAuthentication();
app.UseMiddleware<RequestContextMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string? MapDatabaseUrl(string? databaseUrl)
{
    if (string.IsNullOrWhiteSpace(databaseUrl))
    {
        return null;
    }

    if (!databaseUrl.StartsWith("postgres", StringComparison.OrdinalIgnoreCase))
    {
        return null;
    }

    if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
    {
        return null;
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
    var database = uri.AbsolutePath.TrimStart('/');

    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port.ToString() : "5432";

    return $"Host={host};Port={port};Database={database};Username={username};Password={password};Ssl Mode=Require;Trust Server Certificate=true";
}

public partial class Program
{
}
