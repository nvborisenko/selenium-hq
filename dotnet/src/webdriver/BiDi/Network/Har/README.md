# HAR Capture Extension for BiDi

This extension provides the ability to capture network traffic using the BiDi protocol and export it to HAR (HTTP Archive) format, including request and response body content.

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

// Start capturing network traffic (includes request/response bodies by default)
await using var recorder = await bidi.CaptureNetworkTrafficAsync(new HarCaptureOptions
{
    BrowserName = "Chrome",
    BrowserVersion = "120.0"
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

- `BrowserName`: The browser name to include in the HAR metadata
- `BrowserVersion`: The browser version to include in the HAR metadata

## HAR File Format

The generated HAR file follows the HAR 1.2 specification and includes:

- Request details (method, URL, headers, cookies, query parameters)
- Response details (status code, headers, content type)
- Timing information (DNS, connect, SSL, send, wait, receive)
- Request/response body content (automatically captured)
- Metadata (browser info, timestamps)

## Body Content Capture

By default, the HAR recorder captures request and response bodies for all network traffic. A network data collector is automatically created when you start capturing traffic. This provides complete visibility into all request and response payloads.

**Memory Optimization:** To minimize memory usage, captured network entries are written to a temporary file as they are completed. The entries are only loaded into memory when you call `GetHar()` or `SaveAsync()`. This allows for capturing large amounts of network traffic without consuming excessive memory.

**Note:** Capturing request/response bodies may increase memory usage for large requests/responses.

## Disposing the Recorder

The `HarRecorder` implements `IAsyncDisposable` and should be disposed properly to unsubscribe from network events and clean up the data collector:

```csharp
await using var recorder = await bidi.CaptureNetworkTrafficAsync();
// ... capture network traffic ...
// Dispose is called automatically when leaving the using block
```
