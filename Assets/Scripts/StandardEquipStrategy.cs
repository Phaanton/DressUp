using System.Collections.Generic;

public class StandardEquipStrategy : IEquipStrategy
{
    public void Equip(ClothingItemSO itemToEquip, Dictionary<ClothingCategory, ClothingItemSO> currentEquipment)
    {
        // Apenas coloca o item no seu respectivo slot
        currentEquipment[itemToEquip.category] = itemToEquip;
    }
}