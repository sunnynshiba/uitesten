using OpenQA.Selenium;

public class LoginPage
{
    private IWebDriver _driver;
    private string _baseUrl;

    public LoginPage(IWebDriver driver, string baseUrl)
    {
        _driver = driver;
        _baseUrl = baseUrl;
    }

    public void GoTo() => _driver.Navigate().GoToUrl(_baseUrl);

    public void Login(string username, string password)
    {
        _driver.FindElement(By.Id("Username")).Clear();
        _driver.FindElement(By.Id("Username")).SendKeys(username);

        _driver.FindElement(By.Id("Password")).Clear();
        _driver.FindElement(By.Id("Password")).SendKeys(password);

        _driver.FindElement(By.Name("btn-login")).Click();
    }

    public string GetErrorMessage()
    {
        var errorElement = _driver.FindElement(By.Id("login-error"));
        return errorElement?.Text ?? string.Empty;
    }

    // --- Safe validation message helper ---
    private string GetValidationMessage(string elementId)
    {
        var element = _driver.FindElement(By.Id(elementId));
        return element?.GetAttribute("validationMessage") ?? string.Empty;
    }

    public string GetUsernameValidationMessage()
    {
        return GetValidationMessage("Username");
    }

    public string GetPasswordValidationMessage()
    {
        return GetValidationMessage("Password");
    }
}
