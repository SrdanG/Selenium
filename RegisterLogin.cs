using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using NUnit.Framework;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using System.Data.SqlClient;

namespace Selenium
{
    class RegisterLogin
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
        public void RegisterAndLogin()
        {
            driver.Navigate().GoToUrl(Config.baseURL);
            driver.Manage().Window.Maximize();

            //REGISTER
            driver.FindElement(By.Id("loginbox_register")).Click();
            driver.FindElement(By.Id("RegisterFirstName")).SendKeys("Srdan");
            driver.FindElement(By.Id("RegisterLastName")).SendKeys("Grebensek");
            driver.FindElement(By.Id("RegisterEmail")).SendKeys("test.1nadan@gmail.com");
            driver.FindElement(By.Id("email_repeat")).SendKeys("test.1nadan@gmail.com");
            driver.FindElement(By.Id("RegisterPass")).SendKeys("xyz");
            driver.FindElement(By.Id("PageLogon_PasswordRepeat")).SendKeys("xyz");
            driver.FindElement(By.Id("phoneRegister")).SendKeys("031111111");

            //select dropdown list
            var city = driver.FindElement(By.Id("RegisterCityName"));
            var selectCity = new SelectElement(city);
            selectCity.SelectByValue("Celje");

            driver.FindElement(By.Id("RegisterConditions")).Click();

            /* FIND ELEMNT BY XPATH -> Chrome: Right click on the node => "Copy XPath" (https://www.guru99.com/xpath-selenium.html) */
            driver.FindElement(By.XPath("//*[@id='registerform']/div[31]/div/input")).Click();

            //Web driver wait - REFERENCE: https://www.guru99.com/implicit-explicit-waits-selenium.html
            WebDriverWait wait1 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait1.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[11]/div[3]/button/span"))).Click();

            //check if registered email are saved correctly in DB
            using (var conn = new SqlConnection(Config.connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("select top 1 *  from Account order by AccountId desc", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        string EmailDB = string.Empty;
                        while (reader.Read())
                        {
                            EmailDB = reader.GetString(1);
                        }
                        Assert.AreEqual(EmailDB, "test.1nadan@gmail.com");
                    }
                }
                conn.Close();
            }


            //LOGIN
            driver.FindElement(By.Id("loginbox_login")).Click();
            driver.FindElement(By.Id("LoginEmail")).SendKeys("test.1nadan@gmail.com");
            driver.FindElement(By.Id("LoginPass")).SendKeys("xyz");
            driver.FindElement(By.XPath("//*[@id='loginform']/div[14]/div/input")).Click();

            WebDriverWait wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait2.Until(ExpectedConditions.ElementIsVisible(By.LinkText("Moj profil"))).Click();

            //check if mail used at registration is equal as mail in myProfile
            driver.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div[1]/h1/a")).Click();
            string profileEmail = driver.FindElement(By.Id("Account_Email")).GetAttribute("value");
            Assert.AreEqual(profileEmail, "test.1nadan@gmail.com");

            // Check if Newsletter is checked correct based on selection ("Celje") at registration
            driver.FindElement(By.LinkText("Moje email preference")).Click();
            bool EmailPreferenceCelje = driver.FindElement(By.Id("newsletter3")).Selected;
            bool EmailPreferenceIzdelki = driver.FindElement(By.Id("newsletter9")).Selected;
            bool EmailPreferenceLjubljana = driver.FindElement(By.Id("newsletter13")).Selected;

            //Change Email preference -> change and check if changes saved correctly
            driver.FindElement(By.Id("newsletter3")).Click();
            driver.FindElement(By.Id("newsletter9")).Click();
            driver.FindElement(By.Id("newsletter1")).Click();
            driver.FindElement(By.Id("newsletter5")).Click();

            driver.FindElement(By.XPath("//*[@id='submit']")).Click();
            driver.FindElement(By.LinkText("Nazaj na urejanje")).Click();

            IWebElement EmailPreferenceCeljeUnChecked = driver.FindElement(By.Id("newsletter3"));
            IWebElement EmailPreferencePotovanjaUnChecked = driver.FindElement(By.Id("newsletter9"));
            IWebElement EmailPreferenceLjubljanaChecked = driver.FindElement(By.Id("newsletter1"));
            IWebElement EmailPreferenceObalaChecked = driver.FindElement(By.Id("newsletter5"));

            if (EmailPreferenceCeljeUnChecked.Selected || EmailPreferencePotovanjaUnChecked.Selected || !EmailPreferenceLjubljanaChecked.Selected || !EmailPreferenceObalaChecked.Selected)
                {
                    Assert.Fail("Test change Email preference failed");
                }

            //Check if Email preference changes saved correctly in DB
            using (var conn = new SqlConnection(Config.connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("select " + "City.CityName " +
                    "from NewsletterCitySubscription " +
                    "LEFT JOIN NewsletterEmail on NewsletterCitySubscription.EmailId = NewsletterEmail.Id " +
                    "LEFT JOIN City on NewsletterCitySubscription.CityId = City.CityId " +
                    "where NewsletterEmail.Email = @email " +
                    "order by City.CityName",
                    conn))
                {
                    cmd.Parameters.Add(new SqlParameter("email", "test.1nadan@gmail.com"));

                    using (var reader = cmd.ExecuteReader())
                    {
                        string CityIzdelki = string.Empty;
                        string CityLjubljana = string.Empty;
                        string CityObala = string.Empty;
                        var count = 0;
                        while (reader.Read())
                        {
                            var City = reader.GetString(0);

                            switch (count)
                            {
                                case 0:
                                    CityIzdelki = City;
                                    break;
                                case 1:
                                    CityLjubljana = City;
                                    break;
                                default:
                                    CityObala = City;
                                    break;

                            }
                            count++;
                        }

                        if (CityIzdelki != "Izdelki" || CityLjubljana != "Ljubljana" || CityObala != "Obala")
                        {
                            Assert.Fail("Newsletter in DB are not updated correctly. Is not the same as on myProfile fronend");
                        }   
                    }
                }
                conn.Close();
            }

            //Test reset password functionality
            driver.FindElement(By.XPath("/html/body/div[3]/div/div[1]/div[1]/ul/li[11]/a")).Click();
            driver.FindElement(By.XPath("//*[@id='StaroGeslo']")).SendKeys("test123");
            driver.FindElement(By.XPath("//*[@id='NovoGeslo']")).SendKeys("123test");
            driver.FindElement(By.XPath("//*[@id='NovoGesloRepeat']")).SendKeys("123test");
            driver.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div/form/div/div[11]/input")).Click();

            WebDriverWait waitSuccessReset = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            string successReset = waitSuccessReset.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[3]/div/div[2]/div/h1"))).Text;
            Assert.AreEqual("Geslo je bilo uspešno spremenjeno.", successReset);

            driver.FindElement(By.Id("logoutlink")).Click();

            WebDriverWait loginResetPass = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            loginResetPass.Until(ExpectedConditions.ElementIsVisible(By.Id("loginbox_login"))).Click();
            driver.FindElement(By.Id("LoginEmail")).SendKeys("test.1nadan@gmail.com");
            driver.FindElement(By.Id("LoginPass")).SendKeys("123test");
            driver.FindElement(By.XPath("//*[@id='loginform']/div[14]/div/input")).Click();

            WebDriverWait waitSuccessLoginNewPass = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            string successLoginNewPass = waitSuccessLoginNewPass.Until(ExpectedConditions.ElementIsVisible(By.ClassName("my_profile-user"))).Text;

            if (successLoginNewPass != "Srdan Grebensek")
            {
                Assert.Fail("Login with new reseted password failed");
            }

            //LOGOUT
            driver.FindElement(By.Id("logoutlink")).Click();

        }

        [TearDown]
        public void EndTest()
        {
            driver.Close();
        }
    }
}
