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
    private bool isEditingFonts = false;

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

        this.view.OnAudioToggleClicked += HandleAudioToggle;
        LoadAudioState();

        this.view.OnMainTabClicked += HandleMainTabClicked;
    }

    public void StartGame()
    {
        view.HideNameInput();

        // Pede ao Model os visuais já decorados com as cores
        view.UpdateCharacterVisuals(model.GetCurrentVisuals());

        HandleMainTabClicked(MainCategoryGroup.Head);
        HandleCategoryTabClicked(ClothingCategory.HairFront);
    }

    private void HandleCategoryTabClicked(ClothingCategory category)
    {
        isEditingFonts = false;

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
            if (itemToEquip.category != ClothingCategory.Pet && currentlyEquipped != null && currentlyEquipped.itemID == itemToEquip.itemID)
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
        // Se for um Pet, apenas manda a View instanciar um novo!
        if (item.category == ClothingCategory.Pet)
        {
            view.SpawnPet(item);
            view.PlayEquipFeedback();
            return; // Sai da função sem tentar equipar no Model
        }

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
        if (isEditingFonts)
        {
            // Se estiver na aba de fontes, pinta apenas o texto
            view.SetNameTextColor(color);
        }
        else
        {
            // currentCategory é a variável que já memoriza qual aba está aberta (Shirt, Shoes, etc.)
            model.ChangeEquippedColor(currentCategory, color);
        }
    }

    private void HandleFontsTabClicked()
    {
        isEditingFonts = true;
        view.SetCameraZoom(false);

        // Usa a sua lógica do Model para saber se está destrancado
        view.PopulateFontsMenu(allFontsCatalog, (font) => !font.requiresAdToUnlock || model.IsItemUnlocked(font.itemID));
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

    private void HandleAudioToggle()
    {
        // Verifica o estado atual (volume 0 é mutado, volume 1 é som ligado)
        bool isCurrentlyMuted = AudioListener.volume == 0f;

        // Inverte o estado
        bool newStateMuted = !isCurrentlyMuted;

        ApplyAudioState(newStateMuted);
    }

    private void ApplyAudioState(bool isMuted)
    {
        // O AudioListener afeta TODOS os sons do jogo de uma só vez
        AudioListener.volume = isMuted ? 0f : 1f;

        // Atualiza a View
        view.UpdateAudioButtonIcon(isMuted);

        // Salva a preferência (1 para mutado, 0 para com som)
        PlayerPrefs.SetInt("GameMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadAudioState()
    {
        // Lê a preferência salva (padrão é 0 = com som)
        bool isMuted = PlayerPrefs.GetInt("GameMuted", 0) == 1;
        ApplyAudioState(isMuted);
    }

    private void HandleMainTabClicked(MainCategoryGroup group)
    {
        // 1. Atualiza a visibilidade das sub-guias na UI
        view.UpdateSubTabsVisibility(group);

        // 2. Controla o Zoom: Ativa apenas se a guia principal for "Head"
        view.SetCameraZoom(group == MainCategoryGroup.Head);

        // 3. (Opcional) Define se está nas fontes para o sistema de cores
        isEditingFonts = (group == MainCategoryGroup.Fonts);
    }
}