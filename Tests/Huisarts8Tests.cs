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
    public class Huisarts8Tests
    {
        private IWebDriver driver;
        private string baseUrl = "http://localhost:5070";

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
        }

        [TearDown]
        public void Teardown()
        {
            driver.Quit();
        }

        // Helper: login as x
        private void LoginAs(string username, string password)
        {
            driver.Navigate().GoToUrl(baseUrl);

            driver.FindElement(By.Id("Username")).SendKeys(username);
            driver.FindElement(By.Id("Password")).SendKeys(password);
            driver.FindElement(By.Name("btn-login")).Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Url.Contains("/MyPrescriptions"));
        }

        // -------------------- TC8.1 Dosering instellen --------------------
        [Test]
        public void TC8_1_1_StandaardDosering()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetDosage("Paracetamol 500 mg", "500 mg");
            Assert.IsTrue(GetDosage("Paracetamol 500 mg") == "500 mg");
        }

        [Test]
        public void TC8_1_2_VerhoogDosering()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetDosage("Ibuprofen 200 mg", "400 mg");
            Assert.IsTrue(GetDosage("Ibuprofen 200 mg") == "400 mg");
        }

        [Test]
        public void TC8_1_3_VerlaagDosering()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetDosage("Metformine 500 mg", "250 mg");
            Assert.IsTrue(GetDosage("Metformine 500 mg") == "250 mg");
        }

        [Test]
        public void TC8_1_4_OngeldigeDosering()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetDosage("Amoxicilline 500 mg", "-100 mg");
            var error = driver.FindElement(By.Id("errorMessage")).Text;
            Assert.IsTrue(error.Contains("ongeldige dosering"));
        }

        [Test]
        public void TC8_1_5_MeerderDoseringenAanpassen()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetDosage("Paracetamol 500 mg", "650 mg");
            SetDosage("Ibuprofen 200 mg", "400 mg");
            Assert.IsTrue(GetDosage("Paracetamol 500 mg") == "650 mg");
            Assert.IsTrue(GetDosage("Ibuprofen 200 mg") == "400 mg");
        }

        // -------------------- TC8.2 Innametijden instellen --------------------
        [Test]
        public void TC8_2_1_EnkeleInnametijd()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetIntakeTimes("Paracetamol 500 mg", "08:00");
            Assert.IsTrue(GetIntakeTimes("Paracetamol 500 mg").Contains("08:00"));
        }

        [Test]
        public void TC8_2_2_MeerderInnametijden()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetIntakeTimes("Ibuprofen 200 mg", "08:00,14:00,20:00");
            var times = GetIntakeTimes("Ibuprofen 200 mg");
            Assert.IsTrue(times.Contains("08:00") && times.Contains("14:00") && times.Contains("20:00"));
        }

        [Test]
        public void TC8_2_3_OngeldigeInnametijd()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetIntakeTimes("Metformine 500 mg", "25:00");
            var error = driver.FindElement(By.Id("errorMessage")).Text;
            Assert.IsTrue(error.Contains("ongeldige tijd"));
        }

        [Test]
        public void TC8_2_4_InnametijdenAanpassen()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetIntakeTimes("Amoxicilline 500 mg", "07:00,15:00,22:00");
            var times = GetIntakeTimes("Amoxicilline 500 mg");
            Assert.IsTrue(times.Contains("07:00") && times.Contains("15:00") && times.Contains("22:00"));
        }

        [Test]
        public void TC8_2_5_InnametijdenMeerdereMedicijnen()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetIntakeTimes("Paracetamol 500 mg", "08:00,20:00");
            SetIntakeTimes("Ibuprofen 200 mg", "09:00,21:00");
            Assert.IsTrue(GetIntakeTimes("Paracetamol 500 mg").Contains("08:00"));
            Assert.IsTrue(GetIntakeTimes("Ibuprofen 200 mg").Contains("09:00"));
        }

        // -------------------- TC8.3 Wijzigingen --------------------
        [Test]
        public void TC8_3_1_DoseringWijzigen()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetDosage("Paracetamol 500 mg", "650 mg");
            Assert.IsTrue(GetDosage("Paracetamol 500 mg") == "650 mg");
        }

        [Test]
        public void TC8_3_2_InnametijdWijzigen()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetIntakeTimes("Ibuprofen 200 mg", "09:00,21:00");
            var times = GetIntakeTimes("Ibuprofen 200 mg");
            Assert.IsTrue(times.Contains("09:00") && times.Contains("21:00"));
        }

        [Test]
        public void TC8_3_3_DoseringEnInnametijdWijzigen()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetDosage("Amoxicilline 500 mg", "750 mg");
            SetIntakeTimes("Amoxicilline 500 mg", "07:00,19:00");
            Assert.IsTrue(GetDosage("Amoxicilline 500 mg") == "750 mg");
            var times = GetIntakeTimes("Amoxicilline 500 mg");
            Assert.IsTrue(times.Contains("07:00") && times.Contains("19:00"));
        }

        [Test]
        public void TC8_3_4_ConflictMetAnderMedicijn()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetDosage("Ibuprofen", "400 mg");
            var warning = driver.FindElement(By.Id("warningMessage")).Text;
            Assert.IsTrue(warning.Contains("mogelijke interactie"));
        }

        [Test]
        public void TC8_3_5_OngeldigeWijziging()
        {
            LoginAsDoctor("arts1", "Wachtw123");
            OpenPatientDossier("Jan Jansen");
            SetDosage("Metformine 500 mg", "-500 mg");
            var error = driver.FindElement(By.Id("errorMessage")).Text;
            Assert.IsTrue(error.Contains("ongeldige dosering"));
        }

        // -------------------- Helper Methods --------------------
        private void LoginAsDoctor(string username, string password)
        {
            driver.Navigate().GoToUrl(baseUrl + "/login");
            driver.FindElement(By.Id("username")).SendKeys(username);
            driver.FindElement(By.Id("password")).SendKeys(password);
            driver.FindElement(By.Id("loginButton")).Click();
        }

        private void OpenPatientDossier(string patientName)
        {
            driver.FindElement(By.Id("searchPatient")).SendKeys(patientName);
            driver.FindElement(By.Id("openDossier")).Click();
        }

        private void SetDosage(string medicationName, string dosage)
        {
            driver.FindElement(By.XPath($"//tr[td/text()='{medicationName}']//button[text()='Bewerk']")).Click();
            var dosageInput = driver.FindElement(By.XPath($"//tr[td/text()='{medicationName}']//input[@class='dosage']"));
            dosageInput.Clear();
            dosageInput.SendKeys(dosage);
            driver.FindElement(By.XPath($"//tr[td/text()='{medicationName}']//button[text()='Opslaan']")).Click();
        }

        private string GetDosage(string medicationName)
        {
            return driver.FindElement(By.XPath($"//tr[td/text()='{medicationName}']/td[@class='dosageValue']")).Text;
        }

        private void SetIntakeTimes(string medicationName, string times)
        {
            driver.FindElement(By.XPath($"//tr[td/text()='{medicationName}']//button[text()='Bewerk']")).Click();
            var timesInput = driver.FindElement(By.XPath($"//tr[td/text()='{medicationName}']//input[@class='intakeTimes']"));
            timesInput.Clear();
            timesInput.SendKeys(times);
            driver.FindElement(By.XPath($"//tr[td/text()='{medicationName}']//button[text()='Opslaan']")).Click();
        }

        private string GetIntakeTimes(string medicationName)
        {
            return driver.FindElement(By.XPath($"//tr[td/text()='{medicationName}']/td[@class='intakeTimesValue']")).Text;
        }
    }
}
