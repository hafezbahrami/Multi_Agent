using MQ.Office.MCP.Server;
using Microsoft.Extensions.DependencyInjection;

namespace MQ.Office.MCP.Server.Tests;

public class AppHostConfigurationTests
{
    [Fact]
    public void AddOfficeMcpServices_RegistersWordAndExcelServices()
    {
        var services = new ServiceCollection();
        services.AddOfficeMcpServices();
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<MQ.Office.MCP.Server.Word.IWordService>());
        Assert.NotNull(provider.GetService<MQ.Office.MCP.Server.Excel.IExcelService>());
    }
}
