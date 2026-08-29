using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    private bool _isInventoryOpen = false;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform itemGrid;
    public GameObject itemSlotPrefab;
    public TMP_InputField searchField;

    public static PlayerUIController Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // This ensures the Canvas doesn't disappear when you go into the house
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If a second UI exists, kill it so we only have one
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    public class ItemVisualData
    {
        public string itemName;
        public Sprite itemIcon;
    }

    public List<ItemVisualData> itemDatabase;

    // Update is called once per frame
    void Update()
    {
        bool tabPressed = Keyboard.current.tabKey.wasPressedThisFrame;
        bool iPressed = Keyboard.current.iKey.wasPressedThisFrame;

        if (tabPressed || iPressed)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        _isInventoryOpen = !_isInventoryOpen;
        inventoryPanel.SetActive(_isInventoryOpen);

        // Handle Mouse Cursor
        if (_isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // Pause game while looking at SQL data
            RefreshUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    public void OnSearchChanged(string input)
    {
        RefreshUI(input);
    }

    public void RefreshUI(string search = "")
    {
        // 1. Clear everything out
        foreach (Transform child in itemGrid) { Destroy(child.gameObject); }

        var items = DatabaseManager.Instance.GetInventoryItems(search);
        items = items.Where(i => i.Quantity > 0).ToList();
        int totalSlots = 24;

        // 2. This loop MUST run 24 times to fill the grid
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemGrid);

            // Find your components
            Image iconImage = slot.transform.Find("ItemIcon").GetComponent<Image>();
            TextMeshProUGUI countText = slot.GetComponentInChildren<TextMeshProUGUI>();

            // 3. If we have an item for this index, show it
            if (i < items.Count)
            {
                var item = items[i];
                Debug.Log("UI is drawing item: '" + item.ItemName + "'");
                ItemVisualData visualData = itemDatabase.Find(x => x.itemName == item.ItemName);

                if (visualData != null)
                {
                    iconImage.sprite = visualData.itemIcon;
                    iconImage.enabled = true;
                }
                countText.text = "x" + item.Quantity;
            }
            else
            {
                // 4. This part creates the "empty" look for the other 23 slots
                iconImage.enabled = false;
                countText.text = "";
            }
        }
    }
}
