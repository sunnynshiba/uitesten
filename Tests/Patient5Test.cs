using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace UiTests.Tests
{
    [TestFixture]
    public class Patient5Tests
    {
        private IWebDriver driver;
        private string baseUrl = "http://localhost:5070";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            driver = new ChromeDriver();
            LoginAs("pietp", "Wachtwoord2"); // Jan Jansen default login
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            driver.Quit();
        }

        // ---- Helper: login as a patient ----
        private void LoginAs(string username, string password)
        {
            driver.Navigate().GoToUrl(baseUrl);
            driver.FindElement(By.Id("Username")).SendKeys(username);
            driver.FindElement(By.Id("Password")).SendKeys(password);
            driver.FindElement(By.Name("btn-login")).Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Url.Contains("/MyPrescriptions"));
        }

        // ---- AC5.1: Medicatieoverzicht ----

        [Test]
        public void TC5_1_1_MedicationOverview_HappyPath()
        {
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions");

            // Wait for medication list
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("medication-list")));

            bool amoxicillinExists = driver.FindElements(By.XPath("//td[normalize-space()='Amoxicillin']")).Count > 0;
            Assert.IsTrue(amoxicillinExists, "Amoxicillin should appear in the medication list");

            // Check for Ciprofloxacin
            bool ciprofloxacinExists = driver.FindElements(By.XPath("//td[normalize-space()='Ciprofloxacin']")).Count > 0;
            Assert.IsTrue(ciprofloxacinExists, "Ciprofloxacin should appear in the medication list");
        }

        [Test]
        public void TC5_1_2_MultipleIntakeTimes()
        {
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions");

            IWebElement med = driver.FindElement(By.XPath("//div[contains(., 'Metformine 500mg')]"));
            Assert.IsTrue(med.Text.Contains("08:00"));
            Assert.IsTrue(med.Text.Contains("14:00"));
            Assert.IsTrue(med.Text.Contains("20:00"));
        }

        [Test]
        public void TC5_1_3_WrongDataLoaded_Negative()
        {
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions?error=data");

            IWebElement error = new WebDriverWait(driver, TimeSpan.FromSeconds(5))
                .Until(d => d.FindElement(By.Id("medication-error")));

            Assert.AreEqual("Medicatiegegevens tijdelijk niet beschikbaar", error.Text);
        }

        [Test]
        public void TC5_1_4_SystemErrorOnLoad()
        {
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions?error=server");

            IWebElement error = new WebDriverWait(driver, TimeSpan.FromSeconds(5))
                .Until(d => d.FindElement(By.Id("medication-error")));

            Assert.AreEqual("Er is een technische fout opgetreden. Probeer later opnieuw.", error.Text);
        }

        // ---- AC5.2: Geen actieve medicatie ----

        [Test]
        public void TC5_2_1_NoMedication_HappyPath()
        {
            LoginAs("lisavd", "Wachtwoord2"); // Lisa van Dam, geen medicatie
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions");

            IWebElement msg = new WebDriverWait(driver, TimeSpan.FromSeconds(5))
                .Until(d => d.FindElement(By.Id("no-medication-msg")));

            Assert.AreEqual("Geen actieve medicatie beschikbaar", msg.Text);
        }

        [Test]
        public void TC5_2_2_ExpiredMedicationHidden()
        {
            LoginAs("lisavd", "Wachtwoord2");
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions");

            string page = driver.PageSource;
            Assert.IsFalse(page.Contains("Ibuprofen")); // expired med should not show
            Assert.IsTrue(page.Contains("Geen actieve medicatie beschikbaar"));
        }

        [Test]
        public void TC5_2_3_BackendErrorOnEmptyList()
        {
            LoginAs("lisavd", "Wachtwoord2");
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions?error=db");

            IWebElement msg = new WebDriverWait(driver, TimeSpan.FromSeconds(5))
                .Until(d => d.FindElement(By.Id("medication-error")));

            Assert.AreEqual("Kan medicatiegegevens niet ophalen. Probeer later opnieuw.", msg.Text);
        }

        // ---- AC5.3: Nieuwe medicatie updates ----

        [Test]
        public void TC5_3_1_NewMedicationVisibleAfterRefresh()
        {
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions");

            driver.FindElement(By.Id("btn-refresh")).Click();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            wait.Until(d => d.PageSource.Contains("Amoxicilline 500mg"));
            Assert.IsTrue(driver.PageSource.Contains("Amoxicilline 500mg"));
        }

        [Test]
        public void TC5_3_2_LiveUpdateWithoutReload()
        {
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            bool appears = wait.Until(d => d.PageSource.Contains("Amoxicilline 500mg"));
            Assert.IsTrue(appears, "Nieuwe medicatie zou automatisch moeten verschijnen");
        }

        [Test]
        public void TC5_3_3_NewMedicationNotVisible_Negative()
        {
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions?error=sync");

            IWebElement msg = new WebDriverWait(driver, TimeSpan.FromSeconds(5))
                .Until(d => d.FindElement(By.Id("medication-error")));

            Assert.AreEqual("Medicatiegegevens niet up-to-date. Probeer opnieuw te laden.", msg.Text);
        }

        [Test]
        public void TC5_3_4_MultipleNewMedications()
        {
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions");

            driver.FindElement(By.Id("btn-refresh")).Click();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            string page = driver.PageSource;
            Assert.IsTrue(page.Contains("Amoxicilline 500mg"));
            Assert.IsTrue(page.Contains("Prednison 10mg"));
            Assert.IsFalse(page.Contains("duplicaat"), "Geen dubbele records toegestaan");
        }

        [Test]
        public void TC5_3_5_RemoveOldMedicationAfterUpdate()
        {
            driver.Navigate().GoToUrl(baseUrl + "/MyPrescriptions");
            driver.FindElement(By.Id("btn-refresh")).Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.PageSource.Contains("Amoxicilline 500mg"));

            string page = driver.PageSource;
            Assert.IsFalse(page.Contains("Paracetamol 500mg (verlopen)"));
            Assert.IsTrue(page.Contains("Amoxicilline 500mg"));
        }
    }
}
