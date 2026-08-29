using System;

[Serializable]
public class InventoryItem
{
    public int Id; // Primary Key
    public string ItemName;
    public int Quantity;
    public string Category;
    public string IconPath;
}
