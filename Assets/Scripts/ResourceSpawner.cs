using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    public GameObject treePrefab;
    public int numberOfTrees = 10;
    public Vector2 spawnAreaSize = new Vector2(10, 10);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < numberOfTrees; i++)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2)
            );

            Instantiate(treePrefab, (Vector2)transform.position + randomPos, Quaternion.identity);
        }
    }

}
