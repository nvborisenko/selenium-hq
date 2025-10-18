// <copyright file="HarCaptureTest.cs" company="Selenium Committers">
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

using NUnit.Framework;
using OpenQA.Selenium.BiDi.BrowsingContext;
using OpenQA.Selenium.BiDi.Network.Har;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.Network;

class HarCaptureTest : BiDiTestFixture
{
    [Test]
    public async Task CanCaptureNetworkTrafficToHar()
    {
        await using var recorder = await bidi.CaptureNetworkTrafficAsync(new HarCaptureOptions
        {
            BrowserName = "TestBrowser",
            BrowserVersion = "1.0"
        });

        await context.NavigateAsync(UrlBuilder.WhereIs("bidi/logEntryAdded.html"), new() { Wait = ReadinessState.Complete });

        var har = recorder.GetHar();

        Assert.That(har, Is.Not.Null);
        Assert.That(har.Log, Is.Not.Null);
        Assert.That(har.Log.Version, Is.EqualTo("1.2"));
        Assert.That(har.Log.Creator.Name, Is.EqualTo("Selenium"));
        Assert.That(har.Log.Browser, Is.Not.Null);
        Assert.That(har.Log.Browser.Name, Is.EqualTo("TestBrowser"));
        Assert.That(har.Log.Browser.Version, Is.EqualTo("1.0"));
        Assert.That(har.Log.Entries, Is.Not.Empty);

        var entry = har.Log.Entries.FirstOrDefault(e => e.Request.Url.Contains("logEntryAdded.html"));
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry.Request.Method, Is.EqualTo("GET"));
        Assert.That(entry.Response.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task CanSaveHarToFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"selenium-har-{Guid.NewGuid()}.har");

        try
        {
            await using var recorder = await bidi.CaptureNetworkTrafficAsync();

            await context.NavigateAsync(UrlBuilder.WhereIs("bidi/logEntryAdded.html"), new() { Wait = ReadinessState.Complete });

            await recorder.SaveAsync(tempFile);

            Assert.That(File.Exists(tempFile), Is.True);

            var jsonContent = await File.ReadAllTextAsync(tempFile);
            Assert.That(jsonContent, Is.Not.Empty);

            var harFile = JsonSerializer.Deserialize<HarFile>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.That(harFile, Is.Not.Null);
            Assert.That(harFile.Log, Is.Not.Null);
            Assert.That(harFile.Log.Entries, Is.Not.Empty);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Test]
    public async Task HarEntriesContainRequestDetails()
    {
        await using var recorder = await bidi.CaptureNetworkTrafficAsync();

        await context.NavigateAsync(UrlBuilder.WhereIs("bidi/logEntryAdded.html"), new() { Wait = ReadinessState.Complete });

        var har = recorder.GetHar();
        var entry = har.Log.Entries.FirstOrDefault(e => e.Request.Url.Contains("logEntryAdded.html"));

        Assert.That(entry, Is.Not.Null);
        Assert.That(entry.StartedDateTime, Is.Not.Empty);
        Assert.That(entry.Time, Is.GreaterThanOrEqualTo(0));
        Assert.That(entry.Request.Headers, Is.Not.Empty);
        Assert.That(entry.Response.Headers, Is.Not.Empty);
        Assert.That(entry.Timings, Is.Not.Null);
    }
}
