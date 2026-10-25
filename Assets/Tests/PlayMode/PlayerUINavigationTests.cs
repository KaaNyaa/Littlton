using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerUINavigationTests
{
    private GameObject testControllerObject;
    private PlayerUIController uiController;

    [SetUp]
    public void Setup()
    {
        // Create a fresh GameObject and PlayerUIController for isolation
        testControllerObject = new GameObject("TestUIController");
        uiController = testControllerObject.AddComponent<PlayerUIController>();

        // Create a mock Canvas and Panel so activeSelf checks don't fail
        GameObject canvasObj = new GameObject("TestCanvas", typeof(Canvas));
        GameObject panelObj = new GameObject("InventoryPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);

        uiController.inventoryPanel = panelObj;
        uiController.inventoryPanel.SetActive(false);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up objects after test runs
        if (testControllerObject != null)
            Object.Destroy(testControllerObject);

        Time.timeScale = 1f; // Reset time scale just in case
    }

    [UnityTest]
    public IEnumerator Test_InventoryToggle_ChangesStateCorrectly()
    {
        // Initial state check (Inventory closed)
        Assert.IsFalse(uiController.inventoryPanel.activeSelf, "Inventory should start closed.");

        // Trigger inventory open
        uiController.ToggleInventory();
        yield return null; // Wait one frame for state update

        // Assert UI opened and time paused
        Assert.IsTrue(uiController.inventoryPanel.activeSelf, "Inventory panel should be active after toggle.");
        Assert.AreEqual(0f, Time.timeScale, 0.001f, "Game time should be paused when inventory is open.");

        // Toggle inventory closed again
        uiController.ToggleInventory();
        yield return null; // Wait one frame

        // Assert UI closed and time resumed
        Assert.IsFalse(uiController.inventoryPanel.activeSelf, "Inventory panel should be inactive after closing.");
        Assert.AreEqual(1f, Time.timeScale, 0.001f, "Game time should resume when inventory is closed.");
    }
}