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
    public class Beheerder20Tests
    {
        private IWebDriver driver;
        private string baseUrl = "http://localhost:7058"; 

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }

        // -------------------- TC20.1 Nieuwe gebruiker aanmaken --------------------
        [Test]
        public void TC20_1_1_NieuwePatientAanmaken()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            AddNewUser("Jan Jansen", "janjansen", "P@ss123", "patiënt");
            Assert.IsTrue(UserExistsWithRole("janjansen", "patiënt"));
        }

        [Test]
        public void TC20_1_2_NieuweArtsAanmaken()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            AddNewUser("Dr. Petra de Vries", "petra", "P@ss123", "arts");
            Assert.IsTrue(UserExistsWithRole("petra", "arts"));
        }

        [Test]
        public void TC20_1_3_NieuweApothekerAanmaken()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            AddNewUser("Sander Kok", "sander", "P@ss123", "apotheker");
            Assert.IsTrue(UserExistsWithRole("sander", "apotheker"));
        }

        [Test]
        public void TC20_1_4_GebruikerZonderRol()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            AddNewUser("Lisa van Dam", "lisa", "P@ss123", "");
            var error = driver.FindElement(By.Id("errorMessage")).Text;
            Assert.IsTrue(error.Contains("Rol is verplicht"));
        }

        [Test]
        public void TC20_1_5_GebruikerBestaandeGebruikersnaam()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            AddNewUser("Jan Jansen", "janjansen", "P@ss123", "patiënt");
            var error = driver.FindElement(By.Id("errorMessage")).Text;
            Assert.IsTrue(error.Contains("Gebruikersnaam bestaat al"));
        }

        // -------------------- TC20.2 Rol wijzigen --------------------
        [Test]
        public void TC20_2_1_PatientNaarArts()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            ChangeUserRole("janjansen", "arts");
            Assert.IsTrue(UserHasRole("janjansen", "arts") && !UserHasRole("janjansen", "patiënt"));
        }

        [Test]
        public void TC20_2_2_ArtsNaarApotheker()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            ChangeUserRole("petra", "apotheker");
            Assert.IsTrue(UserHasRole("petra", "apotheker") && !UserHasRole("petra", "arts"));
        }

        [Test]
        public void TC20_2_3_ApothekerNaarPatient()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            ChangeUserRole("sander", "patiënt");
            Assert.IsTrue(UserHasRole("sander", "patiënt") && !UserHasRole("sander", "apotheker"));
        }

        [Test]
        public void TC20_2_4_OngeldigeRolWijzigen()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            ChangeUserRole("janjansen", "manager");
            var error = driver.FindElement(By.Id("errorMessage")).Text;
            Assert.IsTrue(error.Contains("Ongeldige rol"));
        }

        [Test]
        public void TC20_2_5_SlechtsEenRolActief()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            ChangeUserRole("lisa", "arts");
            Assert.IsTrue(UserHasRole("lisa", "arts") && !UserHasRole("lisa", "patiënt"));
        }

        // -------------------- TC20.3 Rol verwijderen --------------------
        [Test]
        public void TC20_3_1_BevestigRolVerwijdering()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            DeleteUserRole("janjansen", true);
            Assert.IsFalse(UserExists("janjansen"));
        }

        [Test]
        public void TC20_3_2_AnnuleerRolVerwijdering()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            DeleteUserRole("lisa", false);
            Assert.IsTrue(UserExists("lisa"));
        }

        [Test]
        public void TC20_3_3_VerwijderingMeerdereGebruikers()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            DeleteUserRole("sander", true);
            Assert.IsFalse(UserHasRole("sander", "apotheker"));
            Assert.IsTrue(UserHasRole("petra", "arts"));
        }

        [Test]
        public void TC20_3_4_GebruikerZonderRolKanNietInloggen()
        {
            driver.Navigate().GoToUrl(baseUrl + "/logout");
            LoginAsUser("janjansen", "P@ss123");
            var error = driver.FindElement(By.Id("errorMessage")).Text;
            Assert.IsTrue(error.Contains("geen rol"));
        }

        [Test]
        public void TC20_3_5_BevestigingsvraagConsistentie()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            string[] users = { "janjansen", "lisa", "sander" };
            foreach (var user in users)
            {
                var confirm = TriggerDeleteRole(user);
                Assert.IsTrue(confirm.Displayed);
            }
        }

        // -------------------- TC20.4 Rollenoverzicht en filter --------------------
        [Test]
        public void TC20_4_1_ControleToegewezenRollen()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            var allUsers = GetAllUsers();
            foreach (var u in allUsers)
            {
                Assert.AreEqual(1, u.Roles.Count);
            }
        }

        [Test]
        public void TC20_4_2_FilterPatient()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            ApplyRoleFilter("patiënt");
            var users = GetDisplayedUsers();
            foreach (var u in users)
            {
                Assert.IsTrue(u.Roles.Contains("patiënt"));
            }
        }

        [Test]
        public void TC20_4_3_FilterArts()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            ApplyRoleFilter("arts");
            var users = GetDisplayedUsers();
            foreach (var u in users)
            {
                Assert.IsTrue(u.Roles.Contains("arts"));
            }
        }

        [Test]
        public void TC20_4_4_FilterApotheker()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            ApplyRoleFilter("apotheker");
            var users = GetDisplayedUsers();
            foreach (var u in users)
            {
                Assert.IsTrue(u.Roles.Contains("apotheker"));
            }
        }

        [Test]
        public void TC20_4_5_FilterResetten()
        {
            LoginAsAdmin("admin", "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=");
            OpenUserManagement();
            ApplyRoleFilter("patiënt");
            ResetRoleFilter();
            var users = GetDisplayedUsers();
            Assert.IsTrue(users.Count >= 3); // minimaal alle testgebruikers zichtbaar
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

        private void OpenUserManagement()
        {
            driver.FindElement(By.Id("userManagement")).Click();
        }

        private void AddNewUser(string name, string username, string password, string role)
        {
            driver.FindElement(By.Id("addUserButton")).Click();
            driver.FindElement(By.Id("name")).SendKeys(name);
            driver.FindElement(By.Id("username")).SendKeys(username);
            driver.FindElement(By.Id("password")).SendKeys(password);
            if (!string.IsNullOrEmpty(role))
            {
                var roleDropdown = driver.FindElement(By.Id("role"));
                var selectElement = new SelectElement(roleDropdown);
                selectElement.SelectByText(role);
            }
            driver.FindElement(By.Id("saveUser")).Click();
        }

        private void ChangeUserRole(string username, string newRole)
        {
            driver.FindElement(By.XPath($"//tr[td/text()='{username}']//button[text()='Bewerk']")).Click();
            var roleDropdown = driver.FindElement(By.Id("role"));
            var selectElement = new SelectElement(roleDropdown);
            selectElement.SelectByText(newRole);
            driver.FindElement(By.Id("saveUser")).Click();
        }

        private void DeleteUserRole(string username, bool confirm)
        {
            driver.FindElement(By.XPath($"//tr[td/text()='{username}']//button[text()='Verwijder rol']")).Click();
            if (confirm)
                driver.FindElement(By.Id("confirmDelete")).Click();
            else
                driver.FindElement(By.Id("cancelDelete")).Click();
        }

        private IWebElement TriggerDeleteRole(string username)
        {
            driver.FindElement(By.XPath($"//tr[td/text()='{username}']//button[text()='Verwijder rol']")).Click();
            return driver.FindElement(By.Id("confirmDelete"));
        }

        private bool UserExistsWithRole(string username, string role)
        {
            try
            {
                return driver.FindElement(By.XPath($"//tr[td/text()='{username}' and td[text()='{role}']]")) != null;
            }
            catch
            {
                return false;
            }
        }

        private bool UserHasRole(string username, string role)
        {
            try
            {
                return driver.FindElement(By.XPath($"//tr[td/text()='{username}']//td[text()='{role}']")) != null;
            }
            catch
            {
                return false;
            }
        }

        private bool UserExists(string username)
        {
            try
            {
                return driver.FindElement(By.XPath($"//tr[td/text()='{username}']")) != null;
            }
            catch
            {
                return false;
            }
        }

        private void ApplyRoleFilter(string role)
        {
            var dropdown = driver.FindElement(By.Id("filterRole"));
            var selectElement = new SelectElement(dropdown);
            selectElement.SelectByText(role);
        }

        private void ResetRoleFilter()
        {
            driver.FindElement(By.Id("resetFilter")).Click();
        }

        private List<(string Username, List<string> Roles)> GetAllUsers()
        {
            var list = new List<(string, List<string>)>();
            var rows = driver.FindElements(By.CssSelector("table#userTable tbody tr"));
            foreach (var row in rows)
            {
                var username = row.FindElement(By.CssSelector("td.username")).Text;
                var role = row.FindElement(By.CssSelector("td.role")).Text;
                list.Add((username, new List<string> { role }));
            }
            return list;
        }

        private List<(string Username, List<string> Roles)> GetDisplayedUsers()
        {
            return GetAllUsers();
        }
    }
}
