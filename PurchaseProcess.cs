using System;
using System.IO;
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
    class PurchaseProcess
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
        public void PurchaseCouponWithEwallet()
        {
            driver.Navigate().GoToUrl(Config.baseURL);
            driver.Manage().Window.Maximize();

            //Login
            driver.FindElement(By.Id("loginbox_login")).Click();
            driver.FindElement(By.Id("LoginEmail")).SendKeys("srdan.grebensek@gmail.com");
            driver.FindElement(By.Id("LoginPass")).SendKeys("xyz");
            driver.FindElement(By.XPath("//*[@id='loginform']/div[14]/div/input")).Click();

            //Buy coupon with ewallet and: check success on site and DB purchaseStatus PAYED
            //  #1 check if coupon exist on the list Veljavni kuponi
            //  #2 check if all paramater in DB are saved correctlly
            //  #3 download coupon and check if file exist on path
            //  #4 send coupon to email and archive

            WebDriverWait wait1 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait1.Until(ExpectedConditions.ElementToBeClickable(By.LinkText("Potovanja"))).Click();
            driver.FindElement(By.ClassName("offer-image")).Click();
            driver.FindElement(By.Id("buyButton28717")).Click();
            var QtyWallet = driver.FindElement(By.Id("CouponCountSelect"));
            var selectQtyEwallet = new SelectElement(QtyWallet);
            selectQtyEwallet.SelectByValue("3");
            driver.FindElement(By.Id("btnNext")).Click();
            driver.FindElement(By.Id("btnNext")).Click();
            driver.FindElement(By.Id("EWallet")).Click();
            driver.FindElement(By.Id("btnNext")).Click();

            WebDriverWait waitSuccessEwalletPurchase = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            string successEwalletPurchase = waitSuccessEwalletPurchase.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/form/div[2]/div/div[1]/h1"))).Text;
            Assert.AreEqual("Čestitamo! Naročilo je bilo uspešno oddano.", successEwalletPurchase);

            // #1
            using (var conn = new SqlConnection(Config.connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("select top 3 * from Coupons order by 1 desc", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        string FirstCouponSaffetyNumber = string.Empty;
                        string SecondCouponSaffetyNumber = string.Empty;
                        string ThirdCouponSaffetyNumber = string.Empty;
                        var count = 0;
                        while (reader.Read())
                        {
                            var SaffetyNumber = reader.GetString(8);

                            switch (count)
                            {
                                case 0:
                                    FirstCouponSaffetyNumber = SaffetyNumber;
                                    break;
                                case 1:
                                    SecondCouponSaffetyNumber = SaffetyNumber;
                                    break;
                                default:
                                    ThirdCouponSaffetyNumber = SaffetyNumber;
                                    break;
                            }
                            count++;
                        }

                        driver.FindElement(By.LinkText("Moji kuponi")).Click();

                        IWebElement bodyTag = driver.FindElement(By.TagName("body"));
                        if (bodyTag.Text.Contains(FirstCouponSaffetyNumber) && bodyTag.Text.Contains(SecondCouponSaffetyNumber) && bodyTag.Text.Contains(ThirdCouponSaffetyNumber)) { }
                        else
                        {
                            Assert.Fail("Cupons or at least one of them are not on the list Moji Kuponi");
                        }
                    }
                }
                conn.Close();
            }

            // #2 and #3 
            using (var conn = new SqlConnection(Config.connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("select top 1 * from AccountsInOffers order by 1 desc", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        int PurchaseID = 0;
                        int OfferID = 0;
                        string PurchaseStatus = string.Empty;
                        int CouponCount = 0;
                        string PaymentType = string.Empty;
                        string Email = string.Empty;
                        decimal TotalPrice = 0;

                        while (reader.Read())
                        {
                            PurchaseID = reader.GetInt32(0);
                            OfferID = reader.GetInt32(1);
                            PurchaseStatus = reader.GetString(3);
                            CouponCount = reader.GetInt32(5);
                            PaymentType = reader.GetString(7);
                            Email = reader.GetString(10);
                            TotalPrice = reader.GetDecimal(15);
                        }

                        // #2
                        Assert.AreEqual(OfferID, 28717);
                        Assert.AreEqual(PurchaseStatus, "PAYED");
                        Assert.AreEqual(CouponCount, 3);
                        Assert.AreEqual(PaymentType, "EWallet");
                        Assert.AreEqual(Email, "srdan.grebensek@gmail.com");
                        Assert.AreEqual(TotalPrice, 297.0000);

                        // #3
                        driver.FindElement(By.ClassName("icon-download")).Click();
                        WebDriverWait WaitCouponToDownload = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                        string FileName = "kupon" + PurchaseID + ".pdf";
                        string path = "C:\\Users\\srdan\\Downloads\\" + FileName;

                        if (!File.Exists(path))
                        {
                            Assert.Fail("Coupon not downloaded");
                        }
                    }
                }
                conn.Close();
            }

            // #3 and #4
            driver.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div/div[4]/div[2]/div[2]/a[2]")).Click();
            driver.FindElement(By.XPath("/html/body/div[3]/div/div[2]/div/div[4]/div[2]/div[2]/a[3]")).Click();
            driver.FindElement(By.LinkText("Arhivirani kuponi")).Click();
            WebDriverWait WaitToSeeIfArchivedSuccess = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            //Buy coupon with UPN and: check success on site and DB purchaseStatus RESERVED
            //  #1 check if UPN paymend order present on site on last purchase step  and check if values are correct (znesek, IBAN)
            //  #2 check if all paramater in DB are saved correctlly

            driver.FindElement(By.ClassName("logo")).Click();

            WebDriverWait wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait2.Until(ExpectedConditions.ElementToBeClickable(By.LinkText("Potovanja"))).Click();
            driver.FindElement(By.ClassName("offer-image")).Click();
            driver.FindElement(By.Id("buyButton28717")).Click();
            driver.FindElement(By.Id("btnNext")).Click();
            driver.FindElement(By.Id("btnNext")).Click();
            driver.FindElement(By.Id("MoneyOrder")).Click();
            driver.FindElement(By.Id("btnNext")).Click();

            // #1
            WebDriverWait waitSuccessUpnPurchase = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            string successUpnPurchase = waitSuccessUpnPurchase.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/form/div[2]/div/div[1]/h1"))).Text;
            Assert.AreEqual("Čestitamo! Naročilo je bilo uspešno oddano.", successUpnPurchase);
            string upnFormPresent = driver.FindElement(By.XPath("/html/body/form/div[2]/div/div[1]/div/div[4]/h1")).Text;

            if ( upnFormPresent == "Plačilni nalog UPN")
            {
                string totalPriceUPN = driver.FindElement(By.XPath("/html/body/form/div[2]/div/div[1]/div/div[4]/div/div[2]/table/tbody/tr[2]/td[2]/div[1]")).Text;
                string ibanUPN = driver.FindElement(By.XPath("/html/body/form/div[2]/div/div[1]/div/div[4]/div/div[2]/table/tbody/tr[2]/td[2]/div[2]")).Text;
                Assert.AreEqual("99,00 EUR", totalPriceUPN);
                Assert.AreEqual("SI56 1010 0004 8153 414", ibanUPN);
            }
            else
            {
                Assert.Fail("UPN payment order are not present on site");
            }

            // #2
            using (var conn = new SqlConnection(Config.connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("select top 1 * from AccountsInOffers order by 1 desc", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        int PurchaseID = 0;
                        int OfferID = 0;
                        string PurchaseStatus = string.Empty;
                        int CouponCount = 0;
                        string PaymentType = string.Empty;
                        string Email = string.Empty;
                        decimal TotalPrice = 0;

                        while (reader.Read())
                        {
                            PurchaseID = reader.GetInt32(0);
                            OfferID = reader.GetInt32(1);
                            PurchaseStatus = reader.GetString(3);
                            CouponCount = reader.GetInt32(5);
                            PaymentType = reader.GetString(7);
                            Email = reader.GetString(10);
                            TotalPrice = reader.GetDecimal(15);
                        }

                        Assert.AreEqual(OfferID, 28717);
                        Assert.AreEqual(PurchaseStatus, "RESERVED");
                        Assert.AreEqual(CouponCount, 1);
                        Assert.AreEqual(PaymentType, "MoneyOrder");
                        Assert.AreEqual(Email, "srdan.grebensek@gmail.com");
                        Assert.AreEqual(TotalPrice, 99.0000);
                    }
                }
                conn.Close();
            }


        }

        [TearDown]
        public void EndTest()
        {
            driver.Close();
        }
    }
}
