using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Zentask.AutomationTests
{
    [TestClass]
    public class LoginTest
    {
        [TestMethod]
        public void Login_From_CSV_Test()
        {
            string[] lines = File.ReadAllLines("Login_test.csv");
            var testResults = new List<object>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] data = lines[i].Split(',');

                string username = data[0];
                string password = data[1];
                string expectedMessage = data[2];

                using (IWebDriver driver = new ChromeDriver())
                {
                    driver.Manage().Window.Maximize();
                    driver.Manage().Cookies.DeleteAllCookies();

                    driver.Navigate().GoToUrl("https://localhost:44355/Login/Index");

                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(d => d.FindElement(By.Id("TenDangNhap")));

                    // Nhập username
                    var usernameField = driver.FindElement(By.Id("TenDangNhap"));
                    usernameField.Clear();
                    if (!string.IsNullOrEmpty(username))
                        usernameField.SendKeys(username);

                    // Nhập password
                    var passwordField = driver.FindElement(By.Id("MatKhau"));
                    passwordField.Clear();
                    if (!string.IsNullOrEmpty(password))
                        passwordField.SendKeys(password);

                    // Click login
                    driver.FindElement(By.CssSelector("button[type='submit']")).Click();

                    Thread.Sleep(1500);

                    bool isPassed = false;

                    // Nếu là Home page → kiểm tra redirect thành công
                    if (expectedMessage.Trim().ToLower() == "home page")
                    {
                        isPassed = driver.Url.Contains("TrangChu");
                    }
                    else
                    {
                        isPassed = driver.PageSource.Contains(expectedMessage);
                    }

                    string status = isPassed ? "PASS" : "FAIL";

                    testResults.Add(new
                    {
                        ID = i,
                        Username = string.IsNullOrEmpty(username) ? "[Trống]" : username,
                        Password = string.IsNullOrEmpty(password) ? "[Trống]" : "******",
                        Expected = expectedMessage,
                        ActualUrl = driver.Url,
                        Status = status,
                        Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
            }

            // Xuất report JSON
            string jsonFileName = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Test-report.json"
            );
            string jsonContent = JsonConvert.SerializeObject(testResults, Formatting.Indented);
            File.WriteAllText(jsonFileName, jsonContent);

            bool hasFailed = testResults.Any(res => (string)((dynamic)res).Status == "FAIL");

            if (hasFailed)
                Assert.Fail("Có ít nhất một test case thất bại. Xem Test-report.json");
        }
    }
}