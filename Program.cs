using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;


namespace WebDriverDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            IWebDriver driver = new ChromeDriver(@"C:\SeleniumDrivers\");
            driver.Url = "http://google.com";

            //IMPLICIT WAIT -> wait for some specific time, 10 second in example below
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            var searcBox = driver.FindElement(By.Name("q"));
            searcBox.SendKeys("Pluralsight");
            searcBox.Submit();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            var imagesLink = driver.FindElement(By.LinkText("Slike"));
            imagesLink.Click();

            //Find and click first element in ul list to click first image
            var ul = driver.FindElement(By.ClassName("rg_ul"));
            var firstImageLink = ul.FindElements(By.TagName("a"))[0];
            firstImageLink.Click();

            driver.Url = @"file:///D:\Tutorials\SeleniumExamples\WebDriverDemo\WebDriverDemo\TestPage.html";

            //RADIO BUTTONS
            //click second radiobutton in radio button list
            var radioButtons = driver.FindElements(By.Name("color"))[1];
            radioButtons.Click();

            //check which radio button is selected
            var radioButtons = driver.FindElements(By.Name("color"));
            foreach (var RadioButton in radioButtons)
            {
                if (RadioButton.Selected)
                    Console.WriteLine(RadioButton.GetAttribute("value"));
            }

            //CHECKBOXES
            var checkBox = driver.FindElement(By.Id("check1"));
            checkBox.Click();

            //DROPDOWN
            //One (hard) way to select from dropdown
            var select = driver.FindElement(By.Id("select1"));
            var tomOption = select.FindElements(By.TagName("option"))[2];
            tomOption.Click();

            //Easy way to select from dropdown -> INSTALL NuGet Packages -> Selenium.support clases
            var select = driver.FindElement(By.Id("select1"));

            var selectElement = new SelectElement(select);
            selectElement.SelectByText("Frank");

            //TABLES
            var outerTable = driver.FindElement(By.TagName("table"));
            var innerTable = outerTable.FindElement(By.TagName("table"));
            var row = innerTable.FindElements(By.TagName("td"))[1];
            Console.WriteLine(row.Text);

            //XPATH
            var rowByXpath = driver.FindElement(By.XPath("/html/body/table/tbody/tr/td[2]/table/tbody/tr[2]/td"));
            Console.WriteLine(rowByXpath.Text);


            //EXPLICIT WAIT -> wait until some element shows up, but not longer then 10 seconds
            driver.Url = "http://google.com";

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var searchBox = wait.Until(d => driver.FindElement(By.Name("q")));
            //OR like using in 1nadan projects
            //var searchBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("q")));

            searchBox.SendKeys("Pluralsight");
            searchBox.Submit();

        }
    }
}
