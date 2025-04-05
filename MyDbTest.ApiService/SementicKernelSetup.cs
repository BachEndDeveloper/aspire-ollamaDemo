using System.Data.Common;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp;

namespace MyDbTest.ApiService;

public static class SementicKernelSetup
{
    public static IHostApplicationBuilder AddSemanticKernelCustom(this IHostApplicationBuilder builder)
    {
        var prop = builder.Configuration.GetConnectionString("chat");
        
        DbConnectionStringBuilder dbConnectionStringBuilder = new DbConnectionStringBuilder();
        dbConnectionStringBuilder.ConnectionString = builder.Configuration.GetConnectionString("chat");
        dbConnectionStringBuilder.TryGetValue("endpoint", out var uri);
        dbConnectionStringBuilder.TryGetValue("model", out var model);

#pragma warning disable SKEXP0070
        builder.Services.AddOllamaChatCompletion((string)model!, new Uri((string)uri!));
#pragma restore disable SKEXP0070

        builder.Services.AddSingleton<CalculatorPlugin>();
        builder.Services.AddSingleton<NamePlugin>();

        builder.Services.AddTransient<Kernel>(sp =>
        {
            KernelPluginCollection pluginCollection = [];
            pluginCollection.AddFromObject(sp.GetRequiredService<CalculatorPlugin>());
            pluginCollection.AddFromObject(sp.GetRequiredService<NamePlugin>());

            return new Kernel(sp, pluginCollection);
        });
        return builder;
    }
}