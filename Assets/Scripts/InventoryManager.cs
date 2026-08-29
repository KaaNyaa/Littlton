using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Data")]
    public Dictionary<string, int> resources = new Dictionary<string, int>();

    private void Awake()
    {
        // Singleton Enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize with starting values (later load from sql)
        resources.Add("Wood", 0);
        resources.Add("Stone", 0);
    }

    public void AddResource(string resourceName, int amount)
    {
        if (resources.ContainsKey(resourceName))
        {
            resources[resourceName] += amount;
            Debug.Log($"Inventory Updated: {resourceName} = {resources[resourceName]}");

            // Where UI update will trigger
        }
        else
        {
            resources.Add(resourceName, amount);
        }
    }
}