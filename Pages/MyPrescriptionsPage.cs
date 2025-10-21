using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace UiTests.Pages
{
    public class MyPrescriptionsPage
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl;
        private readonly WebDriverWait _wait;

        public MyPrescriptionsPage(IWebDriver driver, string baseUrl)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        }

        public bool ContainsMedication(string name)
        {
            try
            {
                _wait.Until(d => d.FindElement(By.Id("prescriptions-table")));
                return _driver.PageSource.Contains(name);
            }
            catch
            {
                return false;
            }
        }

        public bool CanSeeOtherPatientData(int patientId)
        {
            try
            {
                return _driver.PageSource.Contains($"patient-{patientId}");
            }
            catch
            {
                return false;
            }
        }

        public bool HasNoPrescriptions()
        {
            try
            {
                return _driver.FindElements(By.CssSelector(".prescription-row")).Count == 0;
            }
            catch
            {
                return true;
            }
        }
    }
}
