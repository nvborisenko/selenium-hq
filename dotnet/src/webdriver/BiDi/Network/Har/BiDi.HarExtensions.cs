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
