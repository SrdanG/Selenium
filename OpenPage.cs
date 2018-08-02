using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.Extensions;
using NUnit.Framework;
using System.Data.SqlClient;


namespace Selenium
{
    class OpenPage
    {
        IWebDriver driver;

        [SetUp]
        public void Initialize()
        {
            switch (Config.browser)
            {
                case "Chrome":
                    driver = new ChromeDriver(@"C:\SeleniumDrivers");
                    break;
                case "IE":
                    driver = new EdgeDriver(@"C:\SeleniumDrivers");
                    break;
                case "Firefox":
                    FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(@"C:\SeleniumDrivers");
                    service.FirefoxBinaryPath = @"C:\Program Files\Mozilla Firefox\firefox.exe";
                    driver = new FirefoxDriver(service);
                    break;
            }
        }

        [Test]
        public void OpenSite()
        {
            driver.Navigate().GoToUrl(Config.baseURL);
            driver.Manage().Window.Maximize();

            string TitleActual = driver.Title;
            Console.WriteLine("Actual title: " + TitleActual);
            string TitleExpected = "Najboljši kuponi v mestu :: 1nadan.si";
            Assert.AreEqual(TitleExpected, TitleActual);           

            int TitleLengthActual = driver.Title.Length;
            Console.WriteLine("Title length: " + TitleLengthActual);

            string UrlActual = driver.Url;
            Console.WriteLine("Actual Page URL: " + UrlActual);
            string UrlExpected = "https://end-test.1nadan.si/ponudbe-vse";
            Assert.AreEqual(UrlExpected, UrlActual);

            int UrlLength = driver.Url.Length;
            Console.WriteLine("Page URL Length: " + UrlLength);

        }

        [TearDown]
        public void EndTest()
        {
            driver.Close();
        }
    }



}
