using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace UiTests.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl;
        private readonly WebDriverWait _wait;

        public LoginPage(IWebDriver driver, string baseUrl)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        }

        public void GoTo()
        {
            _driver.Navigate().GoToUrl(_baseUrl);
        }

        public void Login(string username, string password)
        {
            var usernameInput = _driver.FindElement(By.Id("Username"));
            var passwordInput = _driver.FindElement(By.Id("Password"));
            var loginButton = _driver.FindElement(By.Name("btn-login"));

            usernameInput.Clear();
            usernameInput.SendKeys(username);

            passwordInput.Clear();
            passwordInput.SendKeys(password);

            loginButton.Click();
        }

        public string GetErrorMessage()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));

            // Wait until the page reloads and the error span is present and visible
            var errorElement = wait.Until(d =>
            {
                try
                {
                    var e = d.FindElement(By.Id("login-error"));
                    return e.Displayed ? e : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });

            return errorElement.Text.Trim();
        }



        public string GetUsernameValidationMessage()
        {
            var usernameInput = _driver.FindElement(By.Id("Username"));
            return usernameInput.GetAttribute("validationMessage");
        }

        public string GetPasswordValidationMessage()
        {
            var passwordInput = _driver.FindElement(By.Id("Password"));
            return passwordInput.GetAttribute("validationMessage");
        }
    }
}
