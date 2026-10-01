using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings; // Necessário para a API de localização
using UnityEngine.ResourceManagement.AsyncOperations; // Necessário para lidar com o carregamento assíncrono

public class DressUpPresenter
{
    private readonly DressUpModel model;
    private readonly DressUpView view;

    // Adicionamos o catálogo para poder buscar as roupas pelo ID
    private readonly CatalogGroupSO mainCatalog;

    private readonly AdsManager adsManager;

    public DressUpPresenter(DressUpModel model, DressUpView view, CatalogGroupSO catalog, AdsManager adsManager)
    {
        this.model = model;
        this.view = view;
        this.mainCatalog = catalog;
        this.adsManager = adsManager;

        this.view.OnEquipButtonClicked += HandleEquipClicked;
        this.model.OnEquipmentChanged += HandleEquipmentChanged;
    }

    private void HandleEquipClicked(string itemId)
    {
        List<ClothingItemSO> allItems = mainCatalog.GetAllItems();
        ClothingItemSO itemToEquip = allItems.Find(item => item.itemID == itemId);

        if (itemToEquip != null)
        {
            if (model.IsItemUnlocked(itemToEquip))
            {
                EquipItemAndFetchTranslation(itemToEquip);
            }
            else
            {
                // Lógica de AD que fizemos na Etapa 4...
                adsManager.ShowRewardedAd(
                    onSuccess: () =>
                    {
                        model.UnlockItem(itemToEquip.itemID);
                        EquipItemAndFetchTranslation(itemToEquip);
                        view.PopulateCategoryMenu(mainCatalog.GetAllItems(), model.IsItemUnlocked);
                    },
                    onFailed: () => Debug.Log("AD cancelado.")
                );
            }
        }
    }

    // Método isolado para manter o código limpo
    private void EquipItemAndFetchTranslation(ClothingItemSO item)
    {
        // 1. Equipa no Model (isso já dispara a atualização visual dos sprites)
        model.TryEquipItem(item, true);

        // 2. Pede a tradução assíncrona do nome da roupa
        var asyncOperation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("UITexts", item.localizationKey);

        // 3. Define o que acontece quando a tradução terminar de carregar
        asyncOperation.Completed += (AsyncOperationHandle<string> handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // Manda a View atualizar o texto na tela
                view.UpdateEquippedItemName(handle.Result);
            }
            else
            {
                Debug.LogError($"Falha ao carregar a tradução para a chave: {item.localizationKey}");
            }
        };
    }

    // Agora recebemos o Dicionário completo do Model
    private void HandleEquipmentChanged(Dictionary<ClothingCategory, ClothingItemSO> currentEquipment)
    {
        // Mandamos a View atualizar a tela com as roupas atuais
        view.UpdateCharacterVisuals(currentEquipment);
    }
}