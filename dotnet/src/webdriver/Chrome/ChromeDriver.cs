// <copyright file="ChromeDriver.cs" company="Selenium Committers">
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
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Remote;

namespace OpenQA.Selenium.Chrome;

/// <summary>
/// Provides a mechanism to write tests against Chrome
/// </summary>
/// <example>
/// <code>
/// [TestFixture]
/// public class Testing
/// {
///     private IWebDriver driver;
///     <para></para>
///     [SetUp]
///     public void SetUp()
///     {
///         driver = new ChromeDriver();
///     }
///     <para></para>
///     [Test]
///     public void TestGoogle()
///     {
///         driver.Navigate().GoToUrl("http://www.google.co.uk");
///         /*
///         *   Rest of the test
///         */
///     }
///     <para></para>
///     [TearDown]
///     public void TearDown()
///     {
///         driver.Quit();
///     }
/// }
/// </code>
/// </example>
public class ChromeDriver : ChromiumDriver
{
    private static readonly Dictionary<string, CommandInfo> chromeCustomCommands = new Dictionary<string, CommandInfo>()
    {
        { ExecuteCdp, new HttpCommandInfo(HttpCommandInfo.PostCommand, "/session/{sessionId}/goog/cdp/execute") },
        { GetCastSinksCommand, new HttpCommandInfo(HttpCommandInfo.GetCommand, "/session/{sessionId}/goog/cast/get_sinks") },
        { SelectCastSinkCommand, new HttpCommandInfo(HttpCommandInfo.PostCommand, "/session/{sessionId}/goog/cast/set_sink_to_use") },
        { StartCastTabMirroringCommand, new HttpCommandInfo(HttpCommandInfo.PostCommand, "/session/{sessionId}/goog/cast/start_tab_mirroring") },
        { StartCastDesktopMirroringCommand, new HttpCommandInfo(HttpCommandInfo.PostCommand, "/session/{sessionId}/goog/cast/start_desktop_mirroring") },
        { GetCastIssueMessageCommand, new HttpCommandInfo(HttpCommandInfo.GetCommand, "/session/{sessionId}/goog/cast/get_issue_message") },
        { StopCastingCommand, new HttpCommandInfo(HttpCommandInfo.PostCommand, "/session/{sessionId}/goog/cast/stop_casting") }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriver"/> class.
    /// </summary>
    public ChromeDriver()
        : this(new ChromeOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriver"/> class using the specified options.
    /// </summary>
    /// <param name="options">The <see cref="ChromeOptions"/> to be used with the Chrome driver.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="options"/> is <see langword="null"/>.</exception>
    public ChromeDriver(ChromeOptions options)
        : this(ChromeDriverService.CreateDefaultService(), options, RemoteWebDriver.DefaultCommandTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriver"/> class using the specified driver service.
    /// </summary>
    /// <param name="service">The <see cref="ChromeDriverService"/> used to initialize the driver.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <see langword="null"/>.</exception>
    public ChromeDriver(ChromeDriverService service)
        : this(service, new ChromeOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriver"/> class using the specified path
    /// to the directory containing ChromeDriver.exe.
    /// </summary>
    /// <param name="chromeDriverDirectory">The full path to the directory containing ChromeDriver.exe.</param>
    public ChromeDriver(string chromeDriverDirectory)
        : this(chromeDriverDirectory, new ChromeOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriver"/> class using the specified path
    /// to the directory containing ChromeDriver.exe and options.
    /// </summary>
    /// <param name="chromeDriverDirectory">The full path to the directory containing ChromeDriver.exe.</param>
    /// <param name="options">The <see cref="ChromeOptions"/> to be used with the Chrome driver.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="options"/> is <see langword="null"/>.</exception>
    public ChromeDriver(string chromeDriverDirectory, ChromeOptions options)
        : this(chromeDriverDirectory, options, RemoteWebDriver.DefaultCommandTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriver"/> class using the specified path
    /// to the directory containing ChromeDriver.exe, options, and command timeout.
    /// </summary>
    /// <param name="chromeDriverDirectory">The full path to the directory containing ChromeDriver.exe.</param>
    /// <param name="options">The <see cref="ChromeOptions"/> to be used with the Chrome driver.</param>
    /// <param name="commandTimeout">The maximum amount of time to wait for each command.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="options"/> is <see langword="null"/>.</exception>
    public ChromeDriver(string chromeDriverDirectory, ChromeOptions options, TimeSpan commandTimeout)
        : this(ChromeDriverService.CreateDefaultService(chromeDriverDirectory), options, commandTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriver"/> class using the specified
    /// <see cref="ChromeDriverService"/> and options.
    /// </summary>
    /// <param name="service">The <see cref="ChromeDriverService"/> to use.</param>
    /// <param name="options">The <see cref="ChromeOptions"/> used to initialize the driver.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="service"/> or <paramref name="options"/> are <see langword="null"/>.</exception>
    public ChromeDriver(ChromeDriverService service, ChromeOptions options)
        : this(service, options, RemoteWebDriver.DefaultCommandTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriver"/> class using the specified <see cref="ChromeDriverService"/>.
    /// </summary>
    /// <param name="service">The <see cref="ChromeDriverService"/> to use.</param>
    /// <param name="options">The <see cref="ChromeOptions"/> to be used with the Chrome driver.</param>
    /// <param name="commandTimeout">The maximum amount of time to wait for each command.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="service"/> or <paramref name="options"/> are <see langword="null"/>.</exception>
    public ChromeDriver(ChromeDriverService service, ChromeOptions options, TimeSpan commandTimeout)
        : base(service, options, commandTimeout)
    {
        this.AddCustomChromeCommands();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeDriver"/> class using the specified executor, skipping session initialization.
    /// </summary>
    /// <param name="executor">The <see cref="ICommandExecutor"/> object used to execute commands.</param>
    /// <param name="options">The <see cref="ChromeOptions"/> to be used with the Chrome driver.</param>
    /// <param name="skipSessionStart">Must be <see langword="true"/> to skip session initialization.</param>
    private ChromeDriver(ICommandExecutor executor, ChromeOptions options, bool skipSessionStart)
        : base(executor, options, skipSessionStart)
    {
    }

    /// <summary>
    /// Asynchronously creates and initializes a new instance of the <see cref="ChromeDriver"/> class with the specified options.
    /// </summary>
    /// <param name="options">The <see cref="ChromeOptions"/> to be used with the Chrome driver.</param>
    /// <param name="commandTimeout">The maximum amount of time to wait for each command. If not specified, uses the default timeout.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the initialized <see cref="ChromeDriver"/> instance.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="options"/> is <see langword="null"/>.</exception>
    public static async Task<ChromeDriver> StartAsync(ChromeOptions options, TimeSpan commandTimeout = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        ChromeDriverService service = ChromeDriverService.CreateDefaultService();
        TimeSpan timeout = commandTimeout == default ? RemoteWebDriver.DefaultCommandTimeout : commandTimeout;

        ICommandExecutor executor = await GenerateDriverServiceCommandExecutorAsync(service, options, timeout).ConfigureAwait(false);

        ChromeDriver driver = new ChromeDriver(executor, options, skipSessionStart: true);

        try
        {
            await driver.StartSessionAsync(ConvertOptionsToCapabilities(options)).ConfigureAwait(false);
            driver.CompleteInitialization();
            driver.AddCustomChromeCommands();
        }
        catch (Exception)
        {
            try
            {
                await driver.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
                // Ignore the clean-up exception. We'll propagate the original failure.
            }
            throw;
        }

        return driver;
    }

    private static async Task<ICommandExecutor> GenerateDriverServiceCommandExecutorAsync(ChromeDriverService service, ChromeOptions options, TimeSpan commandTimeout)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (service.DriverServicePath == null)
        {
            DriverFinder finder = new DriverFinder(options);
            string fullServicePath = await finder.GetDriverPathAsync().ConfigureAwait(false);
            service.DriverServicePath = Path.GetDirectoryName(fullServicePath);
            service.DriverServiceExecutableName = Path.GetFileName(fullServicePath);
            string fullBrowserPath = await finder.GetBrowserPathAsync().ConfigureAwait(false);
            options.BinaryLocation = fullBrowserPath;
            options.BrowserVersion = null;
        }

        await service.StartAsync().ConfigureAwait(false);

        return new DriverServiceCommandExecutor(service, commandTimeout);
    }

    /// <summary>
    /// Gets a read-only dictionary of the custom WebDriver commands defined for ChromeDriver.
    /// The keys of the dictionary are the names assigned to the command; the values are the
    /// <see cref="CommandInfo"/> objects describing the command behavior.
    /// </summary>
    public static IReadOnlyDictionary<string, CommandInfo> CustomCommandDefinitions
    {
        get
        {
            Dictionary<string, CommandInfo> customCommands = new Dictionary<string, CommandInfo>();
            foreach (KeyValuePair<string, CommandInfo> entry in ChromiumCustomCommands)
            {
                customCommands[entry.Key] = entry.Value;
            }

            foreach (KeyValuePair<string, CommandInfo> entry in chromeCustomCommands)
            {
                customCommands[entry.Key] = entry.Value;
            }

            return new ReadOnlyDictionary<string, CommandInfo>(customCommands);
        }
    }

    private void AddCustomChromeCommands()
    {
        foreach (KeyValuePair<string, CommandInfo> entry in CustomCommandDefinitions)
        {
            this.RegisterInternalDriverCommand(entry.Key, entry.Value);
        }
    }
}
