# HAR Capture Extension for BiDi

This extension provides the ability to capture network traffic using the BiDi protocol and export it to HAR (HTTP Archive) format.

## Usage Example

```csharp
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.BiDi;
using OpenQA.Selenium.BiDi.Network.Har;

// Create a WebDriver instance with BiDi enabled
var options = new ChromeOptions();
options.AddArgument("--remote-allow-origins=*");
options.EnableBiDi();

using var driver = new ChromeDriver(options);

// Connect to BiDi
await using var bidi = await driver.AsBiDiAsync();

// Start capturing network traffic with body content
await using var recorder = await bidi.CaptureNetworkTrafficAsync(new HarCaptureOptions
{
    BrowserName = "Chrome",
    BrowserVersion = "120.0",
    IncludeContent = true  // Enable request/response body capture
});

// Navigate to a page
driver.Navigate().GoToUrl("https://www.example.com");

// Wait for some network activity
await Task.Delay(2000);

// Save the captured traffic to a HAR file
await recorder.SaveAsync("network-traffic.har");

// Or get the HAR object directly
var har = recorder.GetHar();
Console.WriteLine($"Captured {har.Log.Entries.Count} network requests");
```

## HAR Capture Options

The `HarCaptureOptions` class allows you to configure the capture:

- `IncludeContent`: Whether to include request/response body content (default: false). When enabled, a data collector is created to capture request and response bodies.
- `BrowserName`: The browser name to include in the HAR metadata
- `BrowserVersion`: The browser version to include in the HAR metadata

**Note:** Setting `IncludeContent = true` will create a network data collector that captures request and response bodies. This may increase memory usage for large requests/responses.

## HAR File Format

The generated HAR file follows the HAR 1.2 specification and includes:

- Request details (method, URL, headers, cookies, query parameters)
- Response details (status code, headers, content type)
- Timing information (DNS, connect, SSL, send, wait, receive)
- Request/response body content (when `IncludeContent` is enabled)
- Metadata (browser info, timestamps)

## Disposing the Recorder

The `HarRecorder` implements `IAsyncDisposable` and should be disposed properly to unsubscribe from network events and clean up the data collector:

```csharp
await using var recorder = await bidi.CaptureNetworkTrafficAsync();
// ... capture network traffic ...
// Dispose is called automatically when leaving the using block
```
