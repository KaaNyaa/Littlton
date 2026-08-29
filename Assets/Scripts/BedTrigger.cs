using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

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
        yield return new WaitForSeconds(1.5f);

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}