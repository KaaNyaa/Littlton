using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Groups")]
    [SerializeField] private GameObject pressAnyKeyGroup;
    [SerializeField] private GameObject menuButtonsGroup;
    [SerializeField] private GameObject newGameMenu;

    [Header("Load Menu Settings")]
    [SerializeField] private GameObject loadGameMenu;
    [SerializeField] private Button[] loadButtons;

    private bool menuOpened = false;

    void Update()
    {
        if (!menuOpened)
        {
            // Check if any key on the keyboard was pressed
            // OR if any mouse button was clicked
            if (Keyboard.current.anyKey.wasPressedThisFrame ||
                Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame)
            {
                OpenMenu();
            }
        }
    }

    public void OpenMenu()
    {
        menuOpened = true;
        pressAnyKeyGroup.SetActive(false);
        menuButtonsGroup.SetActive(true);
        newGameMenu.SetActive(false);
    }

    public void OpenNewGameMenu()
    {
        menuButtonsGroup.SetActive(false);
        newGameMenu.SetActive(true);
    }

    public void BackToMain()
    {
        newGameMenu.SetActive(false);
        menuButtonsGroup.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    public void OpenLoadMenu()
    {
        menuButtonsGroup.SetActive(false);
        loadGameMenu.SetActive(true);

        // Check each slot to see if the file exists
        for (int i = 0; i < loadButtons.Length; i++)
        {
            int slotNum = i + 1;
            string path = Path.Combine(Application.persistentDataPath, $"SaveSlot_{slotNum}.db");

            if (File.Exists(path))
            {
                loadButtons[i].interactable = true;
                // Optional: Change text to show "Save Found" or a timestamp
            }
            else
            {
                loadButtons[i].interactable = false; // Grays out the button
            }
        }
    }

    public void StartNewGame(int slot)
    {
        // Setup the DB (True = Wipe data)
        DatabaseManager.Instance.Initialize(slot, true);

        // Change the scene
        SceneChanger.Instance.MoveToScene("World");
    }

    public void LoadExistingGame(int slot)
    {
        // Setup the DB (False = Don't wipe data)
        DatabaseManager.Instance.Initialize(slot, false);

        // Change the scene
        SceneChanger.Instance.MoveToScene("World");
    }
}