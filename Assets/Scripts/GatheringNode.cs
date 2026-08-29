using System.Collections;
using UnityEngine;

public class GatheringNode : Interactable
{
    [SerializeField] private string resourceName = "Wood";
    [SerializeField] private int maxHits = 3;

    [Header("Resource Settings")]
    [SerializeField] private GameObject woodPrefab; // Resource PREFAB
    [SerializeField] private int resourceAmount = 2; // How many resources spawn

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float maxTiltAngle = 1.5f;

    private int currentHits;
    private Quaternion originalRotation;
    private bool isShaking = false;

    private void Start()
    {
        // Store the original local position to avoid node drifting off
        originalRotation = transform.localRotation;
    }

    public override void Interact()
    {
        currentHits++;

        // Start the shake effect
        if (!isShaking)
        {
            StartCoroutine(ShakeTree());
        }
        Debug.Log($"Hit {gameObject.name}! ({currentHits}/{maxHits})");

        if ( currentHits == maxHits )
        {
            Collect();
        }
    }

    private IEnumerator ShakeTree()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float decay = 1.0f - (elapsed / shakeDuration);
            float tilt = Mathf.Sin(elapsed * 60f) * maxTiltAngle * decay;

            transform.localRotation = Quaternion.Euler(0, 0, tilt);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap back to exactly where we started
        transform.localRotation = originalRotation;
        isShaking = false;
    }

    private void Collect()
    {
        Debug.Log($"Spawning {resourceAmount} {resourceName}.");

        // Use the sprite's height as the spawn anchor.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != sr.sprite == null)
        {
            Debug.LogWarning("Tree is missing a sprite. Cannot spawn resources reliably.");
            return;
        }

        // Calculate the spawn point 
        float spawnYOffset = sr.sprite.bounds.extents.y; // Center to Top
        Vector3 spawnOrigin = transform.position + new Vector3(0, spawnYOffset, 0);

        // Spawn multiple resources with slight random offsets
        for (int i = 0; i < resourceAmount; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), 0, 0);
            GameObject spawnedResource = Instantiate(woodPrefab, spawnOrigin + randomOffset, Quaternion.identity);

            // In parent hierarchy for organization
            // e.g., spawnedResource.transform.SetParent(GameObject.Find("WorldItemsManager").transform);
        }

        Destroy(gameObject); 
    }
}
