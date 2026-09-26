using System.Collections.Generic;

public class TorsoOrLegsEquipStrategy : IEquipStrategy
{
    public void Equip(ClothingItemSO itemToEquip, Dictionary<ClothingCategory, ClothingItemSO> currentEquipment)
    {
        currentEquipment[itemToEquip.category] = itemToEquip;

        // Se colocar uma camisa ou calça, o vestido precisa ser retirado
        currentEquipment.Remove(ClothingCategory.Dress);
    }
}