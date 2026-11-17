using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public struct ToolbarItem
{
    public string itemName;
    public ToolType toolType;
    public Sprite iconSprite;
}

public class ToolbarController : MonoBehaviour
{
    [Header("Toolbar Configuration")]
    public List<ToolbarItem> toolbarItems = new List<ToolbarItem>();
    public List<ToolbarSlot> uiSlots = new List<ToolbarSlot>();

    private int selectedIndex = 0;

    void Start()
    {
        UpdateToolbarUI();
    }

    void Update()
    {
        if (PauseMenu.isPaused) return;

        // Mouse Scroll Wheel Input
        Vector2 scrollValue = Mouse.current.scroll.ReadValue();
        if (scrollValue.y > 0f)
        {
            selectedIndex = (selectedIndex - 1 + uiSlots.Count) % uiSlots.Count;
            UpdateToolbarUI();
        }
        else if (scrollValue.y < 0f)
        {
            selectedIndex = (selectedIndex + 1) % uiSlots.Count;
            UpdateToolbarUI();
        }

        // Number Key Inputs (1, 2, 3...)
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < 9 && Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                selectedIndex = i;
                UpdateToolbarUI();
                break;
            }
        }
    }

    void UpdateToolbarUI()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            bool isSelected = (i == selectedIndex);

            // If this slot has an item configured, show its icon.
            Sprite iconToDisplay = (i < toolbarItems.Count) ? toolbarItems[i].iconSprite : null;

            uiSlots[i].SetSlot(iconToDisplay, isSelected);
        }

        // Sync active tool to InventoryManager
        if (selectedIndex < toolbarItems.Count && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.currentEquippedTool = toolbarItems[selectedIndex].toolType;
            Debug.Log($"Equipped Tool: {toolbarItems[selectedIndex].itemName} ({toolbarItems[selectedIndex].toolType})");
        }
        else if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.currentEquippedTool = ToolType.None;
            Debug.Log("Equipped Tool: None (Empty Slot)");
        }
    }
}