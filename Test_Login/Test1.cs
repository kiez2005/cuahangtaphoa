using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zentask.AutomationTests
{
    [TestClass]
    public class LoginTest
    {
        [TestMethod]
        public void Login_From_CSV_Test()
        {
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Login_test.csv"
            );

            string[] lines = File.ReadAllLines(filePath);

            var testResults = new List<object>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] data = lines[i].Split(',');

                string tcId = data[0];
                string description = data[1];
                string username = data[2];
                string password = data[3];
                string expectedMessage = data[4];

                Console.WriteLine($"Running TestCase: {tcId} - {description}");

                var options = new ChromeOptions();
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("--allow-insecure-localhost");

                using (IWebDriver driver = new ChromeDriver(options))
                {
                    driver.Manage().Window.Maximize();
                    driver.Manage().Cookies.DeleteAllCookies();

                    driver.Navigate().GoToUrl("http://localhost:5001/Login/Index"); 

                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    wait.Until(d => d.FindElement(By.Id("TenDangNhap")));

                    int delay = 1000; 

                    var usernameField = driver.FindElement(By.Id("TenDangNhap"));
                    usernameField.Clear();
                    Thread.Sleep(delay);
                    usernameField.SendKeys(username);

                    Thread.Sleep(delay);

                    var passwordField = driver.FindElement(By.Id("MatKhau"));
                    passwordField.Clear();
                    Thread.Sleep(delay);
                    passwordField.SendKeys(password);

                    Thread.Sleep(delay);

                    driver.FindElement(By.CssSelector("button[type='submit']")).Click();

                    Thread.Sleep(2000); 

                    bool isPassed = false;
                    string actual = "";

                    try
                    {
                        if (expectedMessage.Trim().ToLower() == "home page")
                        {
                            wait.Until(d => d.Url.Contains("Dashboard"));
                            isPassed = driver.Url.Contains("Dashboard");
                            actual = driver.Url;
                        }
                        else
                        {
                            wait.Until(d => d.PageSource.Contains(expectedMessage));
                            isPassed = driver.PageSource.Contains(expectedMessage);
                            actual = expectedMessage;
                        }
                    }
                    catch
                    {
                        isPassed = false;
                        actual = "Không tìm thấy kết quả mong đợi";
                    }

                    string status = isPassed ? "PASS" : "FAIL";

                    testResults.Add(new
                    {
                        TestCaseID = tcId,
                        Description = description,
                        Username = string.IsNullOrEmpty(username) ? "[Trống]" : username,
                        Password = string.IsNullOrEmpty(password) ? "[Trống]" : "******",
                        Expected = expectedMessage,
                        Actual = actual,
                        Status = status,
                        Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });

                    driver.Quit();
                }
            }

            string jsonFile = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Test-report.json"
            );

            string jsonContent = JsonConvert.SerializeObject(testResults, Formatting.Indented);
            File.WriteAllText(jsonFile, jsonContent);

            Console.WriteLine("Test completed! Report saved at: " + jsonFile);

            bool hasFailed = testResults.Any(res => (string)((dynamic)res).Status == "FAIL");

            if (hasFailed)
                Assert.Fail("Có test case FAIL. Xem Test-report.json");
        }
    }
}