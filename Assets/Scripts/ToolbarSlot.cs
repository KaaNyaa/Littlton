using UnityEngine;
using UnityEngine.UI;

public class ToolbarSlot : MonoBehaviour
{
    public Image iconImage;          // Drag the child 'Icon' image here
    public GameObject selectionBorder; // Optional highlight border

    private void Awake()
    {
        // Automatically find the child 'Icon' if it's not manually assigned
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
            }
        }
    }

    public void SetSlot(Sprite icon, bool isSelected)
    {
        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = icon;

                // Ensure the icon is fully opaque (solid color)
                Color c = iconImage.color;
                c.a = 1f;
                iconImage.color = c;
            }
            else
            {
                // Hide only the icon when the slot is empty, keeping the box visible
                iconImage.gameObject.SetActive(false);
            }
        }

        // Toggle the selection highlight border
        if (selectionBorder != null)
        {
            selectionBorder.SetActive(isSelected);
        }
    }
}