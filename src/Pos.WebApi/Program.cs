using Pos.Application.Abstractions;
using Pos.Application.Common;
using Pos.Infrastructure;
using Pos.WebApi.Auth;
using Pos.WebApi.Context;
using Pos.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

if (builder.Environment.IsDevelopment())
{
    var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
        ?? new[] { "http://localhost:5173", "http://127.0.0.1:5173" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevCors", policy =>
            policy.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod());
    });
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPosAuthentication(builder.Configuration);
builder.Services.AddScoped<IRequestContext, RequestContext>();

var app = builder.Build();

app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}

app.UseAuthentication();
app.UseMiddleware<RequestContextMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
