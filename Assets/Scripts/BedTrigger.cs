using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;
using Random = UnityEngine.Random;

public class BedTrigger : MonoBehaviour
{
    // Keeping the event for triggering sounds and events later
    public static event Action OnPlayerSlept;
    private bool playerInRange = false;

    private void Update()
    {
        // Only sleep if the player is near and presses 'E'
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(SleepSequence());
        }
    }

    private IEnumerator SleepSequence()
    {
        // Black out the screen
        yield return StartCoroutine(SceneChanger.Instance.FadeOut());

        // Pause for the "Time Passing" feel
        yield return new WaitForSeconds(3f);

        // Overnight Income Calculation
        CalculateOvernightIncome();

        // Update the Master Switch in the Database
        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.needsRespawn = true;
        }

        // Fire the event (kept incase of health/stamina regeneration. may scrap later)
        OnPlayerSlept?.Invoke();

        // Un-Black out the screen
        yield return StartCoroutine(SceneChanger.Instance.FadeIn());

        Debug.Log("Morning! The world has been reset.");
    }

    private void CalculateOvernightIncome()
    {
        if (DatabaseManager.Instance == null) return;

        // Formula: G = x * (y + r)
        int x = 1;                              // Tool level (hardcoded for now)
        int y = 5;                              // Base resource yield (hardcoded for testing)
        int r = Random.Range(4, 9);             // Random number between 4 and 8 (inclusive)

        int gatheredResources = x * (y + r);

        // Convert gathered resources into gold (1 resource = 1 gold, adjust as needed)
        float earnedGold = gatheredResources;

        // Add gold to database using existing method
        DatabaseManager.Instance.AddGold(earnedGold);

        Debug.Log($"[Sleep Income] Gathered Resources (G): {gatheredResources} (x={x}, y={y}, r={r}). Added {earnedGold} Gold to inventory!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}