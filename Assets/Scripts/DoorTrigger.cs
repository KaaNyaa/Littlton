using UnityEngine;
using UnityEngine.InputSystem;

public class DoorTrigger : MonoBehaviour
{
    public string targetScene;
    [Tooltip("If checked, walking into the trigger will teleport immediately.")]
    public bool isAutomatic = false;

    private bool canEnter = false;

    private void Update()
    {
        // Only listen for 'E' if it's NOT an automatic door
        if (!isAutomatic && canEnter && Keyboard.current.eKey.wasPressedThisFrame)
        {
            EnterDoor();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canEnter = true;

            // If it is an automatic door, just go!
            if (isAutomatic)
            {
                EnterDoor();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) canEnter = false;
    }

    // Helper method so we don't repeat the scene change code
    private void EnterDoor()
    {
        SceneChanger.Instance.MoveToScene(targetScene);
    }
}