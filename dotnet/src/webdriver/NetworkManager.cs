// <copyright file="NetworkManager.cs" company="Selenium Committers">
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

using OpenQA.Selenium.DevTools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace OpenQA.Selenium;

/// <summary>
/// Provides methods for monitoring, intercepting, and modifying network requests and responses.
/// </summary>
public class NetworkManager : INetwork
{
    private readonly Lazy<DevToolsSession> session;
    private readonly List<NetworkRequestHandler> requestHandlers = new List<NetworkRequestHandler>();
    private readonly List<NetworkResponseHandler> responseHandlers = new List<NetworkResponseHandler>();
    private readonly List<NetworkAuthenticationHandler> authenticationHandlers = new List<NetworkAuthenticationHandler>();

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkManager"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IWebDriver"/> instance on which the network should be monitored.</param>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Warnings are added to StartMonitoring and StopMonitoring")]
    [UnconditionalSuppressMessage("Trimming", "IL3050", Justification = "Warnings are added to StartMonitoring and StopMonitoring")]
    public NetworkManager(IWebDriver driver)
    {
        // Use of Lazy<T> means this exception won't be thrown until the user first
        // attempts to access the DevTools session, probably on the first call to
        // StartMonitoring().
        this.session = new Lazy<DevToolsSession>(() =>
        {
            if (driver is not IDevTools devToolsDriver)
            {
                throw new WebDriverException("Driver must implement IDevTools to use these features");
            }

            return devToolsDriver.GetDevToolsSession();
        });
    }

    /// <summary>
    /// Occurs when a browser sends a network request.
    /// </summary>
    public event EventHandler<NetworkRequestSentEventArgs>? NetworkRequestSent;

    /// <summary>
    /// Occurs when a browser receives a network response.
    /// </summary>
    public event EventHandler<NetworkResponseReceivedEventArgs>? NetworkResponseReceived;

    /// <summary>
    /// Asynchronously starts monitoring for network traffic.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RequiresUnreferencedCode("NetworkManager is currently implemented with CDP. When it is implemented with BiDi, AOT will be supported")]
    [RequiresDynamicCode("NetworkManager is currently implemented with CDP. When it is implemented with BiDi, AOT will be supported.")]
    public async Task StartMonitoringAsync()
    {
        this.session.Value.Domains.Network.RequestPaused += OnRequestPausedAsync;
        this.session.Value.Domains.Network.AuthRequired += OnAuthRequiredAsync;
        this.session.Value.Domains.Network.ResponsePaused += OnResponsePausedAsync;
        await this.session.Value.Domains.Network.EnableFetchForAllPatternsAsync().ConfigureAwait(false);
        await this.session.Value.Domains.Network.EnableNetworkAsync().ConfigureAwait(false);
        await this.session.Value.Domains.Network.DisableNetworkCachingAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously stops monitoring for network traffic.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [RequiresUnreferencedCode("Network monitoring is currently implemented with CDP. When it is implemented with BiDi, AOT will be supported")]
    [RequiresDynamicCode("Network monitoring is currently implemented with CDP. When it is implemented with BiDi, AOT will be supported.")]
    public async Task StopMonitoringAsync()
    {
        this.session.Value.Domains.Network.ResponsePaused -= OnResponsePausedAsync;
        this.session.Value.Domains.Network.AuthRequired -= OnAuthRequiredAsync;
        this.session.Value.Domains.Network.RequestPaused -= OnRequestPausedAsync;
        await this.session.Value.Domains.Network.EnableNetworkCachingAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a <see cref="NetworkRequestHandler"/> to examine incoming network requests,
    /// and optionally modify the request or provide a response.
    /// </summary>
    /// <param name="handler">The <see cref="NetworkRequestHandler"/> to add.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="handler"/> is <see langword="null"/>.</exception>
    public void AddRequestHandler(NetworkRequestHandler handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler), "Request handler cannot be null");
        }

        if (handler.RequestMatcher == null)
        {
            throw new ArgumentException("Matcher for request cannot be null", nameof(handler));
        }

        if (handler.RequestTransformer == null && handler.ResponseSupplier == null)
        {
            throw new ArgumentException("Request transformer and response supplier cannot both be null", nameof(handler));
        }

        this.requestHandlers.Add(handler);
    }

    /// <summary>
    /// Clears all added <see cref="NetworkRequestHandler"/> instances.
    /// </summary>
    public void ClearRequestHandlers()
    {
        this.requestHandlers.Clear();
    }

    /// <summary>
    /// Adds a <see cref="NetworkAuthenticationHandler"/> to supply authentication
    /// credentials for network requests.
    /// </summary>
    /// <param name="handler">The <see cref="NetworkAuthenticationHandler"/> to add.</param>
    public void AddAuthenticationHandler(NetworkAuthenticationHandler handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler), "Authentication handler cannot be null");
        }

        if (handler.UriMatcher == null)
        {
            throw new ArgumentException("Matcher for delegate for URL cannot be null", nameof(handler));
        }

        if (handler.Credentials == null)
        {
            throw new ArgumentException("Credentials to use for authentication cannot be null", nameof(handler));
        }

        if (handler.Credentials is not PasswordCredentials)
        {
            throw new ArgumentException("Credentials must contain user name and password (PasswordCredentials)", nameof(handler));
        }

        this.authenticationHandlers.Add(handler);
    }

    /// <summary>
    /// Clears all added <see cref="NetworkAuthenticationHandler"/> instances.
    /// </summary>
    public void ClearAuthenticationHandlers()
    {
        this.authenticationHandlers.Clear();
    }

    /// <summary>
    /// Adds a <see cref="NetworkResponseHandler"/> to examine received network responses,
    /// and optionally modify the response.
    /// </summary>
    /// <param name="handler">The <see cref="NetworkResponseHandler"/> to add.</param>
    public void AddResponseHandler(NetworkResponseHandler handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler), "Request handler cannot be null");
        }

        if (handler.ResponseMatcher == null)
        {
            throw new ArgumentException("Matcher for response cannot be null", nameof(handler));
        }

        this.responseHandlers.Add(handler);
    }

    /// <summary>
    /// Clears all added <see cref="NetworkResponseHandler"/> instances.
    /// </summary>
    public void ClearResponseHandlers()
    {
        this.responseHandlers.Clear();
    }

    private async Task OnAuthRequiredAsync(object sender, AuthRequiredEventArgs e)
    {
        string requestId = e.RequestId;
        Uri uri = new Uri(e.Uri);
        bool successfullyAuthenticated = false;
        foreach (var authenticationHandler in this.authenticationHandlers)
        {
            if (authenticationHandler.UriMatcher!.Invoke(uri))
            {
                PasswordCredentials credentials = (PasswordCredentials)authenticationHandler.Credentials!;
                await this.session.Value.Domains.Network.ContinueWithAuthAsync(e.RequestId, credentials.UserName, credentials.Password).ConfigureAwait(false);
                successfullyAuthenticated = true;
                break;
            }
        }

        if (!successfullyAuthenticated)
        {
            await this.session.Value.Domains.Network.CancelAuthAsync(e.RequestId).ConfigureAwait(false);
        }
    }

    private async Task OnRequestPausedAsync(object sender, RequestPausedEventArgs e)
    {
        if (this.NetworkRequestSent != null)
        {
            this.NetworkRequestSent(this, new NetworkRequestSentEventArgs(e.RequestData));
        }

        foreach (var handler in this.requestHandlers)
        {
            if (handler.RequestMatcher!.Invoke(e.RequestData))
            {
                if (handler.RequestTransformer != null)
                {
                    await this.session.Value.Domains.Network.ContinueRequestAsync(handler.RequestTransformer(e.RequestData)).ConfigureAwait(false);
                    return;
                }

                if (handler.ResponseSupplier != null)
                {
                    await this.session.Value.Domains.Network.ContinueRequestWithResponseAsync(e.RequestData, handler.ResponseSupplier(e.RequestData)).ConfigureAwait(false);
                    return;
                }
            }
        }

        await this.session.Value.Domains.Network.ContinueRequestWithoutModificationAsync(e.RequestData).ConfigureAwait(false);
    }

    private async Task OnResponsePausedAsync(object sender, ResponsePausedEventArgs e)
    {
        if (e.ResponseData.Headers.Count > 0)
        {
            // If no headers are present, the body cannot be retrieved.
            await this.session.Value.Domains.Network.AddResponseBodyAsync(e.ResponseData).ConfigureAwait(false);
        }

        if (this.NetworkResponseReceived != null)
        {
            this.NetworkResponseReceived(this, new NetworkResponseReceivedEventArgs(e.ResponseData));
        }

        foreach (var handler in this.responseHandlers)
        {
            if (handler.ResponseMatcher!.Invoke(e.ResponseData))
            {
                // NOTE: We create a dummy HttpRequestData object here, because the ContinueRequestWithResponse
                // method demands one; however, the only property used by that method is the RequestId property.
                // It might be better to refactor that method signature to simply pass the request ID, or
                // alternatively, just pass the response data, which should also contain the request ID anyway.
                HttpRequestData requestData = new HttpRequestData { RequestId = e.ResponseData.RequestId };
                await this.session.Value.Domains.Network.ContinueRequestWithResponseAsync(requestData, handler.ResponseTransformer!(e.ResponseData)).ConfigureAwait(false);
                return;
            }
        }

        await this.session.Value.Domains.Network.ContinueResponseWithoutModificationAsync(e.ResponseData).ConfigureAwait(false);
    }
}
