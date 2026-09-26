using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewClothingItem", menuName = "DressUp/Clothing Item")]
public class ClothingItemSO : CatalogNodeSO
{
    public string itemID;
    public string localizationKey;
    public ClothingCategory category;
    public Sprite itemSprite;
    public bool requiresAdToUnlock;

    // Implementação do Composite: A folha retorna ela mesma
    public override List<ClothingItemSO> GetAllItems()
    {
        return new List<ClothingItemSO> { this };
    }
}