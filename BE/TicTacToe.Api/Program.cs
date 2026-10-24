using System.Text.Json.Serialization;
using TicTacToe.Api.Repositories;
using TicTacToe.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers with JSON configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configure CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Dependency Injection
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();
builder.Services.AddSingleton<IAiStrategy, AiStrategy>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IScoreboardService, ScoreboardService>();

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Tic Tac Toe REST API",
        Version = "v1",
        Description = "Full-Stack Senior Software Engineer Assessment - Tic Tac Toe Backend Web API"
    });
});

var app = builder.Build();

// Configure Swagger
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TicTacToe.Api v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAngular");

app.MapControllers();

app.Run();

// Make Program accessible for WebApplicationFactory in integration tests if needed
public partial class Program { }
