using System.Collections.Generic;
using UnityEngine;

public abstract class CatalogNodeSO : ScriptableObject
{
    [Header("UI Info")]
    public string displayName;
    public Sprite icon; // Ícone que aparecerá na aba do menu

    // O método mágico que une tudo
    public abstract List<ClothingItemSO> GetAllItems();
}