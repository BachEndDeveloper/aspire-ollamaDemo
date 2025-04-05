using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace MyDbTest.ApiService;

public class NamePlugin(ILogger<NamePlugin> logger)
{
    private readonly ILogger<NamePlugin> _logger = logger;

    [KernelFunction]
    [Description("This function accepts a name and returns the name in uppercase")]
    [return: Description("Returns the name in uppercase")]
    public string GetUpperCaseName(string name)
    {
        _logger.LogInformation("GetUpperCaseName: {Name}", name);
        
        return name.ToUpper();
    }

    [KernelFunction]
    [Description("This function accepts a name and returns the name in lowercase")]
    [return: Description("Returns the name in lowercase")]
    public string GetLowerCaseName(string name)
    {
        return name.ToLower();
    }
}