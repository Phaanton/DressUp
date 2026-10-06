using System;
using System.Collections; // Necessário para a Coroutine
using UnityEngine;
using UnityEngine.UI;

public class ItemButtonView : MonoBehaviour
{
    [SerializeField] private Button buttonComponent;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject lockIcon;

    private string currentItemID;
    private Action<string> onClickCallback;
    
    // NOVO: Para o Presenter/View poder injetar o som de clique aqui
    private DressUpView mainView;

    // Atualizamos o Setup para receber a View principal
    public void Setup(ClothingItemSO itemData, bool isUnlocked, Action<string> callback, DressUpView viewReference = null)
    {
        currentItemID = itemData.itemID;
        mainView = viewReference; // Guardamos a referência

        if (itemData.icon != null) iconImage.sprite = itemData.icon;
        else iconImage.sprite = itemData.itemSprite;

        if (lockIcon != null) lockIcon.SetActive(!isUnlocked);

        onClickCallback = callback;
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        // Toca o som de clique
        if (mainView != null) mainView.PlayClickSound();

        onClickCallback?.Invoke(currentItemID);
    }
}