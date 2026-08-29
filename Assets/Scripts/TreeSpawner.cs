using System.Collections.Generic;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    [Header("Tree Prefabs")]
    public List<GameObject> treePrefabs;

    [Header("Spawn Settings")]
    public int totalTrees = 25;
    public Vector2 areaSize = new Vector2(20, 30);
    public LayerMask blockingLayer;

    void Start()
    {
        // Door Trigger will handle the entry logic,
        RespawnTrees();
    }

    public void RespawnTrees()
    {
        // Clean up stumps / left over trees from yesterday
        foreach (Transform child in transform) { Destroy(child.gameObject); }

        for (int i = 0; i < totalTrees; i++)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                Random.Range(-areaSize.y / 2, areaSize.y / 2)
            );

            Vector3 spawnPoint = transform.position + (Vector3)randomPos;

            // 0.8f radius to keep the clearing from looking too cluttered
            if (!Physics2D.OverlapCircle(spawnPoint, 0.8f, blockingLayer))
            {
                if (treePrefabs.Count > 0)
                {
                    GameObject randomTree = treePrefabs[Random.Range(0, treePrefabs.Count)];
                    Instantiate(randomTree, spawnPoint, Quaternion.identity, this.transform);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 0));
    }
}