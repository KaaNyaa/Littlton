using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Data.Sqlite;
using System.Data;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;
    public bool needsRespawn = false;
    private string _dbPath;

    public string DbPath => _dbPath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Called by Main Menu
    public void Initialize(int slotNumber, bool isNewGame)
    {
        string dbName = $"SaveSlot_{slotNumber}.db";
        string path = Path.Combine(Application.persistentDataPath, dbName);

        // Use URI=file: for consistency with modern SQLite wrappers
        _dbPath = "URI=file:" + path;

        if (isNewGame && File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Wiped old save for Slot {slotNumber}");
        }

        CreateSchema();
        Debug.Log($"Database initialized for Slot {slotNumber}. Path: {_dbPath}");
    }

    public void CreateSchema()
    {
        using (var connection = new SqliteConnection(_dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Existing Inventory Table
                command.CommandText = "CREATE TABLE IF NOT EXISTS Inventory (" +
                                      "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                      "name TEXT UNIQUE, " +
                                      "quantity INTEGER, " +
                                      "category TEXT);";
                command.ExecuteNonQuery();

                // New PlayerStats Table for Gold
                command.CommandText = "CREATE TABLE IF NOT EXISTS PlayerStats (" +
                                      "id INTEGER PRIMARY KEY, " +
                                      "gold REAL);";
                command.ExecuteNonQuery();

                // Initialize gold at 500 for testing
                command.CommandText = "INSERT OR IGNORE INTO PlayerStats (id, gold) VALUES (1, 500);";
                command.ExecuteNonQuery();
            }
        }
        AdminSecurityService.EnsureDefaultPinConfigured(_dbPath);
        Debug.Log("Database initialized and Tables created!");
    }

    public bool VerifyAdminPin(string pin)
    {
        return AdminSecurityService.VerifyPinAgainstDatabase(_dbPath, pin);
    }

    public void AddOrUpdateItem(string itemName, int amount, string cat)
    {
        using (var connection = new SqliteConnection(_dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT OR IGNORE INTO Inventory (name, quantity, category) VALUES (@name, 0, @cat); " +
                                      "UPDATE Inventory SET quantity = quantity + @qty WHERE name = @name;";

                command.Parameters.Add(new SqliteParameter("@name", itemName));
                command.Parameters.Add(new SqliteParameter("@qty", amount));
                command.Parameters.Add(new SqliteParameter("@cat", cat));

                command.ExecuteNonQuery();
            }
        }
        Debug.Log($"Successfully saved {amount} {itemName} to SQL!");
    }

    public void SetItemQuantity(string itemName, int quantity, string category = "Resource")
    {
        int clampedQuantity = Mathf.Max(0, quantity);
        using (var connection = new SqliteConnection(_dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT OR IGNORE INTO Inventory (name, quantity, category) VALUES (@name, 0, @cat); " +
                                      "UPDATE Inventory SET quantity = @qty WHERE name = @name;";

                command.Parameters.Add(new SqliteParameter("@name", itemName));
                command.Parameters.Add(new SqliteParameter("@qty", clampedQuantity));
                command.Parameters.Add(new SqliteParameter("@cat", category));

                command.ExecuteNonQuery();
            }
        }
        Debug.Log($"Set {itemName} quantity to {clampedQuantity} in SQL!");
    }

    public List<InventoryItem> GetInventoryItems(
        string searchFilter = "",
        bool sortByQuantity = false,
        InventorySortOrder sortOrder = InventorySortOrder.AlphabeticalAscending)
    {
        List<InventoryItem> items = new List<InventoryItem>();

        using (var connection = new SqliteConnection(_dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT name, quantity, category FROM Inventory;";
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new InventoryItem
                        {
                            ItemName = reader.GetString(0),
                            Quantity = reader.GetInt32(1),
                            Category = reader.GetString(2)
                        });
                    }
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            items = items.Where(i => InventoryQuery.MatchesSearch(i, searchFilter)).ToList();
        }

        if (sortByQuantity)
        {
            return items.OrderByDescending(i => i.Quantity).ToList();
        }

        return InventoryQuery.SortAlphabetically(items, sortOrder);
    }

    public int GetItemCount(string name)
    {
        int count = 0;
        using (var connection = new SqliteConnection(_dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Quantity FROM Inventory WHERE name = @name";
                command.Parameters.AddWithValue("@name", name);

                var result = command.ExecuteScalar();
                if (result != null)
                {
                    count = System.Convert.ToInt32(result);
                }
            }
        }
        return count;
    }

    public void ProcessSale(float goldAmount)
    {
        using (var connection = new SqliteConnection(_dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Reset Wood and Stone quantities to 0
                command.CommandText = "UPDATE Inventory SET Quantity = 0 WHERE name IN ('Wood', 'Stone')";
                command.ExecuteNonQuery();

                Debug.Log("Database updated: Resources cleared.");
            }
        }
    }

    public void AddGold(float amount)
    {
        using (var connection = new SqliteConnection(_dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Updates the single row in our PlayerStats table
                command.CommandText = "UPDATE PlayerStats SET gold = gold + @amount WHERE id = 1;";
                command.Parameters.Add(new SqliteParameter("@amount", amount));
                command.ExecuteNonQuery();
            }
        }
        Debug.Log($"Added {amount} gold to persistence!");
    }

    public void SetGold(float amount)
    {
        float clampedGold = Mathf.Max(0f, amount);
        using (var connection = new SqliteConnection(_dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE PlayerStats SET gold = @amount WHERE id = 1;";
                command.Parameters.Add(new SqliteParameter("@amount", clampedGold));
                command.ExecuteNonQuery();
            }
        }
        Debug.Log($"Set gold to {clampedGold} in persistence!");
    }

    public float GetTotalGold()
    {
        float currentGold = 0;
        using (var connection = new SqliteConnection(_dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT gold FROM PlayerStats WHERE id = 1;";
                var result = command.ExecuteScalar();

                if (result != null)
                {
                    currentGold = System.Convert.ToSingle(result);
                }
            }
        }
        return currentGold;
    }

    public bool DeductGold(float amount)
    {
        float currentGold = GetTotalGold();
        if (currentGold >= amount)
        {
            using (var connection = new SqliteConnection(_dbPath))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE PlayerStats SET gold = gold - @amount WHERE id = 1;";
                    command.Parameters.Add(new SqliteParameter("@amount", amount));
                    command.ExecuteNonQuery();
                }
            }
            Debug.Log($"Deducted {amount} gold from persistence!");
            return true;
        }
        Debug.Log("Not enough gold in database!");
        return false;
    }
}