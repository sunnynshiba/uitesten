using System;
using UiTests.Utils; // Replace with your namespace where MySqlDatabaseHelper is defined
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace UiTests.Tests;

[TestFixture]
public class MySqlDatabaseHelperTests
{
    private MySqlDatabaseHelper _dbHelper;

    [SetUp]
    public void Setup()
    {
        _dbHelper = new MySqlDatabaseHelper();
    }

    [Test]
    public void ExecuteQuery_ShouldInsertAndDeleteUser()
    {
        string testUsername = "testuser_" + Guid.NewGuid().ToString("N");
        string testEmail = "testuser123456@example.com";
        string password = "JfQ7FIatlaE5jj7rPYO8QBABX8yb7bNbQy4AKY1QIfc=";
        int userRole = 2;
        string insertQuery = $"INSERT INTO users (username, passwordhash, email, userroleid) VALUES ('{testUsername}', '{password}','{testEmail}',{userRole});";
        string deleteQuery = $"DELETE FROM users WHERE username = '{testUsername}';";

        try
        {
            // Act
            _dbHelper.ExecuteQuery(insertQuery);

        }
        catch (Exception ex)
        {
            // Fail the test explicitly if ExecuteQuery throws
            Assert.Fail($"ExecuteQuery threw an exception: {ex.Message}");
        }
        finally
        {
            // Cleanup safely
            try
            {
                _dbHelper.ExecuteQuery(deleteQuery);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
