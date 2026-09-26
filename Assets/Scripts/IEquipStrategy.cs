using System.Collections.Generic;

public interface IEquipStrategy
{
    // Modifica o dicionário de itens equipados baseado na regra específica
    void Equip(ClothingItemSO itemToEquip, Dictionary<ClothingCategory, ClothingItemSO> currentEquipment);
}