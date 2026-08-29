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

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

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

                // Initialize gold at 0 if the table is new
                command.CommandText = "INSERT OR IGNORE INTO PlayerStats (id, gold) VALUES (1, 0);";
                command.ExecuteNonQuery();
            }
        }
        Debug.Log("Database initialized and Tables created!");
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

    public List<InventoryItem> GetInventoryItems(string searchFilter = "", bool sortByQuantity = false)
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

        // APPLY SEARCH (LINQ)
        if (!string.IsNullOrEmpty(searchFilter))
        {
            items = items.Where(i => i.ItemName.ToLower().Contains(searchFilter.ToLower())).ToList();
        }

        // APPLY SORT (LINQ)
        if (sortByQuantity)
            return items.OrderByDescending(i => i.Quantity).ToList();
        else
            return items.OrderBy(i => i.ItemName).ToList();
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
}
