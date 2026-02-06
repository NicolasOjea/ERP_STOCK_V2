using Pos.Application.Common;
using Pos.Infrastructure;
using Pos.WebApi.Auth;
using Pos.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddPosAuthentication();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
