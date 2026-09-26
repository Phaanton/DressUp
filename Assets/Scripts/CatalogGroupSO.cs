using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCatalogGroup", menuName = "DressUp/Catalog Group")]
public class CatalogGroupSO : CatalogNodeSO
{
    [Tooltip("Pode conter itens individuais ou outros grupos!")]
    public List<CatalogNodeSO> children = new List<CatalogNodeSO>();

    // Implementação do Composite: O grupo coleta tudo de seus filhos
    public override List<ClothingItemSO> GetAllItems()
    {
        List<ClothingItemSO> allItems = new List<ClothingItemSO>();

        foreach (CatalogNodeSO child in children)
        {
            if (child != null)
            {
                // Aqui a recursão acontece de forma invisível e segura
                allItems.AddRange(child.GetAllItems());
            }
        }

        return allItems;
    }
}