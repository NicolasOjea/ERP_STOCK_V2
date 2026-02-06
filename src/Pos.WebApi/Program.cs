using Pos.Application.Abstractions;
using Pos.Application.Common;
using Pos.Infrastructure;
using Pos.WebApi.Auth;
using Pos.WebApi.Context;
using Pos.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPosAuthentication(builder.Configuration);
builder.Services.AddScoped<IRequestContext, RequestContext>();

var app = builder.Build();

app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<RequestContextMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
