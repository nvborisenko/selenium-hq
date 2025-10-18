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
    /// Records network traffic and returns a HAR recorder that can be used to save the recorded traffic.
    /// </summary>
    /// <param name="bidi">The BiDi instance.</param>
    /// <param name="options">Optional configuration options.</param>
    /// <returns>A task that represents the asynchronous operation and returns a IHarRecorder.</returns>
    public static async Task<IHarRecorder> RecordHarAsync(this BiDi bidi, HarRecordingOptions? options = null)
    {
        if (bidi is null) throw new ArgumentNullException(nameof(bidi));

        var recorder = new HarRecorder(bidi, options ?? new HarRecordingOptions());
        await recorder.StartAsync().ConfigureAwait(false);
        return recorder;
    }
}

/// <summary>
/// Options for HAR recording.
/// </summary>
public sealed class HarRecordingOptions
{
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
/// Interface for recording network traffic and saving it as HAR format.
/// </summary>
public interface IHarRecorder : IAsyncDisposable
{
    /// <summary>
    /// Saves the captured network traffic to a HAR file.
    /// </summary>
    /// <param name="filePath">The path where the HAR file should be saved.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveAsync(string filePath);
}

/// <summary>
/// Records network traffic and provides methods to save it as HAR format.
/// </summary>
public sealed class HarRecorder : IHarRecorder
{
    private readonly BiDi _bidi;
    private readonly HarRecordingOptions _options;
    private readonly HarFile _harFile;
    private readonly Dictionary<string, HarEntry> _pendingRequests;
    private readonly string _tempFilePath;
    private readonly object _tempFileLock = new object();
    private Subscription? _beforeRequestSubscription;
    private Subscription? _responseStartedSubscription;
    private Subscription? _responseCompletedSubscription;
    private Collector? _dataCollector;

    internal HarRecorder(BiDi bidi, HarRecordingOptions options)
    {
        _bidi = bidi ?? throw new ArgumentNullException(nameof(bidi));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _harFile = new HarFile();
        _pendingRequests = new Dictionary<string, HarEntry>();
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"selenium-har-{Guid.NewGuid()}.jsonl");

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
        // Always create data collector for capturing request and response bodies
        _dataCollector = await _bidi.Network.AddDataCollectorAsync([DataType.Request, DataType.Response], 200000000).ConfigureAwait(false);

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

    private async void OnResponseCompleted(ResponseCompletedEventArgs args)
    {
        HarEntry? entry = null;
        
        lock (_pendingRequests)
        {
            if (_pendingRequests.TryGetValue(args.Request.Request.Id, out entry))
            {
                entry.Response = ConvertResponse(args.Response);
                
                // Calculate total time
                var timings = args.Request.Timings;
                entry.Time = CalculateTotalTime(timings);
            }
        }

        if (entry != null)
        {
            // Retrieve request and response bodies
            if (_dataCollector != null)
            {
                try
                {
                    // Get request body
                    var requestBody = await _bidi.Network.GetDataAsync(DataType.Request, args.Request.Request).ConfigureAwait(false);
                    if (requestBody != null)
                    {
                        entry.Request.PostData = new HarPostData
                        {
                            MimeType = GetContentType(entry.Request.Headers),
                            Text = (string)requestBody
                        };
                    }
                }
                catch
                {
                    // Request body may not be available for all requests (e.g., GET requests)
                }

                try
                {
                    // Get response body
                    var responseBody = await _bidi.Network.GetDataAsync(DataType.Response, args.Request.Request).ConfigureAwait(false);
                    if (responseBody != null)
                    {
                        var bodyText = (string)responseBody;
                        entry.Response.Content.Text = bodyText;
                        entry.Response.Content.Size = System.Text.Encoding.UTF8.GetByteCount(bodyText);
                    }
                }
                catch
                {
                    // Response body may not be available for all responses
                }
            }

            lock (_pendingRequests)
            {
                // Flush entry to temp file instead of keeping in memory
                FlushEntryToTempFile(entry);
                _pendingRequests.Remove(args.Request.Request.Id);
            }
        }
    }

    private void FlushEntryToTempFile(HarEntry entry)
    {
        lock (_tempFileLock)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var entryJson = JsonSerializer.Serialize(entry, jsonOptions);
            File.AppendAllText(_tempFilePath, entryJson + Environment.NewLine);
        }
    }

    private string GetContentType(List<HarHeader> headers)
    {
        var contentTypeHeader = headers.FirstOrDefault(h => h.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase));
        return contentTypeHeader?.Value ?? "application/octet-stream";
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

    private void LoadEntriesFromTempFile()
    {
        lock (_tempFileLock)
        {
            _harFile.Log.Entries.Clear();

            if (File.Exists(_tempFilePath))
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var lines = File.ReadAllLines(_tempFilePath);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var entry = JsonSerializer.Deserialize<HarEntry>(line, jsonOptions);
                        if (entry != null)
                        {
                            _harFile.Log.Entries.Add(entry);
                        }
                    }
                }
            }
        }
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

        // Load entries from temp file before saving
        LoadEntriesFromTempFile();

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

        if (_dataCollector != null)
        {
            await _bidi.Network.RemoveDataCollectorAsync(_dataCollector).ConfigureAwait(false);
        }

        // Clean up temp file
        if (File.Exists(_tempFilePath))
        {
            try
            {
                File.Delete(_tempFilePath);
            }
            catch
            {
                // Ignore errors when deleting temp file
            }
        }
    }
}
