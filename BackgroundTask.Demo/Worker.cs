using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Selenium.WebDriver.UndetectedChromeDriver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundTask.Demo
{
    public class Worker : IWorker
    {
        private IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<Worker> logger;
        private int number = 0;

        public Worker(ILogger<Worker> logger, IWebHostEnvironment webHostEnvironment)
        {
            this.logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Interlocked.Increment(ref number);
                logger.LogInformation($"Worker printing number: {number}");

                try
                {
                    string path = Path.Combine(_webHostEnvironment.WebRootPath, "4.1.0_0.crx");
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddExtensions(path);

                    using (var driver = UndetectedChromeDriver.Instance("profile_name_ex", chromeOptions))
                    {
                        var extension_Protocol = "chrome-extension";
                        var extension_ID = "bihmplhobchoageeokmgbdihknkjbknd";

                        //Truy cập vào index page thay vì mở click vào extension để mở popup, index page sẽ có dạng extension protocol + extension ID + /panel/index.html
                        var indexPage = extension_Protocol + "://" + extension_ID + "/panel/index.html";

                        //Truy cập vào index page vừa khai báo phía trên
                        driver.Navigate().GoToUrl(indexPage);

                        //Vì khi sử dụng extension, extension sẽ tự mở một page của extension nên chúng ta cần switch lại tab đầu tiên (tab index page)
                        //Khai báo các tabs đang mở
                        var tabs = new List<string>(driver.WindowHandles);
                        //Switch về tab đầu tiên
                        driver.SwitchTo().Window(tabs[0]);

                        Thread.Sleep(3000);

                        //Click vào nút connect
                        driver.FindElement(By.Id("ConnectionButton")).Click();

                        Thread.Sleep(3000);

                        driver.Navigate().GoToUrl("https://phim18.live");

                        Thread.Sleep(3000);

                        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        var quickView = wait.Until(drv => drv.FindElement(By.XPath("//a[contains(text(), 'Xem nhanh')]")));
                        quickView.Click();

                        Thread.Sleep(5000);

                        var button = wait.Until(drv => drv.FindElement(By.Id("video-content")));
                        button.Click();

                        Thread.Sleep(3000);

                        //Switch về tab đầu tiên
                        driver.SwitchTo().Window(tabs[0]);

                        button.Click();

                        Thread.Sleep(3000);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }

                var r = new Random();
                int rInt = r.Next(60000, 180000);

                await Task.Delay(rInt, cancellationToken);
            }
        }
    }
}