// <copyright file="BiDi.HarExtensions.cs" company="Selenium Committers">
// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.Network.Har;

/// <summary>
/// Extension methods for BiDi class to capture network traffic as HAR.
/// </summary>
public static class BiDiHarExtensions
{
    /// <summary>
    /// Captures network traffic and returns a HAR recorder that can be used to save the captured traffic.
    /// </summary>
    /// <param name="bidi">The BiDi instance.</param>
    /// <param name="options">Optional configuration options.</param>
    /// <returns>A task that represents the asynchronous operation and returns a HarRecorder.</returns>
    public static async Task<HarRecorder> CaptureNetworkTrafficAsync(this BiDi bidi, HarCaptureOptions? options = null)
    {
        if (bidi is null) throw new ArgumentNullException(nameof(bidi));

        var recorder = new HarRecorder(bidi, options ?? new HarCaptureOptions());
        await recorder.StartAsync().ConfigureAwait(false);
        return recorder;
    }
}

/// <summary>
/// Options for HAR capture.
/// </summary>
public sealed class HarCaptureOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to include response content in the HAR file.
    /// </summary>
    public bool IncludeResponseContent { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include request/response body content in the HAR file.
    /// </summary>
    public bool IncludeContent { get; set; } = false;

    /// <summary>
    /// Gets or sets the browser name to include in the HAR file.
    /// </summary>
    public string? BrowserName { get; set; }

    /// <summary>
    /// Gets or sets the browser version to include in the HAR file.
    /// </summary>
    public string? BrowserVersion { get; set; }
}

/// <summary>
/// Records network traffic and provides methods to save it as HAR format.
/// </summary>
public sealed class HarRecorder : IAsyncDisposable
{
    private readonly BiDi _bidi;
    private readonly HarCaptureOptions _options;
    private readonly HarFile _harFile;
    private readonly Dictionary<string, HarEntry> _pendingRequests;
    private Subscription? _beforeRequestSubscription;
    private Subscription? _responseStartedSubscription;
    private Subscription? _responseCompletedSubscription;

    internal HarRecorder(BiDi bidi, HarCaptureOptions options)
    {
        _bidi = bidi ?? throw new ArgumentNullException(nameof(bidi));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _harFile = new HarFile();
        _pendingRequests = new Dictionary<string, HarEntry>();

        if (!string.IsNullOrEmpty(options.BrowserName))
        {
            _harFile.Log.Browser = new HarBrowser
            {
                Name = options.BrowserName,
                Version = options.BrowserVersion ?? string.Empty
            };
        }
    }

    internal async Task StartAsync()
    {
        _beforeRequestSubscription = await _bidi.Network.OnBeforeRequestSentAsync(OnBeforeRequestSent).ConfigureAwait(false);
        _responseStartedSubscription = await _bidi.Network.OnResponseStartedAsync(OnResponseStarted).ConfigureAwait(false);
        _responseCompletedSubscription = await _bidi.Network.OnResponseCompletedAsync(OnResponseCompleted).ConfigureAwait(false);
    }

    private void OnBeforeRequestSent(BeforeRequestSentEventArgs args)
    {
        var entry = new HarEntry
        {
            StartedDateTime = args.Timestamp.ToString("o"),
            Request = ConvertRequest(args.Request),
            Timings = ConvertTimings(args.Request.Timings),
            Time = 0
        };

        if (args.Context != null)
        {
            entry.Pageref = args.Context.Id;
        }

        lock (_pendingRequests)
        {
            _pendingRequests[args.Request.Request.Id] = entry;
        }
    }

    private void OnResponseStarted(ResponseStartedEventArgs args)
    {
        lock (_pendingRequests)
        {
            if (_pendingRequests.TryGetValue(args.Request.Request.Id, out var entry))
            {
                entry.Response = ConvertResponse(args.Response);
            }
        }
    }

    private void OnResponseCompleted(ResponseCompletedEventArgs args)
    {
        lock (_pendingRequests)
        {
            if (_pendingRequests.TryGetValue(args.Request.Request.Id, out var entry))
            {
                entry.Response = ConvertResponse(args.Response);
                
                // Calculate total time
                var timings = args.Request.Timings;
                entry.Time = CalculateTotalTime(timings);

                _harFile.Log.Entries.Add(entry);
                _pendingRequests.Remove(args.Request.Request.Id);
            }
        }
    }

    private HarRequest ConvertRequest(RequestData request)
    {
        var harRequest = new HarRequest
        {
            Method = request.Method,
            Url = request.Url,
            HttpVersion = "HTTP/1.1",
            HeadersSize = request.HeadersSize ?? -1,
            BodySize = request.BodySize ?? -1
        };

        foreach (var header in request.Headers)
        {
            harRequest.Headers.Add(new HarHeader
            {
                Name = header.Name,
                Value = (string)header.Value
            });
        }

        foreach (var cookie in request.Cookies)
        {
            harRequest.Cookies.Add(new HarCookie
            {
                Name = cookie.Name,
                Value = (string)cookie.Value,
                Domain = cookie.Domain,
                Path = cookie.Path,
                HttpOnly = cookie.HttpOnly,
                Secure = cookie.Secure,
                Expires = cookie.Expiry?.ToString("o")
            });
        }

        // Parse query string from URL
        var uri = new Uri(request.Url);
        if (!string.IsNullOrEmpty(uri.Query))
        {
            var queryString = uri.Query.TrimStart('?');
            var queryParams = queryString.Split('&');
            foreach (var param in queryParams)
            {
                var parts = param.Split('=', 2);
                harRequest.QueryString.Add(new HarQueryParam
                {
                    Name = Uri.UnescapeDataString(parts[0]),
                    Value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty
                });
            }
        }

        return harRequest;
    }

    private HarResponse ConvertResponse(ResponseData response)
    {
        var harResponse = new HarResponse
        {
            Status = response.Status,
            StatusText = response.StatusText,
            HttpVersion = response.Protocol,
            HeadersSize = response.HeadersSize ?? -1,
            BodySize = response.BodySize ?? -1,
            Content = new HarContent
            {
                Size = response.BodySize ?? 0,
                MimeType = response.MimeType
            }
        };

        foreach (var header in response.Headers)
        {
            var headerValue = (string)header.Value;
            harResponse.Headers.Add(new HarHeader
            {
                Name = header.Name,
                Value = headerValue
            });

            // Check for redirect URL
            if (header.Name.Equals("Location", StringComparison.OrdinalIgnoreCase))
            {
                harResponse.RedirectURL = headerValue;
            }
        }

        return harResponse;
    }

    private HarTimings ConvertTimings(FetchTimingInfo timings)
    {
        return new HarTimings
        {
            Blocked = -1,
            Dns = CalculateDuration(timings.DnsStart, timings.DnsEnd),
            Connect = CalculateDuration(timings.ConnectStart, timings.ConnectEnd),
            Ssl = CalculateDuration(timings.TlsStart, timings.ConnectEnd),
            Send = CalculateDuration(timings.RequestStart, timings.RequestStart),
            Wait = CalculateDuration(timings.RequestStart, timings.ResponseStart),
            Receive = CalculateDuration(timings.ResponseStart, timings.ResponseEnd)
        };
    }

    private double CalculateDuration(double start, double end)
    {
        if (start < 0 || end < 0 || end < start)
        {
            return -1;
        }
        return end - start;
    }

    private double CalculateTotalTime(FetchTimingInfo timings)
    {
        if (timings.FetchStart >= 0 && timings.ResponseEnd >= 0 && timings.ResponseEnd >= timings.FetchStart)
        {
            return timings.ResponseEnd - timings.FetchStart;
        }
        return 0;
    }

    /// <summary>
    /// Gets the captured HAR file.
    /// </summary>
    /// <returns>The HAR file containing all captured network traffic.</returns>
    public HarFile GetHar()
    {
        return _harFile;
    }

    /// <summary>
    /// Saves the captured network traffic to a HAR file.
    /// </summary>
    /// <param name="filePath">The path where the HAR file should be saved.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SaveAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(_harFile, options);
        await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes the recorder and unsubscribes from network events.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_beforeRequestSubscription != null)
        {
            await _beforeRequestSubscription.DisposeAsync().ConfigureAwait(false);
        }

        if (_responseStartedSubscription != null)
        {
            await _responseStartedSubscription.DisposeAsync().ConfigureAwait(false);
        }

        if (_responseCompletedSubscription != null)
        {
            await _responseCompletedSubscription.DisposeAsync().ConfigureAwait(false);
        }
    }
}
