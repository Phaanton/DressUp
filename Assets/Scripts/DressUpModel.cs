using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.VersionControl;
using UnityEngine;

// 1. Nova estrutura que guarda o ID da roupa e a cor aplicada
[Serializable]
public class SavedEquipmentData
{
    public string itemID;
    public Color itemColor;
}

[Serializable]
public class DressUpSaveData
{
    public List<string> unlockedItemIDs = new List<string>();
    public List<SavedEquipmentData> equippedItems = new List<SavedEquipmentData>();
}

public class DressUpModel
{
    private Dictionary<ClothingCategory, ClothingItemSO> equippedItems = new Dictionary<ClothingCategory, ClothingItemSO>();
    private Dictionary<ClothingCategory, Color> equippedColors = new Dictionary<ClothingCategory, Color>();
    
    private Dictionary<ClothingCategory, IEquipStrategy> equipStrategies;
    private HashSet<string> unlockedItemsIDs = new HashSet<string>();

    public event Action<Dictionary<ClothingCategory, IClothingVisual>> OnEquipmentChanged;

    public DressUpModel()
    {
        equipStrategies = new Dictionary<ClothingCategory, IEquipStrategy>
        {
            { ClothingCategory.Hair, new StandardEquipStrategy() },
            { ClothingCategory.Hat, new StandardEquipStrategy() },
            { ClothingCategory.Shoes, new StandardEquipStrategy() },
            { ClothingCategory.Accessory, new StandardEquipStrategy() },
            { ClothingCategory.Shirt, new TorsoOrLegsEquipStrategy() },
            { ClothingCategory.Pants, new TorsoOrLegsEquipStrategy() },
            { ClothingCategory.Dress, new DressEquipStrategy() },
            { ClothingCategory.Pet, new StandardEquipStrategy() },
            { ClothingCategory.Face, new StandardEquipStrategy() }
        };
    }

    // NAssinatura atualizada que recebe a cor a aplicar
    public void TryEquipItem(ClothingItemSO itemToEquip, Color color, bool triggerEvent = true)
    {
        if (equipStrategies.TryGetValue(itemToEquip.category, out IEquipStrategy strategy))
        {
            strategy.Equip(itemToEquip, equippedItems);
        }

        // Atualiza a cor no novo dicionário
        equippedColors[itemToEquip.category] = color;

        if (triggerEvent)
        {
            OnEquipmentChanged?.Invoke(GetCurrentVisuals());
            SaveData();
        }
    }

    // Sobrecarga: Se enviarmos apenas a roupa (como nos botões atuais), ele assume a cor branca
    public void TryEquipItem(ClothingItemSO itemToEquip, bool triggerEvent = true)
    {
        TryEquipItem(itemToEquip, Color.white, triggerEvent);
    }

    // Método para alterar apenas a cor da peça que já está vestida
    public void ChangeEquippedColor(ClothingCategory category, Color newColor)
    {
        if (equippedItems.ContainsKey(category))
        {
            equippedColors[category] = newColor;
            OnEquipmentChanged?.Invoke(GetCurrentVisuals());
            SaveData();
        }
    }

    public bool IsItemUnlocked(ClothingItemSO item)
    {
        if (!item.requiresAdToUnlock) return true;
        return unlockedItemsIDs.Contains(item.itemID);
    }

    public bool IsItemUnlocked(string itemID)
    {
        return unlockedItemsIDs.Contains(itemID);
    }

    public ClothingItemSO GetEquippedItem(ClothingCategory category)
    {
        return equippedItems.ContainsKey(category) ? equippedItems[category] : null;
    }

    public void UnequipItem(ClothingCategory category, bool triggerEvent = true)
    {
        if (equippedItems.ContainsKey(category))
        {
            equippedItems.Remove(category);
            equippedColors.Remove(category); // Limpa a cor também

            if (triggerEvent)
            {
                OnEquipmentChanged?.Invoke(GetCurrentVisuals());
                SaveData();
            }
        }
    }

    public void LoadData(List<ClothingItemSO> allCatalogItems)
    {
        if (PlayerPrefs.HasKey("DressUpSave"))
        {
            string json = PlayerPrefs.GetString("DressUpSave");
            DressUpSaveData data = JsonUtility.FromJson<DressUpSaveData>(json);

            unlockedItemsIDs = new HashSet<string>(data.unlockedItemIDs);

            foreach (SavedEquipmentData savedData in data.equippedItems)
            {
                ClothingItemSO item = allCatalogItems.Find(i => i.itemID == savedData.itemID);
                if (item != null)
                {
                    // Restaura a roupa e a cor exata do save
                    TryEquipItem(item, savedData.itemColor, false);
                }
            }
            Debug.Log("Jogo carregado com sucesso!");
        }
    }

    public void UnlockItem(string itemID)
    {
        unlockedItemsIDs.Add(itemID);
        SaveData();
    }

    // NOVO: A magia do Decorator acontece aqui. Ele embrulha a roupa e a cor antes de enviar para a View.
    public Dictionary<ClothingCategory, IClothingVisual> GetCurrentVisuals()
    {
        var visuals = new Dictionary<ClothingCategory, IClothingVisual>();

        foreach (var kvp in equippedItems)
        {
            ClothingCategory category = kvp.Key;
            ClothingItemSO item = kvp.Value;
            Color color = equippedColors.ContainsKey(category) ? equippedColors[category] : Color.white;

            // Cria o visual padrão
            IClothingVisual visual = new DefaultClothingVisual(item);

            // Se tiver uma cor aplicada, embrulha no Decorator
            if (color != Color.white)
            {
                visual = new ColorClothingDecorator(visual, color);
            }

            visuals[category] = visual;
        }
        return visuals;
    }

    // Mantido temporariamente para não quebrar outros scripts
    public Dictionary<ClothingCategory, ClothingItemSO> GetCurrentEquipment()
    {
        return equippedItems;
    }

    private void SaveData()
    {
        DressUpSaveData data = new DressUpSaveData();
        data.unlockedItemIDs = new List<string>(unlockedItemsIDs);

        foreach (var kvp in equippedItems)
        {
            data.equippedItems.Add(new SavedEquipmentData
            {
                itemID = kvp.Value.itemID,
                // Se não houver cor registada, guarda branco
                itemColor = equippedColors.ContainsKey(kvp.Key) ? equippedColors[kvp.Key] : Color.white
            });
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("DressUpSave", json);
        PlayerPrefs.Save();
        Debug.Log("Jogo salvo com sucesso: " + json);
    }
}