using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatTerminalUI : MonoBehaviour
{
    [Header("UI Panels & Inputs")]
    public GameObject terminalRootPanel;
    public GameObject pinEntrySection;
    public GameObject cheatButtonsSection;
    public TMP_InputField pinInputField;
    public TextMeshProUGUI statusText;

    [Header("Cheat Target Values")]
    public float targetGold = 9999f;
    public int targetWood = 99;
    public int targetStone = 99;

    private bool _isOpen = false;
    private bool _isUnlocked = false;

    private void Awake()
    {
        if (terminalRootPanel != null)
        {
            terminalRootPanel.SetActive(false);
        }
        SetUnlockedState(false);
    }

    private void Update()
    {
        if (!_isOpen || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseTerminal();
        }
    }

    public void OpenTerminal()
    {
        _isOpen = true;
        if (terminalRootPanel != null)
        {
            terminalRootPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        if (pinInputField != null)
        {
            pinInputField.text = string.Empty;
            pinInputField.ActivateInputField();
        }

        if (!_isUnlocked)
        {
            SetUnlockedState(false);
            SetStatus("Enter PIN to unlock cheat menu.");
        }
        else
        {
            SetUnlockedState(true);
            SetStatus("Cheat menu unlocked.");
        }
    }

    public void CloseTerminal()
    {
        _isOpen = false;
        SetUnlockedState(false);

        if (terminalRootPanel != null)
        {
            terminalRootPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    public void SubmitPin()
    {
        string enteredPin = pinInputField != null ? pinInputField.text.Trim() : string.Empty;

        if (DatabaseManager.Instance == null)
        {
            SetStatus("DatabaseManager not found.");
            return;
        }

        bool valid = DatabaseManager.Instance.VerifyAdminPin(enteredPin);
        if (valid)
        {
            _isUnlocked = true;
            SetUnlockedState(true);
            SetStatus("PIN accepted. Cheat buttons available.");
        }
        else
        {
            _isUnlocked = false;
            SetUnlockedState(false);
            SetStatus("Invalid PIN.");
            if (pinInputField != null)
            {
                pinInputField.text = string.Empty;
                pinInputField.ActivateInputField();
            }
        }
    }

    public void SetGoldTo9999()
    {
        if (!_isUnlocked || DatabaseManager.Instance == null)
        {
            return;
        }

        DatabaseManager.Instance.SetGold(targetGold);
        RefreshOpenUI();
        SetStatus($"Gold set to {targetGold:N0}.");
    }

    public void SetWoodTo99()
    {
        if (!_isUnlocked || DatabaseManager.Instance == null)
        {
            return;
        }

        DatabaseManager.Instance.SetItemQuantity("Wood", targetWood, "Resource");
        RefreshOpenUI();
        SetStatus($"Wood set to {targetWood}.");
    }

    public void SetStoneTo99()
    {
        if (!_isUnlocked || DatabaseManager.Instance == null)
        {
            return;
        }

        DatabaseManager.Instance.SetItemQuantity("Stone", targetStone, "Resource");
        RefreshOpenUI();
        SetStatus($"Stone set to {targetStone}.");
    }

    private void SetUnlockedState(bool unlocked)
    {
        if (pinEntrySection != null)
        {
            pinEntrySection.SetActive(!unlocked);
        }
        if (cheatButtonsSection != null)
        {
            cheatButtonsSection.SetActive(unlocked);
        }
    }

    private void RefreshOpenUI()
    {
        if (PlayerUIController.Instance != null)
        {
            PlayerUIController.Instance.RefreshGoldDisplay();
            string search = PlayerUIController.Instance.searchField != null
                ? PlayerUIController.Instance.searchField.text
                : string.Empty;
            PlayerUIController.Instance.RefreshUI(search);
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}