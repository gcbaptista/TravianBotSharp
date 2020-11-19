﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TbsCore.Helpers;
using TravBotSharp.Files.Helpers;
using TravBotSharp.Files.Tasks.LowLevel;

namespace TravBotSharp.Files.Models.AccModels
{
    public class WebBrowserInfo
    {
        const int maxLogCnt = 1000;
        public WebBrowserInfo()
        {
            Logs = new List<string>();
        }
        
        public ChromeDriver Driver { get; set; }
        public string CurrentUrl => this.Driver.Url;
        private Account acc;
        public HtmlAgilityPack.HtmlDocument Html { get; set; }

        // Account Logs
        public List<string> Logs { get; set; }
        public event EventHandler LogHandler;
        public void Log(string message, Exception e) => 
                    Log(message + $"\nStack Trace:\n{e.StackTrace}\n\nMessage:" + e.Message + "\n------------------------\n");
        public void Log(string msg)
        {
            msg = DateTime.Now.ToString("HH:mm:ss") + ": " + msg;
            Logs.Insert(0, msg);

            LogHandler?.Invoke(typeof(WebBrowserInfo), new LogEventArgs() { Log = msg });
            
            if(maxLogCnt < Logs.Count)
            {
                Logs.RemoveRange(maxLogCnt, Logs.Count - 1);
            }
        }
        public class LogEventArgs : EventArgs
        {
            public string Log { get; set; }
        }

        public async Task InitSelenium(Account acc, bool newAccess = true)
        {
            this.acc = acc;
            Access access = newAccess ? await acc.Access.GetNewAccess() : acc.Access.GetCurrentAccess();

            SetupChromeDriver(access, acc.AccInfo.Nickname, acc.AccInfo.ServerUrl);

            if(this.Html == null)
            {
                this.Html = new HtmlAgilityPack.HtmlDocument();
            }

            if (!string.IsNullOrEmpty(access.Proxy))
            {
                var checkproxy = new CheckProxy();
                await checkproxy.Execute(acc);
            }
        }

        private void SetupChromeDriver(Access access, string username, string server)
        {
            ChromeOptions options = new ChromeOptions();

            // Turn on logging preferences for buildings localization (string).
            //var loggingPreferences = new OpenQA.Selenium.Chromium.ChromiumPerformanceLoggingPreferences();
            //loggingPreferences.IsCollectingNetworkEvents = true;
            //options.PerformanceLoggingPreferences = loggingPreferences;
            //options.SetLoggingPreference("performance", LogLevel.All);

            if (!string.IsNullOrEmpty(access.Proxy))
            {
                if (!string.IsNullOrEmpty(access.ProxyUsername))
                {
                    // Add proxy authentication
                    var proxyAuth = new ProxyAuthentication();
                    var extensionPath = proxyAuth.CreateExtension(username, server, access);
                    options.AddExtension(extensionPath);
                }

                options.AddArgument($"--proxy-server={access.Proxy}:{access.ProxyPort}");
                options.AddArgument("ignore-certificate-errors");
            }
            if (!string.IsNullOrEmpty(access.UserAgent))
            {
                options.AddArgument("--user-agent=" + access.UserAgent);
            }

            // Make browser headless to preserve memory resources
            if(acc.Settings.HeadlessMode) options.AddArguments("headless");

            // Do not download images in order to preserve memory resources
            if (acc.Settings.DisableImages) options.AddArguments("--blink-settings=imagesEnabled=false");

            // Add browser caching
            var dir = IoHelperCore.GetCacheDir(username, server, access);
            Directory.CreateDirectory(dir);
            options.AddArguments("user-data-dir=" + dir);

            // Disable message "Chrome is being controlled by automated test software"
            //options.AddArgument("--excludeSwitches=enable-automation"); // Doesn't work anymore

            // Hide command prompt
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            try
            {
                this.Driver = new ChromeDriver(service, options);

                // Set timeout
                this.Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
            }
            catch(Exception e)
            {
                Log($"Error opening chrome driver! Is it already opened?", e);
            }
        }

        internal Dictionary<string, string> GetCookies()
        {
            var cookies = Driver.Manage().Cookies.AllCookies;
            var cookiesDir = new Dictionary<string, string>();
            for (int i = 0; i < cookies.Count; i++)
            {
                cookiesDir.Add(cookies[i].Name, cookies[i].Value);
            }
            return cookiesDir;
        }

        public async Task Navigate(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            int repeatCnt = 0;
            bool repeat;
            do
            {
                try
                {
                    // Will throw exception after timeout
                    this.Driver.Navigate().GoToUrl(url);
                    repeat = false;
                }
                catch (Exception e)
                {
                    acc.Wb.Log($"Error sending http request to {url}", e);
                    repeat = true;
                    if (++repeatCnt >= 5 && !string.IsNullOrEmpty(acc.Access.GetCurrentAccess().Proxy))
                    {
                        // Change access
                        repeatCnt = 0;
                        var changeAccess = new ChangeAccess();
                        await changeAccess.Execute(acc);
                        await Task.Delay(AccountHelper.Delay() * 5);
                    }
                    await Task.Delay(AccountHelper.Delay());
                }
            }
            while(repeat);


            await Task.Delay(AccountHelper.Delay());
            //if (!string.IsNullOrEmpty(acc.Access.GetCurrentAccess().Proxy))
            //{
            //    // We are using proxy. Connection is probably slower -> additional delay.
            //    await Task.Delay(AccountHelper.Delay() * 2);
            //}

            this.Html.LoadHtml(this.Driver.PageSource);
            await TaskExecutor.PageLoaded(acc);
        }

        public void Close()
        {
            try
            {
                this.Driver.Quit();
            }
            catch(Exception e)
            {
                Log($"Error closing chrome driver", e);
            }
        }
    }
}