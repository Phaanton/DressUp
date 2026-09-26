using System;
using System.Collections.Generic;

public class DressUpModel
{
    // Estado atual do personagem (o que está vestido)
    private Dictionary<ClothingCategory, ClothingItemSO> equippedItems = new Dictionary<ClothingCategory, ClothingItemSO>();

    // Mapa de Estratégias
    private Dictionary<ClothingCategory, IEquipStrategy> equipStrategies;

    // Guarda os IDs das roupas que já foram desbloqueadas
    private HashSet<string> unlockedItemsIDs = new HashSet<string>();

    public event Action<Dictionary<ClothingCategory, ClothingItemSO>> OnEquipmentChanged;


    public DressUpModel()
    {
        // Inicializa o mapa de estratégias (Factory simples)
        equipStrategies = new Dictionary<ClothingCategory, IEquipStrategy>
        {
            { ClothingCategory.Hair, new StandardEquipStrategy() },
            { ClothingCategory.Hat, new StandardEquipStrategy() },
            { ClothingCategory.Shoes, new StandardEquipStrategy() },
            { ClothingCategory.Accessory, new StandardEquipStrategy() },

            { ClothingCategory.Shirt, new TorsoOrLegsEquipStrategy() },
            { ClothingCategory.Pants, new TorsoOrLegsEquipStrategy() },

            { ClothingCategory.Dress, new DressEquipStrategy() }
        };
    }

    public void TryEquipItem(ClothingItemSO itemToEquip, bool hasWatchedAd)
    {
        if (itemToEquip.requiresAdToUnlock && !hasWatchedAd)
        {
            // Lógica de rejeição ou aviso de AD irá aqui depois
            return;
        }

        // Executa a estratégia correta sem usar IFs!
        if (equipStrategies.TryGetValue(itemToEquip.category, out IEquipStrategy strategy))
        {
            strategy.Equip(itemToEquip, equippedItems);

            // Dispara o evento avisando o Presenter que os itens mudaram
            OnEquipmentChanged?.Invoke(equippedItems);
        }
    }

    // Checa se o item é livre ou se já foi desbloqueado
    public bool IsItemUnlocked(ClothingItemSO item)
    {
        if (!item.requiresAdToUnlock) return true;

        return unlockedItemsIDs.Contains(item.itemID);
    }

    // Registra o desbloqueio
    public void UnlockItem(string itemID)
    {
        unlockedItemsIDs.Add(itemID);
        // Aqui, no futuro, você chamará um método para salvar no PlayerPrefs ou num JSON.
    }
}