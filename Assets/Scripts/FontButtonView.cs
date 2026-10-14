using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FontButtonView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fontNameText;
    [SerializeField] private Button button;
    [SerializeField] private GameObject adIcon;

    public void Setup(FontItemSO fontItem, bool isUnlocked, Action<string> onClickAction)
    {
        // Força o texto a ser "Aa" para servir de ícone
        fontNameText.text = "Aa";
        fontNameText.font = fontItem.fontAsset;

        if (adIcon != null) adIcon.SetActive(fontItem.requiresAdToUnlock && !isUnlocked);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickAction?.Invoke(fontItem.itemID));
    }
}