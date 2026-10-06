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

    private ClothingCategory currentCategory;

    public DressUpPresenter(DressUpModel model, DressUpView view, CatalogGroupSO catalog, AdsManager adsManager)
    {
        this.model = model;
        this.view = view;
        this.mainCatalog = catalog;
        this.adsManager = adsManager;

        this.view.OnEquipButtonClicked += HandleEquipClicked;

        // O evento agora está sincronizado e recebe o IClothingVisual
        this.model.OnEquipmentChanged += HandleEquipmentChanged;

        this.view.OnCategoryTabClicked += HandleCategoryTabClicked;
        this.view.OnColorSelected += HandleColorSelected;
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

        view.PopulateCategoryMenu(filteredItems, model.IsItemUnlocked);
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
}