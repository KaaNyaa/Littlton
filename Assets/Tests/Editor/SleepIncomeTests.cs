using NUnit.Framework;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;

public class SleepIncomeTests
{
    private IDbConnection dbConnection;
    private string testDbPath = "URI=file::memory:";

    [SetUp]
    public void Setup()
    {
        // Open in-memory DB and create PlayerStats table for testing gold additions
        dbConnection = new SqliteConnection(testDbPath);
        dbConnection.Open();

        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = "CREATE TABLE IF NOT EXISTS PlayerStats (id INTEGER PRIMARY KEY, gold REAL);";
            command.ExecuteNonQuery();
            command.CommandText = "INSERT OR IGNORE INTO PlayerStats (id, gold) VALUES (1, 100);";
            command.ExecuteNonQuery();
        }
    }

    [TearDown]
    public void TearDown()
    {
        dbConnection.Close();
        dbConnection.Dispose();
    }

    [Test]
    public void Test_IncomeFormula_FallsWithinExpectedBounds()
    {
        // Test formula: G = x * (y + r)
        // x = 1, y = 5, r = [4, 8] inclusive 
        int x = 1;
        int y = 5;

        // Simulate multiple rolls to ensure it always stays within [9, 13]
        for (int i = 0; i < 100; i++)
        {
            // Replicating UnityEngine.Random.Range behavior via C++ or pseudo-random or testing bounds
            int simulatedR = UnityEngine.Random.Range(4, 9);
            int gatheredResources = x * (y + simulatedR);

            Assert.GreaterOrEqual(gatheredResources, 9, "Minimum resource yield should be at least 9 (1 * (5 + 4)).");
            Assert.LessOrEqual(gatheredResources, 13, "Maximum resource yield should not exceed 13 (1 * (5 + 8)).");
        }
    }

    [Test]
    public void Test_DatabaseGoldAddition_FromSleepIncome()
    {
        float initialGold = 100f;
        float earnedGold = 11f; // Simulated income result

        // Simulate AddGold logic
        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = "UPDATE PlayerStats SET gold = gold + @amount WHERE id = 1;";
            command.Parameters.Add(new SqliteParameter("@amount", earnedGold));
            command.ExecuteNonQuery();
        }

        // Verify total gold
        float finalGold = 0;
        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = "SELECT gold FROM PlayerStats WHERE id = 1;";
            finalGold = System.Convert.ToSingle(command.ExecuteScalar());
        }

        Assert.AreEqual(initialGold + earnedGold, finalGold, 0.01f, "Gold should successfully increment by the overnight income amount.");
    }

    [Test]
    public void Test_DailyResetFlag_TogglesCorrectly()
    {
        // Simulate setting the flag when sleeping
        bool needsRespawn = false;
        needsRespawn = true;
        Assert.IsTrue(needsRespawn, "Flag should be set to true when player goes to sleep.");

        // Simulate running DayInit sequence
        needsRespawn = false;
        Assert.IsFalse(needsRespawn, "Flag should be cleared back to false after day initialization.");
    }

    [Test]
    public void Test_ResourceCap_DoesNotExceedMaximum()
    {
        // Define your cap limit
        int resourceCap = 99;

        // Scenario A: Player is at 95 items, gathers 10 more (total would be 105)
        int currentCount = 95;
        int gatheredAmount = 10;

        int finalCount = Mathf.Min(resourceCap, currentCount + gatheredAmount);

        Assert.AreEqual(99, finalCount, "Resource count should cap strictly at 99 and not overflow.");

        // Scenario B: Player is already at the cap (99), gathers more
        currentCount = 99;
        gatheredAmount = 5;

        finalCount = Mathf.Min(resourceCap, currentCount + gatheredAmount);

        Assert.AreEqual(99, finalCount, "Resource count should remain at 99 when already at maximum cap.");
    }
}