using System;
using System.Data;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using UiTests.Pages;
using UiTests.Utils;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace UiTests
{
    [TestFixture]
    public class Patient5Tests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private MyPrescriptionsPage prescriptionsPage;
        private MySqlDatabaseHelper _dbHelper;
        private readonly string baseUrl = "http://localhost:5070";

        // ---------- DATABASE SETUP ----------

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            _dbHelper = new MySqlDatabaseHelper();
            Console.WriteLine("🧹 Cleaning up old test data before seeding...");
            try
            {
                _dbHelper.ExecuteQuery("DELETE FROM prescription_medicines;");
                _dbHelper.ExecuteQuery("DELETE FROM prescriptions;");
                _dbHelper.ExecuteQuery("DELETE FROM users WHERE Username IN ('jjansen','pietp','lisavd','huisarts1','specialist1');");
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Cleanup before seeding failed (ignored if tables were empty): " + ex.Message);
            }

            Console.WriteLine("🌱 Seeding test users and prescriptions...");

            try
            {
                // --- USERS ---
                _dbHelper.ExecuteQuery(@"
                    INSERT INTO users (Username, PasswordHash, Email, CreatedAt, UserRoleID, PhoneNumber)
                    VALUES ('jjansen','fgW/pr3lK9QFtmUdvKsR3rgW+R81PLzQNPJUh7Nd0Pk=','jan.jansen@example.com',NOW(),4,'0612345671');
                ");
                _dbHelper.ExecuteQuery(@"
                    INSERT INTO users (Username, PasswordHash, Email, CreatedAt, UserRoleID, PhoneNumber)
                    VALUES ('pietp','KgR12Xl0MGYj/gVeoTxBpgGrqmIt+bJ9xP855vCcSdk=','pietp@example.com',NOW(),4,'0612345672');
                ");
                _dbHelper.ExecuteQuery(@"
                    INSERT INTO users (Username, PasswordHash, Email, CreatedAt, UserRoleID, PhoneNumber)
                    VALUES ('lisavd','KgR12Xl0MGYj/gVeoTxBpgGrqmIt+bJ9xP855vCcSdk=','vandam@example.com',NOW(),4,'0612345673');
                ");
                _dbHelper.ExecuteQuery(@"
                    INSERT INTO users (Username, PasswordHash, Email, CreatedAt, UserRoleID, PhoneNumber)
                    VALUES ('huisarts1','3pm5LNMy73MeDa5blZORfMk9ZlxsNICSjVfPWFj7YfU==','huisarts1@example.com',NOW(),5,'0612345674');
                ");
                _dbHelper.ExecuteQuery(@"
                    INSERT INTO users (Username, PasswordHash, Email, CreatedAt, UserRoleID, PhoneNumber)
                    VALUES ('specialist1','3pm5LNMy73MeDa5blZORfMk9ZlxsNICSjVfPWFj7YfU==','specialist1@example.com',NOW(),6,'0612345675');
                ");

                // --- PRESCRIPTIONS ---

                // Prescription 1: jjansen by huisarts1
                var res1 = _dbHelper.ExecuteQuery(@"
                    SELECT p.UserID AS patientID, d.UserID AS prescriberID
                    FROM users p JOIN users d ON d.Username='huisarts1'
                    WHERE p.Username='jjansen';
                ");
                int patient1 = Convert.ToInt32(res1.Rows[0]["patientID"]);
                int prescriber1 = Convert.ToInt32(res1.Rows[0]["prescriberID"]);

                _dbHelper.ExecuteQuery($@"
                    INSERT INTO prescriptions (patientID, prescriberID, prescription_date, prescription_start_date, prescription_end_date, description)
                    VALUES ({patient1}, {prescriber1}, '2025-10-01','2025-10-02','2025-10-08','3x per day');
                ");

                var presc1 = _dbHelper.ExecuteQuery("SELECT LAST_INSERT_ID() AS id;");
                int prescId1 = Convert.ToInt32(presc1.Rows[0]["id"]);
                _dbHelper.ExecuteQuery($@"
                    INSERT INTO prescription_medicines (prescriptionID, medicineID, quantity, instructions)
                    VALUES ({prescId1}, 1, 10, 'Dissolve in water');
                ");

                // Prescription 2: jjansen by specialist1
                var res2 = _dbHelper.ExecuteQuery(@"
                    SELECT p.UserID AS patientID, d.UserID AS prescriberID
                    FROM users p JOIN users d ON d.Username='specialist1'
                    WHERE p.Username='jjansen';
                ");
                int patient2 = Convert.ToInt32(res2.Rows[0]["patientID"]);
                int prescriber2 = Convert.ToInt32(res2.Rows[0]["prescriberID"]);

                _dbHelper.ExecuteQuery($@"
                    INSERT INTO prescriptions (patientID, prescriberID, prescription_date, prescription_start_date, prescription_end_date, description)
                    VALUES ({patient2}, {prescriber2}, '2025-09-25','2025-09-26','2025-10-02','2x per day');
                ");

                var presc2 = _dbHelper.ExecuteQuery("SELECT LAST_INSERT_ID() AS id;");
                int prescId2 = Convert.ToInt32(presc2.Rows[0]["id"]);
                _dbHelper.ExecuteQuery($@"
                    INSERT INTO prescription_medicines (prescriptionID, medicineID, quantity, instructions)
                    VALUES ({prescId2}, 2, 3, 'Swallow whole');
                ");

                // Prescription 3: pietp by huisarts1
                var res3 = _dbHelper.ExecuteQuery(@"
                    SELECT p.UserID AS patientID, d.UserID AS prescriberID
                    FROM users p JOIN users d ON d.Username='huisarts1'
                    WHERE p.Username='pietp';
                ");
                int patient3 = Convert.ToInt32(res3.Rows[0]["patientID"]);
                int prescriber3 = Convert.ToInt32(res3.Rows[0]["prescriberID"]);

                _dbHelper.ExecuteQuery($@"
                    INSERT INTO prescriptions (patientID, prescriberID, prescription_date, prescription_start_date, prescription_end_date, description)
                    VALUES ({patient3}, {prescriber3}, '2025-10-03','2025-10-04','2025-10-10','3x per day');
                ");

                var presc3 = _dbHelper.ExecuteQuery("SELECT LAST_INSERT_ID() AS id;");
                int prescId3 = Convert.ToInt32(presc3.Rows[0]["id"]);
                _dbHelper.ExecuteQuery($@"
                    INSERT INTO prescription_medicines (prescriptionID, medicineID, quantity, instructions)
                    VALUES ({prescId3}, 3, 3, 'Swallow whole');
                ");

                // Prescription 4: pietp by specialist1
                var res4 = _dbHelper.ExecuteQuery(@"
                    SELECT p.UserID AS patientID, d.UserID AS prescriberID
                    FROM users p JOIN users d ON d.Username='specialist1'
                    WHERE p.Username='pietp';
                ");
                int patient4 = Convert.ToInt32(res4.Rows[0]["patientID"]);
                int prescriber4 = Convert.ToInt32(res4.Rows[0]["prescriberID"]);

                _dbHelper.ExecuteQuery($@"
                    INSERT INTO prescriptions (patientID, prescriberID, prescription_date, prescription_start_date, prescription_end_date, description)
                    VALUES ({patient4}, {prescriber4}, '2025-10-05','2025-10-06','2025-10-12','2x per day');
                ");

                var presc4 = _dbHelper.ExecuteQuery("SELECT LAST_INSERT_ID() AS id;");
                int prescId4 = Convert.ToInt32(presc4.Rows[0]["id"]);
                _dbHelper.ExecuteQuery($@"
                    INSERT INTO prescription_medicines (prescriptionID, medicineID, quantity, instructions)
                    VALUES ({prescId4}, 4, 3, 'Swallow whole');
                ");

                Console.WriteLine("✅ Seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Seeding failed: " + ex.Message);
                throw;
            }
        }

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            loginPage = new LoginPage(driver, baseUrl);
            prescriptionsPage = new MyPrescriptionsPage(driver, baseUrl);
            loginPage.GoTo();
        }

        [TearDown]
        public void Cleanup()
        {
            driver.Quit();
        }

        [OneTimeTearDown]
        public void GlobalCleanup()
        {
            Console.WriteLine("🧹 Cleaning up database...");
            try
            {
                _dbHelper.ExecuteQuery("DELETE FROM prescription_medicines;");
                _dbHelper.ExecuteQuery("DELETE FROM prescriptions;");
                _dbHelper.ExecuteQuery("DELETE FROM users WHERE Username IN ('jjansen','pietp','lisavd','huisarts1','specialist1');");
                Console.WriteLine("✅ Database cleanup successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Cleanup failed: " + ex.Message);
            }
        }

        // ---------- TESTS ----------

        // ---------- AC5.1 ----------

        [Test]
        public void TC5_1_1_MedicationOverview_HappyPath()
        {
            loginPage.Login("jjansen", "Wachtwoord2");
            prescriptionsPage.GoTo();

            Assert.IsTrue(prescriptionsPage.ContainsMedication("Paracetamol 500mg"));
            Assert.IsTrue(prescriptionsPage.ContainsMedication("Omeprazol 20mg"));
        }

        [Test]
        public void TC5_1_2_MultipleIntakeTimes()
        {
            loginPage.Login("jjansen", "Wachtwoord2");
            prescriptionsPage.GoTo();

            Assert.IsTrue(prescriptionsPage.ContainsMedication("Metformine 500mg"));
        }

        [Test]
        public void TC5_1_3_WrongData_Negative()
        {
            loginPage.Login("jjansen", "Wachtwoord2");
            prescriptionsPage.GoTo();

            Assert.IsTrue(prescriptionsPage.HasError("Medicatiegegevens tijdelijk niet beschikbaar"));
        }

        [Test]
        public void TC5_1_4_SystemError()
        {
            loginPage.Login("jjansen", "Wachtwoord2");
            prescriptionsPage.GoTo();

            Assert.IsTrue(prescriptionsPage.HasError("Er is een technische fout opgetreden. Probeer later opnieuw."));
        }

        // ---------- AC5.2 ----------

        [Test]
        public void TC5_2_1_NoMedication_HappyPath()
        {
            loginPage.Login("lisavd", "Wachtwoord2");
            prescriptionsPage.GoTo();

            Assert.IsTrue(prescriptionsPage.HasNoPrescriptions());
        }

        [Test]
        public void TC5_2_2_ExpiredMedicationNotVisible()
        {
            loginPage.Login("lisavd", "Wachtwoord2");
            prescriptionsPage.GoTo();

            Assert.IsTrue(prescriptionsPage.HasNoPrescriptions());
        }

        [Test]
        public void TC5_2_3_TechnicalErrorEmptyList()
        {
            loginPage.Login("lisavd", "Wachtwoord2");
            prescriptionsPage.GoTo();

            Assert.IsTrue(prescriptionsPage.HasError("Kan medicatiegegevens niet ophalen. Probeer later opnieuw."));
        }

        // ---------- AC5.3 ----------

        [Test]
        public void TC5_3_1_NewMedicationVisibleAfterRefresh()
        {
            loginPage.Login("jjansen", "Wachtwoord2");
            prescriptionsPage.GoTo();

            prescriptionsPage.Refresh();
            Assert.IsTrue(prescriptionsPage.ContainsMedication("Amoxicilline 500mg"));
        }

        [Test]
        public void TC5_3_2_LiveUpdateWithoutRefresh()
        {
            loginPage.Login("jjansen", "Wachtwoord2");
            prescriptionsPage.GoTo();

            // Hypothetical wait for live sync
            System.Threading.Thread.Sleep(5000);

            Assert.IsTrue(prescriptionsPage.ContainsMedication("Amoxicilline 500mg"));
        }

        [Test]
        public void TC5_3_3_NewMedicationNotVisible_Negative()
        {
            loginPage.Login("jjansen", "Wachtwoord2");
            prescriptionsPage.GoTo();

            Assert.IsTrue(prescriptionsPage.HasError("Medicatiegegevens niet up-to-date. Probeer opnieuw te laden."));
        }

        [Test]
        public void TC5_3_4_MultipleNewMedications()
        {
            loginPage.Login("jjansen", "Wachtwoord2");
            prescriptionsPage.GoTo();

            prescriptionsPage.Refresh();
            Assert.IsTrue(prescriptionsPage.ContainsMedication("Amoxicilline 500mg"));
            Assert.IsTrue(prescriptionsPage.ContainsMedication("Ciprofloxacin"));
        }

        [Test]
        public void TC5_3_5_OldMedicationRemoved()
        {
            loginPage.Login("jjansen", "Wachtwoord2");
            prescriptionsPage.GoTo();

            prescriptionsPage.Refresh();
            Assert.IsFalse(prescriptionsPage.ContainsMedication("Paracetamol 250mg")); // expired
        }
    }
}
