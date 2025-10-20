using System;
using System.Data;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using UiTests.Pages;
using UiTests.Utils;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace UiTests
{
    [TestFixture]
    public class Patient4Tests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private MyPrescriptionsPage prescriptionsPage;
        private MySqlDatabaseHelper _dbHelper;
        private readonly string baseUrl = "https://localhost:7059";

        // ---------- DATABASE SETUP ----------

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            // Start ChromeDriver
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();

            // Init pages
            loginPage = new LoginPage(driver, baseUrl);

            // DB helper
            _dbHelper = new MySqlDatabaseHelper();

            Console.WriteLine("Seeding test users...");

            try
            {
                _dbHelper.ExecuteQuery(@"
            INSERT INTO users (Username, PasswordHash, Email, CreatedAt, UserRoleID, PhoneNumber)
            VALUES
            ('jjansen','fgW/pr3lK9QFtmUdvKsR3rgW+R81PLzQNPJUh7Nd0Pk=','jan.jansen@example.com',NOW(),4,'0612345671'),
            ('pietp','KgR12Xl0MGYj/gVeoTxBpgGrqmIt+bJ9xP855vCcSdk=','pietp@example.com',NOW(),4,'0612345672'),
            ('lisavd','KgR12Xl0MGYj/gVeoTxBpgGrqmIt+bJ9xP855vCcSdk=','vandam@example.com',NOW(),4,'0612345673'),
            ('huisarts1','gbwVEPsPR1Vsp8xDqDgFfQIT7ajvEgY14dPnOnr7dF0=','huisarts1@example.com',NOW(),5,'0612345674'),
            ('specialist1','gbwVEPsPR1Vsp8xDqDgFfQIT7ajvEgY14dPnOnr7dF0=','specialist1@example.com',NOW(),6,'0612345675');
        ");
                Console.WriteLine("Test users inserted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to seed test users!");
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        [OneTimeTearDown]
        public void GlobalCleanup()
        {
            Console.WriteLine("Cleaning up test users...");

            try
            {
                _dbHelper.ExecuteQuery(@"
            DELETE FROM users 
            WHERE Username IN ('jjansen','pietp','lisavd','huisarts1','specialist1');
        ");
                Console.WriteLine("Test users deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cleanup error:");
                Console.WriteLine(ex.Message);
            }

            driver.Quit();
        }


        // ---------- HELPERS ----------

        private void LoginAs(string username, string password)
        {
            loginPage.GoTo();
            loginPage.Login(username, password);

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Url.Contains("/Prescriptions/MyPrescriptions"));
        }

        // ---------- TESTS ----------

        [Test]
        public void VerifySeededUsersExist()
        {
            var result = _dbHelper.ExecuteQuery(
                "SELECT COUNT(*) AS userCount FROM users WHERE Username IN ('jjansen','pietp','lisavd','huisarts1','specialist1');"
            );

            Assert.AreEqual(5, Convert.ToInt32(result.Rows[0]["userCount"]),
                "Not all test users were added to DB.");
            Console.WriteLine("DB test ran successfully: " + result.Rows[0]["userCount"]);
        }

        [Test]
        public void TC4_1_1_Login_Success()
        {
            LoginAs("jjansen", "Wachtw123");
            Assert.IsTrue(driver.Url.Contains("/Prescriptions/MyPrescriptions"));
            Assert.IsTrue(driver.PageSource.Contains("Logged in:"), "Expected logged-in state.");
        }

        [Test]
        public void TC4_1_2_Login_WrongPassword()
        {
            loginPage.GoTo();
            loginPage.Login("jjansen", "Wachtw12");
            Assert.AreEqual("Username/password not recognized", loginPage.GetErrorMessage());
        }

        [Test]
        public void TC4_1_3_Login_UnknownUser()
        {
            loginPage.GoTo();
            loginPage.Login("unknownuser", "Wachtw123");
            Assert.AreEqual("User not found", loginPage.GetErrorMessage());
        }

        [Test]
        public void TC4_1_4_Login_EmptyUsername()
        {
            loginPage.GoTo();
            var validationMessage = loginPage.GetUsernameValidationMessage();
            Assert.IsTrue(validationMessage.Contains("Please fill out this field"));
        }

        [Test]
        public void TC4_1_5_Login_EmptyPassword()
        {
            loginPage.GoTo();
            var validationMessage = loginPage.GetPasswordValidationMessage();
            Assert.IsTrue(validationMessage.Contains("Please fill out this field"));
        }

        [Test]
        public void TC4_1_6_Login_Redirect()
        {
            LoginAs("jjansen", "Wachtw123");
            Assert.IsTrue(driver.Url.Contains("/MyPrescriptions"));
        }

        [Test]
        public void TC4_2_1_OwnMedicationVisible()
        {
            LoginAs("pietp", "Wachtwoord2");
            Assert.IsTrue(prescriptionsPage.ContainsMedication("Amoxicillin"));
        }

        [Test]
        public void TC4_2_2_NoAccessOtherPatients()
        {
            LoginAs("pietp", "Wachtwoord2");
            Assert.IsFalse(prescriptionsPage.CanSeeOtherPatientData(7));
        }

        [Test]
        public void TC4_2_3_NoActiveMedication()
        {
            LoginAs("lisavd", "Wachtwoord2");
            Assert.IsTrue(prescriptionsPage.HasNoPrescriptions());
        }

        // ---------- DISABLED TESTS (no pages yet) ----------

        [Test, Ignore("ForgotPasswordPage not implemented")]
        public void TC4_3_1_ResetPassword_HappyPath() { }

        [Test, Ignore("ForgotPasswordPage not implemented")]
        public void TC4_3_2_ResetPassword_InvalidEmail() { }

        [Test, Ignore("ResetPasswordPage not implemented")]
        public void TC4_3_3_ResetLinkExpired() { }

        [Test, Ignore("ResetPasswordPage not implemented")]
        public void TC4_3_4_ResetPassword_WeakPassword() { }

        [Test, Ignore("ResetPasswordPage not implemented")]
        public void TC4_3_5_ResetPassword_LoginWithNew() { }
    }
}
