using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MyDbTest.ApiService;
using OllamaSharp;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddOllamaSharpChatClient("chat");

builder.AddSemanticKernelCustom();

// builder.AddKeyedOllamaSharpChatClient("chat");
// builder.AddOllamaApiClient("chat");

// #pragma warning disable SKEXP0070
// builder.Services.AddOllamaChatCompletion(GetRequiredService<OllamaApiClient>());

builder.AddMongoDBClient(connectionName: "mongodb");

var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

string[] summaries =
    ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapPost("/chat", async (IChatClient chatClient, string question) =>
{
    var response = await chatClient.GetResponseAsync(question);
    return response.Text;
});

app.MapPost("/semantickernel", async (IChatCompletionService chatCompletionService, Kernel kernel, string question) =>
{
    ChatHistory chatHistory = new();
    chatHistory.AddSystemMessage("You can use the functions available to you, to calculate math problems.");
    chatHistory.AddUserMessage(question);
    
#pragma warning disable SKEXP0070
    PromptExecutionSettings executionSettings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
#pragma restore disable SKEXP0070
    
    var response =
        await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings: executionSettings,
            kernel: kernel);

    return response.Content;
});

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapGet("/movies/seed", async (IMongoClient client) =>
{
    var db = client.GetDatabase("mongodb");
    await db.CreateCollectionAsync("Movies");
    //db.DropCollection("Movies");

    var collection = db.GetCollection<MovieEntity>("Movies");

    var movies = new[]
    {
        new MovieEntity
        {
            Title = "The Shawshank Redemption",
            ReleaseDate = new DateOnly(1994, 10, 14),
            Genre = "Drama"
        },
        new MovieEntity
        {
            Title = "The Godfather",
            ReleaseDate = new DateOnly(1972, 3, 24),
            Genre = "Crime"
        },
        new MovieEntity
        {
            Title = "The Dark Knight",
            ReleaseDate = new DateOnly(2008, 7, 18),
            Genre = "Action"
        }
    };

    try
    {
        var movie1 = new MovieEntity
            { Title = "The Shawshank Redemption", ReleaseDate = new DateOnly(1994, 10, 14), Genre = "Drama" };
        collection.InsertOne(movie1);
        // collection.InsertMany(movies);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}).WithName("SeedMovies");

app.MapGet("/movies", async (IMongoClient client) =>
    {
        var db = client.GetDatabase("mongodb");
        var collection = db.GetCollection<MovieEntity>("Movies");

        var movies = await collection.Find(_ => true).ToListAsync();

        Console.WriteLine(JsonSerializer.Serialize(movies));
        return movies;
    })
    .WithName("GetMovies");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class Movie()
{
    public string Title { get; init; } = string.Empty;
    public DateOnly ReleaseDate { get; init; }
    public string? Genre { get; init; } = string.Empty;
}

public class MovieEntity
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    public string Genre { get; set; } = string.Empty;

    [MongoDB.Bson.Serialization.Attributes.BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MongoDB.Bson.Serialization.Attributes.BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}