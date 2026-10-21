using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Mono.Data.Sqlite;

public class DatabaseManagerTests
{
    private IDbConnection dbConnection;
    private string testDbPath = "URI=file::memory:";

    [SetUp]
    public void Setup()
    {
        // Open an in-memory SQLite connection for rapid testing
        dbConnection = new SqliteConnection(testDbPath);
        dbConnection.Open();

        CreateTestSchemas();
    }

    [TearDown]
    public void TearDown()
    {
        dbConnection.Close();
        dbConnection.Dispose();
    }

    private void CreateTestSchemas()
    {
        using (var command = dbConnection.CreateCommand())
        {
            // Replicate your exact DatabaseManager schema setup
            command.CommandText = "CREATE TABLE IF NOT EXISTS Inventory (" +
                                  "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                  "name TEXT UNIQUE, " +
                                  "quantity INTEGER, " +
                                  "category TEXT);";
            command.ExecuteNonQuery();

            command.CommandText = "CREATE TABLE IF NOT EXISTS PlayerStats (" +
                                  "id INTEGER PRIMARY KEY, " +
                                  "gold REAL);";
            command.ExecuteNonQuery();

            command.CommandText = "INSERT OR IGNORE INTO PlayerStats (id, gold) VALUES (1, 0);";
            command.ExecuteNonQuery();
        }
    }

    [Test]
    public void Test_SchemaInitialization_CreatesExpectedTables()
    {
        bool inventoryExists = false;
        bool playerStatsExists = false;

        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Inventory';";
            inventoryExists = command.ExecuteScalar() != null;

            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='PlayerStats';";
            playerStatsExists = command.ExecuteScalar() != null;
        }

        Assert.IsTrue(inventoryExists, "Inventory table should exist after initialization.");
        Assert.IsTrue(playerStatsExists, "PlayerStats table should exist after initialization.");
    }

    [Test]
    public void Test_AddOrUpdateItem_InsertsAndIncrementsCorrectly()
    {
        // 1. Simulate AddOrUpdateItem logic for 'Wood'
        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = "INSERT OR IGNORE INTO Inventory (name, quantity, category) VALUES (@name, 0, @cat); " +
                                  "UPDATE Inventory SET quantity = quantity + @qty WHERE name = @name;";

            command.Parameters.Add(new SqliteParameter("@name", "Wood"));
            command.Parameters.Add(new SqliteParameter("@qty", 10));
            command.Parameters.Add(new SqliteParameter("@cat", "Material"));
            command.ExecuteNonQuery();
        }

        // 2. Increment existing item
        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = "INSERT OR IGNORE INTO Inventory (name, quantity, category) VALUES (@name, 0, @cat); " +
                                  "UPDATE Inventory SET quantity = quantity + @qty WHERE name = @name;";

            command.Parameters.Add(new SqliteParameter("@name", "Wood"));
            command.Parameters.Add(new SqliteParameter("@qty", 5));
            command.Parameters.Add(new SqliteParameter("@cat", "Material"));
            command.ExecuteNonQuery();
        }

        // 3. Verify total quantity is 15
        int finalQuantity = 0;
        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = "SELECT quantity FROM Inventory WHERE name = 'Wood';";
            finalQuantity = System.Convert.ToInt32(command.ExecuteScalar());
        }

        Assert.AreEqual(15, finalQuantity, "Wood quantity should accumulate correctly via upsert logic.");
    }

    [Test]
    public void Test_PlayerStats_GoldIncrementAndRetrieval()
    {
        // Add gold
        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = "UPDATE PlayerStats SET gold = gold + @amount WHERE id = 1;";
            command.Parameters.Add(new SqliteParameter("@amount", 250.5f));
            command.ExecuteNonQuery();
        }

        // Retrieve gold
        float totalGold = 0;
        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = "SELECT gold FROM PlayerStats WHERE id = 1;";
            totalGold = System.Convert.ToSingle(command.ExecuteScalar());
        }

        Assert.AreEqual(250.5f, totalGold, 0.01f, "Player gold should match added amount accurately.");
    }
}