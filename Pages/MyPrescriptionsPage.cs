using OpenQA.Selenium;

namespace UiTests.Pages
{
    public class MyPrescriptionsPage
    {
        private readonly IWebDriver _driver;

        public MyPrescriptionsPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // --- Locators ---
        private IWebElement AlertInfo => _driver.FindElement(By.CssSelector(".alert-info"));
        private IReadOnlyCollection<IWebElement> MedicationRows =>
            _driver.FindElements(By.CssSelector(".table tbody tr"));
        private IWebElement LoggedInUser => _driver.FindElement(By.CssSelector("span[name='user_name']"));

        // --- Actions & Assertions ---
        public string GetLoggedInUsername() => LoggedInUser.Text.Trim();

        public bool HasNoPrescriptions()
        {
            try
            {
                return AlertInfo.Text.Contains("You have no current prescriptions", StringComparison.OrdinalIgnoreCase);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public bool ContainsMedication(string medicationName)
        {
            foreach (var row in MedicationRows)
            {
                if (row.Text.Contains(medicationName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public bool CanSeeOtherPatientData(int patientId)
        {
            // Example: if your rows include patient IDs or names, verify those aren’t visible
            return _driver.PageSource.Contains($"Patient ID: {patientId}");
        }
    }
}
