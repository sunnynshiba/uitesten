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
    public class Huisarts7Tests
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

        //Helper: create prescription
        private void CreatePrescription(string patientId, string startDate, string endDate, string instructions)
        {
            // Select patient
            var patientDropdown = new SelectElement(driver.FindElement(By.Id("patientSelect")));
            patientDropdown.SelectByValue(patientId);

            // Fill dates
            driver.FindElement(By.Id("startDate")).SendKeys(startDate);
            driver.FindElement(By.Id("endDate")).SendKeys(endDate);

            // Fill instructions / description
            driver.FindElement(By.Id("generalInstructions")).SendKeys(instructions);

            // Save prescription
            driver.FindElement(By.CssSelector("button.btn-primary")).Click();
        }

        // Helper: add medicine to prescription
        private void AddMedicineToPrescription(string medicineId, string quantity, string instructions)
        {
            // Click "Add Medicine" button
            driver.FindElement(By.CssSelector("button.btn-outline-primary")).Click();

            // Select medicine
            var medicineDropdown = new SelectElement(driver.FindElement(By.Id("medicineSelect")));
            medicineDropdown.SelectByValue(medicineId);

            // Fill quantity
            driver.FindElement(By.Id("quantity")).SendKeys(quantity);

            // Fill instructions
            driver.FindElement(By.Id("instructions")).SendKeys(instructions);

            // Save medicine
            driver.FindElement(By.CssSelector("button.btn-success")).Click();
        }



        //  TC7.1 
        [Test]
        public void TC7_1_1_CorrectVoorschrijvenNieuwMedicijn()
        {
            LoginAs("huisarts1", "huisartsen");
            driver.Navigate().GoToUrl(baseUrl + "/Prescriptions/new");

            // Step 1: Create prescription
            CreatePrescription("7", "2025-10-10", "2025-10-20", "Neem medicijn na elke maaltijd");

            // Step 2: Add medicine to it
            AddMedicineToPrescription("2", "15", "2x per dag");

            // Assert medicine shows up in the prescription details
            Assert.IsTrue(driver.PageSource.Contains("Ibuprofen"));
        }

        [Test]
        public void TC7_1_2_VoorschrijvenZonderDosering()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            driver.FindElement(By.Id("newMedicationButton")).Click();
            driver.FindElement(By.Id("medicationName")).SendKeys("Ibuprofen");
            driver.FindElement(By.Id("startDate")).SendKeys(DateTime.Today.ToString("yyyy-MM-dd"));
            driver.FindElement(By.Id("intakeTimes")).SendKeys("09:00,21:00");
            driver.FindElement(By.Id("saveMedication")).Click();

            var error = driver.FindElement(By.Id("errorMessage")).Text;
            Assert.AreEqual("Dosering is verplicht", error);
        }

        [Test]
        public void TC7_1_3_StartdatumInVerleden()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            driver.FindElement(By.Id("newMedicationButton")).Click();
            driver.FindElement(By.Id("medicationName")).SendKeys("Amoxicilline");
            driver.FindElement(By.Id("dosage")).SendKeys("250 mg");
            driver.FindElement(By.Id("startDate")).SendKeys("2025-09-01");
            driver.FindElement(By.Id("intakeTimes")).SendKeys("08:00,20:00");
            driver.FindElement(By.Id("saveMedication")).Click();

            var warning = driver.FindElement(By.Id("warningMessage")).Text;
            Assert.IsTrue(warning.Contains("Startdatum ligt in het verleden"));
        }

        [Test]
        public void TC7_1_4_MedicijnMetInteractie()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            driver.FindElement(By.Id("newMedicationButton")).Click();
            driver.FindElement(By.Id("medicationName")).SendKeys("Aspirine");
            driver.FindElement(By.Id("dosage")).SendKeys("100 mg");
            driver.FindElement(By.Id("startDate")).SendKeys(DateTime.Today.ToString("yyyy-MM-dd"));
            driver.FindElement(By.Id("intakeTimes")).SendKeys("08:00");
            driver.FindElement(By.Id("saveMedication")).Click();

            var warning = driver.FindElement(By.Id("warningMessage")).Text;
            Assert.IsTrue(warning.Contains("mogelijke interacties"));
        }

        [Test]
        public void TC7_1_5_ZonderDossiertoegang()
        {
            LoginAs("arts2", "huisartsen");
            driver.FindElement(By.Id("searchPatient")).SendKeys("PatiëntZonderToegang");
            driver.FindElement(By.Id("openDossier")).Click();

            var error = driver.FindElement(By.Id("errorMessage")).Text;
            Assert.AreEqual("Geen toegang tot dossier", error);
        }

        //TC7.2 
        [Test]
        public void TC7_2_1_NieuwVoorschriftDirectZichtbaar()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            AddNewMedication("Paracetamol", "500 mg", DateTime.Today, "08:00,20:00");
            driver.FindElement(By.Id("refreshOverview")).Click();

            var list = driver.FindElement(By.Id("medicationList")).Text;
            Assert.IsTrue(list.Contains("Paracetamol"));
        }

        [Test]
        public void TC7_2_2_WaarschuwingContraIndicaties()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            AddNewMedication("Aspirine", "100 mg", DateTime.Today, "08:00");
            driver.FindElement(By.Id("refreshOverview")).Click();

            var warning = driver.FindElement(By.Id("warningMessage")).Text;
            Assert.IsTrue(warning.Contains("mogelijke interacties"));
        }

        [Test]
        public void TC7_2_3_VernieuwenZonderNieuwVoorschrift()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            driver.FindElement(By.Id("refreshOverview")).Click();
            var list = driver.FindElement(By.Id("medicationList")).Text;
            Assert.IsNotNull(list); // overzicht blijft onveranderd
        }

        [Test]
        public void TC7_2_4_MeerderVoorschriftenVernieuwen()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            AddNewMedication("Paracetamol", "500 mg", DateTime.Today, "08:00,20:00");
            AddNewMedication("Amoxicilline", "250 mg", DateTime.Today, "09:00,21:00");
            driver.FindElement(By.Id("refreshOverview")).Click();

            var list = driver.FindElement(By.Id("medicationList")).Text;
            Assert.IsTrue(list.Contains("Paracetamol"));
            Assert.IsTrue(list.Contains("Amoxicilline"));
        }

        [Test]
        public void TC7_2_5_VernieuwenBijNetwerkvertraging()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            AddNewMedication("Ibuprofen", "400 mg", DateTime.Today, "08:00,20:00");

            // Simuleer netwerkvertraging
            driver.FindElement(By.Id("refreshOverview")).Click();
            var list = driver.FindElement(By.Id("medicationList")).Text;
            Assert.IsTrue(list.Contains("Ibuprofen"));
        }

        // TC7.3 
        [Test]
        public void TC7_3_1_WaarschuwingBijInteractie()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            AddNewMedication("Aspirine", "100 mg", DateTime.Today, "08:00");
            driver.FindElement(By.Id("saveMedication")).Click();

            var warning = driver.FindElement(By.Id("warningMessage")).Text;
            Assert.IsTrue(warning.Contains("mogelijke interactie"));
        }

        [Test]
        public void TC7_3_2_AlternatiefMedicijnKiezen()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            AddNewMedication("Aspirine", "100 mg", DateTime.Today, "08:00");
            driver.FindElement(By.Id("saveMedication")).Click();
            driver.FindElement(By.Id("chooseAlternative")).Click();
            driver.FindElement(By.Id("alternativeMedication")).SendKeys("Paracetamol");
            driver.FindElement(By.Id("confirmAlternative")).Click();

            var list = driver.FindElement(By.Id("medicationList")).Text;
            Assert.IsTrue(list.Contains("Paracetamol"));
        }

        [Test]
        public void TC7_3_3_WaarschuwingNegerenBevestigen()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            AddNewMedication("Aspirine", "100 mg", DateTime.Today, "08:00");
            driver.FindElement(By.Id("saveMedication")).Click();
            driver.FindElement(By.Id("ignoreWarning")).Click();

            var list = driver.FindElement(By.Id("medicationList")).Text;
            Assert.IsTrue(list.Contains("Aspirine"));
        }

        [Test]
        public void TC7_3_4_MeerderInteractieMedicijnen()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            AddNewMedication("Aspirine + Ibuprofen", "100 mg", DateTime.Today, "08:00");
            driver.FindElement(By.Id("saveMedication")).Click();

            var warning = driver.FindElement(By.Id("warningMessage")).Text;
            Assert.IsTrue(warning.Contains("mogelijke interacties"));
        }

        [Test]
        public void TC7_3_5_OpslaanZonderInteractie()
        {
            LoginAs("huisarts1", "huisartsen");
            //OpenPateintDossier("Jan Jansen");

            AddNewMedication("Vitamin C", "500 mg", DateTime.Today, "08:00");
            driver.FindElement(By.Id("saveMedication")).Click();

            var warning = driver.FindElements(By.Id("warningMessage"));
            Assert.IsTrue(warning.Count == 0); // geen waarschuwing
        }


        private void AddNewMedication(string name, string dosage, DateTime startDate, string intakeTimes)
        {
            driver.FindElement(By.Id("newMedicationButton")).Click();
            driver.FindElement(By.Id("medicationName")).SendKeys(name);
            driver.FindElement(By.Id("dosage")).SendKeys(dosage);
            driver.FindElement(By.Id("startDate")).SendKeys(startDate.ToString("yyyy-MM-dd"));
            driver.FindElement(By.Id("intakeTimes")).SendKeys(intakeTimes);
            driver.FindElement(By.Id("saveMedication")).Click();
        }
    }
}
