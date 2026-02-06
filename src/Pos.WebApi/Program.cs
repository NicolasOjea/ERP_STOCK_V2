using Pos.Application.Abstractions;
using Pos.Application.UseCases.Health;
using Pos.Infrastructure.Persistence;
using Pos.WebApi.Auth;
using Pos.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddPosAuthentication();

builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddSingleton<PosDbContext>();

var app = builder.Build();

app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
