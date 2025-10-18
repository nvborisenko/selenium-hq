// <copyright file="HarEntry.cs" company="Selenium Committers">
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

using System.Collections.Generic;

namespace OpenQA.Selenium.BiDi.Network.Har;

/// <summary>
/// Represents a HAR entry object containing request and response information.
/// </summary>
public sealed class HarEntry
{
    /// <summary>
    /// Gets or sets the reference to the parent page.
    /// </summary>
    public string? Pageref { get; set; }

    /// <summary>
    /// Gets or sets the date and time stamp of the request start.
    /// </summary>
    public string StartedDateTime { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total elapsed time in milliseconds.
    /// </summary>
    public double Time { get; set; }

    /// <summary>
    /// Gets or sets the request information.
    /// </summary>
    public HarRequest Request { get; set; } = new HarRequest();

    /// <summary>
    /// Gets or sets the response information.
    /// </summary>
    public HarResponse Response { get; set; } = new HarResponse();

    /// <summary>
    /// Gets or sets the cache information.
    /// </summary>
    public HarCache Cache { get; set; } = new HarCache();

    /// <summary>
    /// Gets or sets the timing information.
    /// </summary>
    public HarTimings Timings { get; set; } = new HarTimings();

    /// <summary>
    /// Gets or sets the server IP address.
    /// </summary>
    public string? ServerIPAddress { get; set; }

    /// <summary>
    /// Gets or sets the connection information.
    /// </summary>
    public string? Connection { get; set; }

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents a HAR request object.
/// </summary>
public sealed class HarRequest
{
    /// <summary>
    /// Gets or sets the request method.
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the absolute URL of the request.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request HTTP version.
    /// </summary>
    public string HttpVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of cookie objects.
    /// </summary>
    public List<HarCookie> Cookies { get; set; } = new List<HarCookie>();

    /// <summary>
    /// Gets or sets the list of header objects.
    /// </summary>
    public List<HarHeader> Headers { get; set; } = new List<HarHeader>();

    /// <summary>
    /// Gets or sets the list of query parameter objects.
    /// </summary>
    public List<HarQueryParam> QueryString { get; set; } = new List<HarQueryParam>();

    /// <summary>
    /// Gets or sets the posted data information.
    /// </summary>
    public HarPostData? PostData { get; set; }

    /// <summary>
    /// Gets or sets the total number of bytes from the start of the HTTP request message.
    /// </summary>
    public long HeadersSize { get; set; } = -1;

    /// <summary>
    /// Gets or sets the size of the request body in bytes.
    /// </summary>
    public long BodySize { get; set; } = -1;

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents a HAR response object.
/// </summary>
public sealed class HarResponse
{
    /// <summary>
    /// Gets or sets the response status code.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the response status description.
    /// </summary>
    public string StatusText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response HTTP version.
    /// </summary>
    public string HttpVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of cookie objects.
    /// </summary>
    public List<HarCookie> Cookies { get; set; } = new List<HarCookie>();

    /// <summary>
    /// Gets or sets the list of header objects.
    /// </summary>
    public List<HarHeader> Headers { get; set; } = new List<HarHeader>();

    /// <summary>
    /// Gets or sets the response body content.
    /// </summary>
    public HarContent Content { get; set; } = new HarContent();

    /// <summary>
    /// Gets or sets the redirection target URL from the Location response header.
    /// </summary>
    public string RedirectURL { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of bytes from the start of the HTTP response message.
    /// </summary>
    public long HeadersSize { get; set; } = -1;

    /// <summary>
    /// Gets or sets the size of the received response body in bytes.
    /// </summary>
    public long BodySize { get; set; } = -1;

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents a HAR cookie object.
/// </summary>
public sealed class HarCookie
{
    /// <summary>
    /// Gets or sets the cookie name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cookie value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path pertaining to the cookie.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the host of the cookie.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Gets or sets the cookie expiration time.
    /// </summary>
    public string? Expires { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is HTTP only.
    /// </summary>
    public bool? HttpOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cookie is secure.
    /// </summary>
    public bool? Secure { get; set; }

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents a HAR header object.
/// </summary>
public sealed class HarHeader
{
    /// <summary>
    /// Gets or sets the header name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the header value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents a HAR query parameter object.
/// </summary>
public sealed class HarQueryParam
{
    /// <summary>
    /// Gets or sets the query parameter name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the query parameter value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents HAR post data information.
/// </summary>
public sealed class HarPostData
{
    /// <summary>
    /// Gets or sets the MIME type of the posted data.
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of posted parameters.
    /// </summary>
    public List<HarPostParam>? Params { get; set; }

    /// <summary>
    /// Gets or sets the plain text posted data.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents a HAR post parameter object.
/// </summary>
public sealed class HarPostParam
{
    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameter value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the name of the uploaded file.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the content type of the uploaded file.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents HAR content information.
/// </summary>
public sealed class HarContent
{
    /// <summary>
    /// Gets or sets the length of the content in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the length of the returned content in bytes.
    /// </summary>
    public long? Compression { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the response.
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response body text.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the encoding used for the response text.
    /// </summary>
    public string? Encoding { get; set; }

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents HAR cache information.
/// </summary>
public sealed class HarCache
{
    /// <summary>
    /// Gets or sets the state of the cache entry before the request.
    /// </summary>
    public HarCacheEntry? BeforeRequest { get; set; }

    /// <summary>
    /// Gets or sets the state of the cache entry after the request.
    /// </summary>
    public HarCacheEntry? AfterRequest { get; set; }

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents a HAR cache entry object.
/// </summary>
public sealed class HarCacheEntry
{
    /// <summary>
    /// Gets or sets the expiration time of the cache entry.
    /// </summary>
    public string? Expires { get; set; }

    /// <summary>
    /// Gets or sets the last accessed time of the cache entry.
    /// </summary>
    public string LastAccess { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ETag.
    /// </summary>
    public string ETag { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of times the entry has been opened.
    /// </summary>
    public int HitCount { get; set; }

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents HAR timing information.
/// </summary>
public sealed class HarTimings
{
    /// <summary>
    /// Gets or sets the time spent in a queue waiting for a network connection.
    /// </summary>
    public double Blocked { get; set; } = -1;

    /// <summary>
    /// Gets or sets the DNS resolution time.
    /// </summary>
    public double Dns { get; set; } = -1;

    /// <summary>
    /// Gets or sets the time required to create a TCP connection.
    /// </summary>
    public double Connect { get; set; } = -1;

    /// <summary>
    /// Gets or sets the time required for SSL/TLS negotiation.
    /// </summary>
    public double Ssl { get; set; } = -1;

    /// <summary>
    /// Gets or sets the time required to send the HTTP request to the server.
    /// </summary>
    public double Send { get; set; } = -1;

    /// <summary>
    /// Gets or sets the waiting for a response from the server.
    /// </summary>
    public double Wait { get; set; } = -1;

    /// <summary>
    /// Gets or sets the time required to read the entire response from the server.
    /// </summary>
    public double Receive { get; set; } = -1;

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}
