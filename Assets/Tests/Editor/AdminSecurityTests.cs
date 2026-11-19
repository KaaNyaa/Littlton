using System.IO;
using NUnit.Framework;
using UnityEngine;

public class AdminSecurityTests
{
    private string _tempDbPath;
    private GameObject _dbObject;
    private DatabaseManager _dbManager;

    /// <summary>
    /// Creates an isolated temporary SQLite database and DatabaseManager instance before each test
    /// so real save slots are never modified.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Generate a unique temporary database file path for test isolation
        string filePath = Path.Combine(
            Application.temporaryCachePath,
            $"test_admin_{System.Guid.NewGuid()}.db");
        _tempDbPath = "URI=file:" + filePath;

        // Attach DatabaseManager to a temporary GameObject for testing
        _dbObject = new GameObject("TestDb");
        _dbManager = _dbObject.AddComponent<DatabaseManager>();

        // Inject the temporary database path into the private _dbPath field via reflection
        var dbPathField = typeof(DatabaseManager).GetField(
            "_dbPath",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        dbPathField.SetValue(_dbManager, _tempDbPath);

        // Initialize tables (Inventory, PlayerStats, AdminConfig) and seed the default hashed PIN
        _dbManager.CreateSchema();
    }

    /// <summary>
    /// Destroys the temporary GameObject and deletes the temporary SQLite file after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (_dbObject != null)
        {
            Object.DestroyImmediate(_dbObject);
        }

        // Strip the SQLite URI prefix and remove the temporary database from disk
        string rawPath = _tempDbPath.Replace("URI=file:", string.Empty);
        if (File.Exists(rawPath))
        {
            File.Delete(rawPath);
        }
    }

    /// <summary>
    /// Verifies that hashing the same PIN twice generates unique random salts and distinct hashes
    /// (preventing rainbow-table attacks), while both still validate against the original PIN.
    /// </summary>
    [Test]
    public void CreateSaltedHash_DifferentSaltsProduceDifferentHashes()
    {
        string hashOne = AdminSecurityService.CreateSaltedHash("1234", out string saltOne);
        string hashTwo = AdminSecurityService.CreateSaltedHash("1234", out string saltTwo);

        Assert.AreNotEqual(saltOne, saltTwo);
        Assert.AreNotEqual(hashOne, hashTwo);
        Assert.IsTrue(AdminSecurityService.VerifyPin("1234", hashOne, saltOne));
        Assert.IsTrue(AdminSecurityService.VerifyPin("1234", hashTwo, saltTwo));
    }

    /// <summary>
    /// Verifies that database PIN authentication accepts the valid PIN, rejects incorrect PINs,
	/// and treats SQL injection payloads as literal text rather than executable SQL.
    /// </summary>
    [Test]
    public void VerifyAdminPin_ValidAndInvalidPins_BehaveAsExpected()
    {
        // Valid default PIN should authenticate
        Assert.IsTrue(_dbManager.VerifyAdminPin("1234"));

        // Incorrect PIN should be rejected
        Assert.IsFalse(_dbManager.VerifyAdminPin("0000"));

        // Classic SQL injection payload should fail safely due to parameterized queries and PBKDF2 verification
        Assert.IsFalse(_dbManager.VerifyAdminPin("' OR '1'='1"));
    }

    /// <summary>
    /// Verifies that the admin cheat setters write exact target values (9999 Gold, 99 Wood, 99 Stone)
	/// into SQLite and persist them accurately.
    /// </summary>
    [Test]
    public void CheatSetters_SetGold9999Wood99Stone99_PersistExactValues()
    {
        // Apply cheat values to the database
        _dbManager.SetGold(9999f);
        _dbManager.SetItemQuantity("Wood", 99, "Resource");
        _dbManager.SetItemQuantity("Stone", 99, "Resource");

        // Confirm persisted values match exact targets
        Assert.AreEqual(9999f, _dbManager.GetTotalGold());
        Assert.AreEqual(99, _dbManager.GetItemCount("Wood"));
        Assert.AreEqual(99, _dbManager.GetItemCount("Stone"));
    }
}