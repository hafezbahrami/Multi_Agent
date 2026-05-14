using MQ.Office.MCP.Server.Excel;
using MQ.Office.MCP.Server.Word;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace MQ.Office.MCP.Server;

public static class AppHostConfiguration
{
    public static IServiceCollection AddOfficeMcpServices(this IServiceCollection services)
    {
        services.AddSingleton<IWordService, WordService>();
        services.AddSingleton<IExcelService, ExcelService>();

        services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly(typeof(WordTools).Assembly)
            .WithToolsFromAssembly(typeof(ExcelTools).Assembly);

        return services;
    }
}
