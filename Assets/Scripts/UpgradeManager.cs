using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeManager : MonoBehaviour
{
    [Header("Building Configuration")]
    [SerializeField] private ToolType targetToolType; // e.g., Axe for Sawmill, Pickaxe for Forge

    [Header("Upgrade Settings")]
    [SerializeField] private int upgradeCost = 100;     // Cost in gold
    [SerializeField] private int maxToolTier = 5;       // Maximum allowable upgrade level

    [Header("UI")]
    public GameObject interactPrompt; // A simple "Press E to Upgrade" text object

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryUpgradeTool();
        }
    }

    private void TryUpgradeTool()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager instance not found in the scene.");
            return;
        }

        if (DatabaseManager.Instance == null)
        {
            Debug.LogWarning("DatabaseManager instance not found in the scene.");
            return;
        }

        // Check if the correct tool type is equipped for this building
        if (InventoryManager.Instance.currentEquippedTool != targetToolType)
        {
            Debug.Log($"Cannot upgrade here! This station only upgrades {targetToolType}s, but you have a {InventoryManager.Instance.currentEquippedTool} equipped.");
            return;
        }

        // Check if the tool is already maxed out
        if (InventoryManager.Instance.currentToolTier >= maxToolTier)
        {
            Debug.Log($"Your {targetToolType} is already at maximum tier!");
            return;
        }

        // Check current gold from the SQLite database
        float currentGold = DatabaseManager.Instance.GetTotalGold();

        if (currentGold >= upgradeCost)
        {
            // Deduct gold from SQLite
            bool success = DatabaseManager.Instance.DeductGold(upgradeCost);

            if (success)
            {
                // Increment the tool tier in InventoryManager
                InventoryManager.Instance.currentToolTier++;
                Debug.Log($"{targetToolType} Upgraded Successfully! New Tier: {InventoryManager.Instance.currentToolTier}. Remaining Gold: {DatabaseManager.Instance.GetTotalGold()}");
            }
        }
        else
        {
            Debug.Log($"Not enough gold! You have {currentGold} gold, but need {upgradeCost} gold.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }
}