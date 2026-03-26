using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace OpenQA.Selenium.BiDi.Cdp;

internal class CdpTests : BiDiTestFixture
{
    [Test]
    public async Task GetSessionAsync_ShouldReturnSession()
    {
        var cdp = bidi.AsModule<CdpModule>();

        var result = await cdp.GetSessionAsync(context);
        Assert.IsNotNull(result);
        Assert.IsNotEmpty(result.Session);
    }

    [Test]
    public async Task SendCommandAsync_ShouldReturnResult()
    {
        var cdp = bidi.AsModule<CdpModule>();

        var session = await cdp.GetSessionAsync(context);
        
        await cdp.SendCommandAsync("Page.navigate", new ()
        {
            ["url"] = "https://www.example.com"
        }, new SendCommandOptions { Session = session.Session });

        await Task.Delay(2000);

        await cdp.SendCommandAsync("Page.reload", new ()
        {
            ["ignoreCache"] = true
        }, new SendCommandOptions { Session = session.Session });
        
        await Task.Delay(2000);
    }
}
