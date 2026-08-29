using UnityEngine;
using System.Collections;

public class DayInit : MonoBehaviour
{
    void Start()
    {
        if (DatabaseManager.Instance.needsRespawn)
        {
            StartCoroutine(ClearRespawnFlag());
        }
    }

    IEnumerator ClearRespawnFlag()
    {
        // Wait until the very end of the frame
        yield return new WaitForEndOfFrame();

        DatabaseManager.Instance.needsRespawn = false;
        Debug.Log("All resources spawned. Resetting respawn flag for the day.");
    }
}