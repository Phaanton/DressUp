using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
//using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class DressUpPresenter
{
    private readonly DressUpModel model;
    private readonly DressUpView view;
    private readonly CatalogGroupSO mainCatalog;
    private readonly AdsManager adsManager;
    private List<FontItemSO> allFontsCatalog;

    private ClothingCategory currentCategory;

    public DressUpPresenter(DressUpModel model, DressUpView view, CatalogGroupSO catalog, AdsManager adsManager)
    {
        this.model = model;
        this.view = view;
        this.mainCatalog = catalog;
        this.adsManager = adsManager;

        this.allFontsCatalog = catalog.allFontItems;

        this.view.OnEquipButtonClicked += HandleEquipClicked;

        // O evento agora está sincronizado e recebe o IClothingVisual
        this.model.OnEquipmentChanged += HandleEquipmentChanged;

        this.view.OnCategoryTabClicked += HandleCategoryTabClicked;
        this.view.OnColorSelected += HandleColorSelected;

        this.view.OnFontsTabClicked += HandleFontsTabClicked;
        this.view.OnFontEquipClicked += HandleFontEquipClicked;
    }

    public void StartGame()
    {
        // Pede ao Model os visuais já decorados com as cores
        view.UpdateCharacterVisuals(model.GetCurrentVisuals());

        HandleCategoryTabClicked(ClothingCategory.Shirt);
    }

    private void HandleCategoryTabClicked(ClothingCategory category)
    {
        currentCategory = category;
        RefreshCurrentCategoryView();
    }

    private void RefreshCurrentCategoryView()
    {
        List<ClothingItemSO> allItems = mainCatalog.GetAllItems();
        List<ClothingItemSO> filteredItems = allItems.FindAll(item => item.category == currentCategory);

        view.PopulateCategoryMenu(filteredItems, (item) => model.IsItemUnlocked(item));
    }

    private void HandleEquipClicked(string itemId)
    {
        List<ClothingItemSO> allItems = mainCatalog.GetAllItems();
        ClothingItemSO itemToEquip = allItems.Find(item => item.itemID == itemId);

        if (itemToEquip != null)
        {
            // Verifica se o item clicado já é o que o personagem está a usar
            ClothingItemSO currentlyEquipped = model.GetEquippedItem(itemToEquip.category);
            if (currentlyEquipped != null && currentlyEquipped.itemID == itemToEquip.itemID)
            {
                // Se for o mesmo, ele desequipa!
                model.UnequipItem(itemToEquip.category);
                view.PlayEquipFeedback();
                view.UpdateEquippedItemName(""); // Limpa o nome na tela
                return; // Interrompe a função aqui para não equipar novamente
            }

            if (model.IsItemUnlocked(itemToEquip))
            {
                EquipItemAndFetchTranslation(itemToEquip);
            }
            else
            {
                adsManager.ShowRewardedAd(
                    onSuccess: () =>
                    {
                        model.UnlockItem(itemToEquip.itemID);
                        EquipItemAndFetchTranslation(itemToEquip);
                        RefreshCurrentCategoryView();
                    },
                    onFailed: () => Debug.Log("AD cancelado.")
                );
            }
        }
    }

    private void EquipItemAndFetchTranslation(ClothingItemSO item)
    {
        model.TryEquipItem(item, true);

        view.PlayEquipFeedback();

        var asyncOperation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("UITexts", item.localizationKey);
        asyncOperation.Completed += (AsyncOperationHandle<string> handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                view.UpdateEquippedItemName(handle.Result);
        }
        ;
    }

    // Assinatura do método atualizada para IClothingVisual
    private void HandleEquipmentChanged(Dictionary<ClothingCategory, IClothingVisual> currentEquipment)
    {
        view.UpdateCharacterVisuals(currentEquipment);
    }

    private void HandleColorSelected(Color color)
    {
        // currentCategory é a variável que já memoriza qual aba está aberta (Shirt, Shoes, etc.)
        model.ChangeEquippedColor(currentCategory, color);
    }

    private void HandleFontsTabClicked()
    {
        // Usa a sua lógica do Model para saber se está destrancado
        view.PopulateFontsMenu(allFontsCatalog, (font) => !font.requiresAdToUnlock || model.IsItemUnlocked(font.itemID)); // Usamos uma lambda para adaptar ao método antigo
    }

    private void HandleFontEquipClicked(string fontId)
    {
        FontItemSO fontToEquip = allFontsCatalog.Find(f => f.itemID == fontId);
        if (fontToEquip == null) return;

        bool isUnlocked = !fontToEquip.requiresAdToUnlock || model.IsItemUnlocked(fontToEquip.itemID); // Se não usa Ads ou já destrancou

        if (isUnlocked)
        {
            view.ChangeNameFont(fontToEquip.fontAsset);
        }
        else
        {
            adsManager.ShowRewardedAd(
                onSuccess: () =>
                {
                    model.UnlockItem(fontToEquip.itemID); // Reaproveitamos o UnlockItem das roupas!
                    view.ChangeNameFont(fontToEquip.fontAsset);
                    HandleFontsTabClicked(); // Atualiza a aba para sumir o cadeado/video
                },
                onFailed: () => Debug.Log("AD de Fonte cancelado.")
            );
        }
    }
}