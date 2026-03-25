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
}
