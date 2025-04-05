var builder = DistributedApplication.CreateBuilder(args);

var mongoUser = builder.AddParameter("admin", "admin");
var mongoPass = builder.AddParameter("password", "passwordLocalDevelopment1929", secret: true);

var mongo = builder.AddMongoDB("mongo", userName: mongoUser, password: mongoPass)
    .WithLifetime(ContainerLifetime.Persistent);

var mongoDb = mongo.AddDatabase("mongodb");

var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithOpenWebUI();

var chat = ollama.AddModel("chat", "llama3.2");


var apiService = builder.AddProject<Projects.MyDbTest_ApiService>("apiservice")
    .WithReference(chat)
    .WithReference(mongoDb)
    .WaitFor(chat)
    .WaitFor(mongoDb);


builder.AddProject<Projects.MyDbTest_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();