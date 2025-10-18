// <copyright file="HarLog.cs" company="Selenium Committers">
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
/// Represents the root object of exported HAR data.
/// </summary>
public sealed class HarFile
{
    /// <summary>
    /// Gets or sets the log object.
    /// </summary>
    public HarLog Log { get; set; } = new HarLog();
}

/// <summary>
/// Represents a HAR log object.
/// </summary>
public sealed class HarLog
{
    /// <summary>
    /// Gets or sets the version of the HAR format.
    /// </summary>
    public string Version { get; set; } = "1.2";

    /// <summary>
    /// Gets or sets the creator information.
    /// </summary>
    public HarCreator Creator { get; set; } = new HarCreator();

    /// <summary>
    /// Gets or sets the browser information.
    /// </summary>
    public HarBrowser? Browser { get; set; }

    /// <summary>
    /// Gets or sets the list of page objects.
    /// </summary>
    public List<HarPage> Pages { get; set; } = new List<HarPage>();

    /// <summary>
    /// Gets or sets the list of entry objects.
    /// </summary>
    public List<HarEntry> Entries { get; set; } = new List<HarEntry>();

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents the creator information.
/// </summary>
public sealed class HarCreator
{
    /// <summary>
    /// Gets or sets the name of the creator.
    /// </summary>
    public string Name { get; set; } = "Selenium";

    /// <summary>
    /// Gets or sets the version of the creator.
    /// </summary>
    public string Version { get; set; } = "4.0";

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents the browser information.
/// </summary>
public sealed class HarBrowser
{
    /// <summary>
    /// Gets or sets the name of the browser.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the browser.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents a page object.
/// </summary>
public sealed class HarPage
{
    /// <summary>
    /// Gets or sets the unique identifier for the page.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time stamp for the page.
    /// </summary>
    public string StartedDateTime { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the page timings.
    /// </summary>
    public HarPageTimings PageTimings { get; set; } = new HarPageTimings();

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents page timing information.
/// </summary>
public sealed class HarPageTimings
{
    /// <summary>
    /// Gets or sets the time in milliseconds for the page to load.
    /// </summary>
    public double OnContentLoad { get; set; } = -1;

    /// <summary>
    /// Gets or sets the time in milliseconds for the page to finish loading.
    /// </summary>
    public double OnLoad { get; set; } = -1;

    /// <summary>
    /// Gets or sets an optional comment.
    /// </summary>
    public string? Comment { get; set; }
}
