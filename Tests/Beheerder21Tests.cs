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
    public class Beheerder21Tests
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

        // -------------------- TC21.1 Logbestand volledig bekijken --------------------
        [Test]
        public void TC21_1_1_LogbestandVolledigBekijken()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            var logs = GetAllLogs();
            Assert.IsTrue(logs.Count > 0, "Geen logs gevonden");
            foreach (var log in logs)
            {
                Assert.IsNotEmpty(log.Date);
                Assert.IsNotEmpty(log.Time);
                Assert.IsNotEmpty(log.Action);
                Assert.IsNotEmpty(log.User);
            }
        }

        [Test]
        public void TC21_1_2_ControlerenSpecifiekeActie()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            var log = FindLog("medicatie toegevoegd", "Lisa van Dam", "03-10-2025 10:30");
            Assert.IsNotNull(log);
            Assert.AreEqual("Lisa van Dam", log.User);
            Assert.AreEqual("medicatie toegevoegd", log.Action);
        }

        [Test]
        public void TC21_1_3_LeegLogbestand()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            ClearAllLogs(); // Helper die database of overzicht leegt
            var logs = GetAllLogs();
            Assert.IsTrue(logs.Count == 0);
            var message = driver.FindElement(By.Id("noLogsMessage")).Text;
            Assert.IsTrue(message.Contains("Geen acties gevonden"));
        }

        [Test]
        public void TC21_1_4_FilterenOpDatum()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            ApplyDateFilter("03-10-2025");
            var logs = GetAllLogs();
            foreach (var log in logs)
            {
                Assert.IsTrue(log.Date == "03-10-2025");
            }
        }

        // -------------------- TC21.2 Filteren op gebruiker en export --------------------
        [Test]
        public void TC21_2_1_FilterenOpGebruiker()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            ApplyUserFilter("Lisa van Dam");
            var logs = GetAllLogs();
            foreach (var log in logs)
            {
                Assert.AreEqual("Lisa van Dam", log.User);
            }
        }

        [Test]
        public void TC21_2_2_ExportGefilterdeActies()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            ApplyUserFilter("Sander Kok");
            ExportLogs("SanderLogs.csv");
            var exported = File.ReadAllLines("SanderLogs.csv");
            foreach (var line in exported)
            {
                Assert.IsTrue(line.Contains("Sander Kok"));
            }
        }

        [Test]
        public void TC21_2_3_FilterenGebruikerZonderActies()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            ApplyUserFilter("Mark de Vries");
            var logs = GetAllLogs();
            Assert.IsTrue(logs.Count == 0);
            var message = driver.FindElement(By.Id("noLogsMessage")).Text;
            Assert.IsTrue(message.Contains("Geen acties gevonden"));
        }

        [Test]
        public void TC21_2_4_FilterEnExportNegatief()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            ApplyUserFilter("Lisa van Dam");
            var logs = GetAllLogs();
            foreach (var log in logs)
            {
                Assert.AreEqual("Lisa van Dam", log.User);
            }
            ExportLogs("LisaLogs.csv");
            var exported = File.ReadAllLines("LisaLogs.csv");
            foreach (var line in exported)
            {
                Assert.IsTrue(line.Contains("Lisa van Dam") && !line.Contains("Jan Jansen") && !line.Contains("Sander Kok"));
            }
        }

        // -------------------- TC21.3 Logrecords bij wijzigingen --------------------
        [Test]
        public void TC21_3_1_LogrecordBijWijziging()
        {
            LoginAsUser("Lisa van Dam", "P@ss123");
            AddMedication("Paracetamol 500mg");
            SaveChanges();
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            var log = FindLog("Medicijn toegevoegd: Paracetamol 500mg", "Lisa van Dam");
            Assert.IsNotNull(log);
        }

        [Test]
        public void TC21_3_2_LogrecordRaadpleegbaarDoorBeheerder()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            var log = FindLog("Medicijn toegevoegd: Paracetamol 500mg", "Lisa van Dam");
            Assert.IsNotNull(log);
        }

        [Test]
        public void TC21_3_3_LogrecordMeerdereActies()
        {
            LoginAsUser("Sander Kok", "P@ss123");
            AddMedication("Metformine 500mg");
            ChangeDosage("Aspirine 100mg", "150mg");
            SaveChanges();
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            var log1 = FindLog("Medicijn toegevoegd: Metformine 500mg", "Sander Kok");
            var log2 = FindLog("Dosering gewijzigd: Aspirine 100mg", "Sander Kok");
            Assert.IsNotNull(log1);
            Assert.IsNotNull(log2);
        }

        [Test]
        public void TC21_3_4_NegatieveTestWijzigingNietOpgeslagen()
        {
            LoginAsUser("Jan Jansen", "P@ss123");
            ChangeMedicationButCancel("Omeprazol 20mg");
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenLogManagement();
            var log = FindLog("Omeprazol 20mg", "Jan Jansen");
            Assert.IsNull(log);
        }

        // -------------------- Helper Methods --------------------
        private void LoginAsAdmin(string username, string password)
        {
            driver.Navigate().GoToUrl(baseUrl + "/login");
            driver.FindElement(By.Id("username")).SendKeys(username);
            driver.FindElement(By.Id("password")).SendKeys(password);
            driver.FindElement(By.Id("loginButton")).Click();
        }

        private void LoginAsUser(string username, string password)
        {
            driver.Navigate().GoToUrl(baseUrl + "/login");
            driver.FindElement(By.Id("username")).SendKeys(username);
            driver.FindElement(By.Id("password")).SendKeys(password);
            driver.FindElement(By.Id("loginButton")).Click();
        }

        private void OpenLogManagement()
        {
            driver.FindElement(By.Id("logManagement")).Click();
        }

        private void ApplyUserFilter(string username)
        {
            var dropdown = driver.FindElement(By.Id("filterUser"));
            var selectElement = new SelectElement(dropdown);
            selectElement.SelectByText(username);
        }

        private void ApplyDateFilter(string date)
        {
            driver.FindElement(By.Id("filterDate")).SendKeys(date);
            driver.FindElement(By.Id("applyFilter")).Click();
        }

        private void ExportLogs(string filename)
        {
            driver.FindElement(By.Id("exportButton")).Click();
            // Wacht en simuleer download opslaan
            Thread.Sleep(1000);
        }

        private void AddMedication(string medication)
        {
            driver.FindElement(By.Id("addMedication")).SendKeys(medication);
        }

        private void ChangeDosage(string medication, string newDosage)
        {
            driver.FindElement(By.XPath($"//tr[td/text()='{medication}']//input[@class='dosage']")).Clear();
            driver.FindElement(By.XPath($"//tr[td/text()='{medication}']//input[@class='dosage']")).SendKeys(newDosage);
        }

        private void SaveChanges()
        {
            driver.FindElement(By.Id("saveChanges")).Click();
        }

        private void ChangeMedicationButCancel(string medication)
        {
            driver.FindElement(By.XPath($"//tr[td/text()='{medication}']//input[@class='dosage']")).SendKeys("999mg");
            driver.FindElement(By.Id("cancelChanges")).Click();
        }

        private void ClearAllLogs()
        {
            driver.FindElement(By.Id("clearLogs")).Click();
        }

        private LogEntry FindLog(string action, string user, string dateTime = null)
        {
            var logs = GetAllLogs();
            foreach (var log in logs)
            {
                if (log.Action.Contains(action) && log.User == user)
                {
                    if (dateTime == null || log.Date + " " + log.Time == dateTime)
                        return log;
                }
            }
            return null;
        }

        private List<LogEntry> GetAllLogs()
        {
            var list = new List<LogEntry>();
            var rows = driver.FindElements(By.CssSelector("table#logTable tbody tr"));
            foreach (var row in rows)
            {
                var date = row.FindElement(By.CssSelector("td.date")).Text;
                var time = row.FindElement(By.CssSelector("td.time")).Text;
                var user = row.FindElement(By.CssSelector("td.user")).Text;
                var action = row.FindElement(By.CssSelector("td.action")).Text;
                list.Add(new LogEntry { Date = date, Time = time, User = user, Action = action });
            }
            return list;
        }

        public class LogEntry
        {
            public string Date { get; set; }
            public string Time { get; set; }
            public string User { get; set; }
            public string Action { get; set; }
        }
    }
}
