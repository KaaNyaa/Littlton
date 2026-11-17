using System.Collections;
using UnityEngine;

public class GatheringNode : Interactable
{
    [SerializeField] private string resourceName = "Wood";

    [Header("Tool Requirement")]
    [SerializeField] private ToolType requiredTool; // e.g., Axe
    [SerializeField] private int requiredTier = 1;  // Minimum tier needed to harvest this node

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
        originalRotation = transform.localRotation;
    }

    public override void Interact()
    {
        if (InventoryManager.Instance == null) return;

        // Check if the correct tool type is equipped
        if (InventoryManager.Instance.currentEquippedTool != requiredTool)
        {
            Debug.Log($"Wrong tool! You need a {requiredTool}.");
            return;
        }

        // Check if the tool's tier is high enough for this node
        int playerTier = InventoryManager.Instance.currentToolTier;
        if (playerTier < requiredTier)
        {
            Debug.Log($"Tool tier too low! This node requires Tier {requiredTier}, but your tool is Tier {playerTier}.");
            return;
        }

        // Calculate dynamic max hits using your formula: 6 - tier level (minimum 1 hit)
        int dynamicMaxHits = Mathf.Max(1, 6 - playerTier);

        currentHits++;

        if (!isShaking)
        {
            StartCoroutine(ShakeTree());
        }
        Debug.Log($"Hit {gameObject.name}! ({currentHits}/{dynamicMaxHits}) - Using Tier {playerTier} tool.");

        // Destroy and collect when dynamic max hits are reached
        if (currentHits >= dynamicMaxHits)
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

        transform.localRotation = originalRotation;
        isShaking = false;
    }

    private void Collect()
    {
        Debug.Log($"Spawning {resourceAmount} {resourceName}.");

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogWarning("Tree is missing a sprite. Cannot spawn resources reliably.");
            return;
        }

        float spawnYOffset = sr.sprite.bounds.extents.y;
        Vector3 spawnOrigin = transform.position + new Vector3(0, spawnYOffset, 0);

        for (int i = 0; i < resourceAmount; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), 0, 0);
            GameObject spawnedResource = Instantiate(woodPrefab, spawnOrigin + randomOffset, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}