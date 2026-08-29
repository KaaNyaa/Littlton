using UnityEngine;
using UnityEngine.InputSystem;

public class MerchantCounter : MonoBehaviour
{
    [Header("Settings")]
    public float woodValue = 5f;
    public float stoneValue = 10f;

    [Header("UI")]
    public GameObject interactPrompt; // A simple "Press E to Sell" text object

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SellItems();
        }
    }

    private void SellItems()
    {
        // Get current counts from Database
        int woodCount = DatabaseManager.Instance.GetItemCount("Wood");
        int stoneCount = DatabaseManager.Instance.GetItemCount("Stone");

        if (woodCount <= 0 && stoneCount <= 0)
        {
            Debug.Log("You have nothing to sell!");
            return;
        }

        // Calculate Total Gold
        float totalEarned = (woodCount * woodValue) + (stoneCount * stoneValue);

        // Update Database
        DatabaseManager.Instance.ProcessSale(0);

        // Save the gold to the persistent PlayerStats table
        DatabaseManager.Instance.AddGold(totalEarned);

        // Refresh UI
        // Clears the x0 icons and updates the inventory view
        PlayerUIController.Instance.RefreshUI("");

        Debug.Log($"Sold everything for {totalEarned} gold! Current Total: {DatabaseManager.Instance.GetTotalGold()}");
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