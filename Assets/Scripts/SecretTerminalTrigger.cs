using UnityEngine;
using UnityEngine.InputSystem;

public class SecretTerminalTrigger : MonoBehaviour
{
    [Header("Interaction")]
    public string playerTag = "Player";
    public GameObject promptIndicator;
    public CheatTerminalUI cheatTerminalUI;

    private bool _playerInRange = false;

    private void OnEnable()
    {
        if (promptIndicator != null)
        {
            promptIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        if (!_playerInRange || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.zKey.wasPressedThisFrame && cheatTerminalUI != null)
        {
            cheatTerminalUI.OpenTerminal();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = true;
            if (promptIndicator != null)
            {
                promptIndicator.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = false;
            if (promptIndicator != null)
            {
                promptIndicator.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = true;
            if (promptIndicator != null)
            {
                promptIndicator.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = false;
            if (promptIndicator != null)
            {
                promptIndicator.SetActive(false);
            }
        }
    }
}