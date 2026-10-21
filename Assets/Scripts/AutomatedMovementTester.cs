using System.Collections;
using UnityEngine;

public class AutomatedMovementTester : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    private Rigidbody2D rb;

    [Header("Test Settings")]
    public bool isTesting = true;
    public float moveDuration = 2.0f;
    public float pauseDuration = 0.5f;

    [Header("Reset Settings")]
    private Vector2 startPosition;

    void Start()
    {
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (playerMovement != null)
        {
            rb = playerMovement.GetComponent<Rigidbody2D>();
            startPosition = rb.position; // Capture initial placement in the scene
        }

        if (isTesting && playerMovement != null)
        {
            StartCoroutine(RunInfiniteMovementLoop());
        }
    }

    IEnumerator RunInfiniteMovementLoop()
    {
        while (isTesting)
        {
            // Reset state before each run
            ResetPlayerState();
            yield return new WaitForSeconds(0.5f); // pause at start

            Debug.Log(" Starting Test Cycle. . . ");

            // Test Moving Right
            SetMovement(new Vector2(1, 0));
            yield return new WaitForSeconds(moveDuration);

            // Test Dash Right
            TriggerDash();
            yield return new WaitForSeconds(0.5f);

            // Test Dashing While Cooldown is Active (Will fail/ignore)
            Debug.Log("Testing: Dash on Cooldown (Is blocked)");
            TriggerDash();
            yield return new WaitForSeconds(1.0f);

            // Test Moving Up-Right (Diagonal)
            SetMovement(new Vector2(1, 1).normalized);
            yield return new WaitForSeconds(moveDuration);

            // Test Moving Left
            SetMovement(new Vector2(-1, 0));
            yield return new WaitForSeconds(moveDuration);

            // Stop Movement
            SetMovement(Vector2.zero);
            yield return new WaitForSeconds(pauseDuration);

            // Test Dash with Zero Input (Will fail/ignore)
            TriggerDash();
            yield return new WaitForSeconds(1.0f);

            Debug.Log("Test Cycle Complete. Resetting position. . . ");
        }
    }

    void SetMovement(Vector2 dir)
    {
        playerMovement.SetAutomatedMovement(dir);
    }

    void TriggerDash()
    {
        playerMovement.TriggerAutomatedDash();
    }

    void ResetPlayerState()
    {
        // Stop all movement inputs
        SetMovement(Vector2.zero);

        // Instantly teleport back to starting point
        if (rb != null)
        {
            rb.position = startPosition;
            rb.linearVelocity = Vector2.zero; // Clear momentum to avoid drifting
        }
    }
}