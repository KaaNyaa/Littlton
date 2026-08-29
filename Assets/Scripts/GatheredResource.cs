using System.Collections;
using UnityEngine;

public class GatheredResource : MonoBehaviour
{
    [Header("Fall & Wait Settings")]
    [SerializeField] private float fallDuration = 0.5f;   // How long it takes to hit the ground
    [SerializeField] private float fallDistance = 1.0f;   // How far it falls from its spawn point
    [SerializeField] private float waitOnGroundDuration = 1.0f; // Delay before vacuum starts

    [Header("Fly to Player Settings")]
    [SerializeField] private float flySpeed = 5.0f;
    [SerializeField] private float acceleration = 0.2f;   // Increase speed as it gets closer

    // State management
    private bool isFlyingToPlayer = false;
    private Transform playerTransform;
    public string itemName;

    private void Start()
    {
        // Start the visual fall animation
        StartCoroutine(ExecuteGatherCycle());
    }

    private IEnumerator ExecuteGatherCycle()
    {
        // Simulate falling to the ground
        Vector3 spawnPosition = transform.position;
        Vector3 targetGroundPosition = spawnPosition - new Vector3(0, fallDistance, 0);
        float elapsedFall = 0f;

        while (elapsedFall < fallDuration)
        {
            transform.position = Vector3.Lerp(spawnPosition, targetGroundPosition, elapsedFall / fallDuration);
            elapsedFall += Time.deltaTime;
            yield return null;
        }
        transform.position = targetGroundPosition; // Final snap to "ground"

        // Wait on the ground
        yield return new WaitForSeconds(waitOnGroundDuration);

        // Identify the player and start the vacuum
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            isFlyingToPlayer = true;
        }
        else
        {
            // If player cannot be found, the log just stays there
            Debug.LogError("GatheredResource cannot find the Player tag!");
        }
    }

    private void Update()
    {
        // Use Update for smooth movement rather than Coroutines for the vacuum
        if (isFlyingToPlayer && playerTransform != null)
        {
            // Move smoothly towards the player
            float step = flySpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, step);

            // Accelerate as you get closer
            flySpeed += acceleration;
        }
    }

    // Handle contact (Collision Detection)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Delete only if resources are already in the vacuum phase.
        if (isFlyingToPlayer && other.CompareTag("Player"))
        {
            DatabaseManager.Instance.AddOrUpdateItem(itemName, 1, "Resource");

            Destroy(gameObject);
        }
    }
}