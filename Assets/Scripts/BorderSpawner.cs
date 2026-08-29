using System.Collections.Generic;
using UnityEngine;

public class BorderSpawner : MonoBehaviour
{
    [Header("Tree Prefabs (Decorative Only)")]
    public List<GameObject> borderTreePrefabs;

    [Header("Dimensions")]
    public Vector2 totalSize = new Vector2(40, 50);
    public Vector2 playableSize = new Vector2(20, 30);

    [Header("Exit Gap Settings")]
    public float exitWidth = 4f;

    [Range(0.1f, 2f)]
    public float density = 0.8f; // How close trees are to each other

    void Start()
    {
        SpawnBorder();
    }

    void SpawnBorder()
    {
        float step = 1f / density;

        for (float x = -totalSize.x / 2; x <= totalSize.x / 2; x += step)
        {
            for (float y = -totalSize.y / 2; y <= totalSize.y / 2; y += step)
            {
                // Check Inner Playable Zone
                bool inPlayableX = x > -playableSize.x / 2 && x < playableSize.x / 2;
                bool inPlayableY = y > -playableSize.y / 2 && y < playableSize.y / 2;

                if (inPlayableX && inPlayableY) continue;

                // Clear a path from the center-bottom to the edge
                // Vertical strip the width of your exitWidth
                bool inPathX = x > -exitWidth / 2 && x < exitWidth / 2;
                bool inPathY = y < 0; // Clears everything in the bottom half within that X range

                if (inPathX && inPathY) continue;

                // Spawn decorative tree with slight randomness
                Vector3 pos = transform.position + new Vector3(
                    x + Random.Range(-0.4f, 0.4f),
                    y + Random.Range(-0.4f, 0.4f),
                    0
                );

                GameObject prefab = borderTreePrefabs[Random.Range(0, borderTreePrefabs.Count)];
                Instantiate(prefab, pos, Quaternion.identity, this.transform);
            }
        }
    }
}