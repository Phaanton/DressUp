using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemButtonView : MonoBehaviour
{
    [SerializeField] private Button buttonComponent;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject lockIcon; // A referência visual do cadeado

    private string currentItemID;
    private Action<string> onClickCallback;

    // Adicionamos o bool isUnlocked
    public void Setup(ClothingItemSO itemData, bool isUnlocked, Action<string> callback)
    {
        currentItemID = itemData.itemID;

        if (itemData.icon != null)
            iconImage.sprite = itemData.icon;
        else
            iconImage.sprite = itemData.itemSprite;

        // Ativa o ícone de cadeado apenas se o item estiver bloqueado
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        onClickCallback = callback;
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        onClickCallback?.Invoke(currentItemID);
    }
}