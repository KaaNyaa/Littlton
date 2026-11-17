using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ToolController : MonoBehaviour
{
    public ToolType currentTool = ToolType.Axe;

    [Header("UI References")]
    public Image toolIconDisplay; // 'Icon' child Image component
    public Sprite axeSprite;      // Axe artwork
    public Sprite pickaxeSprite;  // Pickaxe artwork

    void Start()
    {
        SyncWithInventory();
        UpdateToolUI();
    }

    void Update()
    {
        // Don't scroll through tools if the game is paused!
        if (PauseMenu.isPaused) return;

        // Get the scroll wheel vector from the New Input System
        Vector2 scrollValue = Mouse.current.scroll.ReadValue();

        if (scrollValue.y != 0f)
        {
            // Scrolled Up or Down -> Switch Tool
            SwitchTool();
        }
    }

    void SwitchTool()
    {
        // Toggle between the two tools
        if (currentTool == ToolType.Axe)
        {
            currentTool = ToolType.Pickaxe;
        }
        else
        {
            currentTool = ToolType.Axe;
        }

        SyncWithInventory();
        UpdateToolUI();
        Debug.Log("Equipped Tool: " + currentTool);
    }

    void SyncWithInventory()
    {
        // Tell the global InventoryManager what tool is currently active
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.currentEquippedTool = currentTool;
        }
    }

    void UpdateToolUI()
    {
        if (toolIconDisplay == null) return;

        // Swap the sprite on HUD
        if (currentTool == ToolType.Axe)
        {
            toolIconDisplay.sprite = axeSprite;
        }
        else if (currentTool == ToolType.Pickaxe)
        {
            toolIconDisplay.sprite = pickaxeSprite;
        }
    }
}