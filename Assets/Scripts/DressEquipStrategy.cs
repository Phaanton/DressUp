using System.Collections.Generic;

public class DressEquipStrategy : IEquipStrategy
{
    public void Equip(ClothingItemSO itemToEquip, Dictionary<ClothingCategory, ClothingItemSO> currentEquipment)
    {
        currentEquipment[itemToEquip.category] = itemToEquip;

        // O vestido cobre o corpo todo, então removemos partes individuais
        currentEquipment.Remove(ClothingCategory.Shirt);
        currentEquipment.Remove(ClothingCategory.Pants);
    }
}