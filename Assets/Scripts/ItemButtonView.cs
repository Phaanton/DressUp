using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemButtonView : MonoBehaviour
{
    [SerializeField] private Button buttonComponent;
    [SerializeField] private Image iconImage;

    private string currentItemID;

    // Mantendo o formato == == que combinámos!
    private Action<string> onClickCallback;

    public void Setup(ClothingItemSO itemData, Action<string> callback)
    {
        currentItemID = itemData.itemID;

        // Correção: a propriedade herdada de CatalogNodeSO chama-se 'icon'
        if (itemData.icon != null)
            iconImage.sprite = itemData.icon;
        else
            iconImage.sprite = itemData.itemSprite;

        onClickCallback = callback;

        // Limpa ouvintes antigos (útil se o botão for reutilizado num Object Pool)
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        // Grita para a View: "Fui clicado! E o meu ID é este!"
        onClickCallback?.Invoke(currentItemID);
    }
}