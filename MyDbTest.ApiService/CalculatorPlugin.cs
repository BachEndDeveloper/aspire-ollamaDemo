using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace MyDbTest.ApiService;

public class CalculatorPlugin(ILogger<CalculatorPlugin> logger)
{
    private readonly ILogger<CalculatorPlugin> _logger = logger;

    [KernelFunction]
    [Description("This functions accepts 2 numbers and will add the 2 numbers together")]
    [return: Description("Returns the sum of the 2 numbers")]
    public int Calculate(int number1, int number2)
    {
        _logger.LogInformation("Calculating {Number1} + {Number2} = {Number1}", number1, number2, number1 + number2);
        // Call API here
        return number1 + number2;
    }
}