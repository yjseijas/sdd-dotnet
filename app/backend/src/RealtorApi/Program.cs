using FluentValidation;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddSlices(typeof(Program).Assembly);
builder.Services.AddHandlers(typeof(Program).Assembly);

var app = builder.Build();

app.MapSlices();

app.Run();

public partial class Program { }
