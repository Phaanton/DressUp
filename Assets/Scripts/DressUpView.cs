using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DressUpView : MonoBehaviour
{
    // Evento disparado quando o jogador clica em um botão
    public event Action<string> OnEquipButtonClicked;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI equippedItemNameText;

    // Conectado aos botões de UI no Inspector
    public void ClickEquipButton(string itemId)
    {
        OnEquipButtonClicked?.Invoke(itemId);
    }

    // Chamado pelo Presenter para atualizar o visual
    public void UpdateCharacterVisuals(Dictionary<ClothingCategory, ClothingItemSO> currentEquipment)
    {
        // Por enquanto fica vazio, vamos implementar a lógica visual depois!
        Debug.Log("View recebeu os itens atualizados!");
    }

    // Método que o Presenter vai chamar quando a tradução chegar
    public void UpdateEquippedItemName(string localizedName)
    {
        if (equippedItemNameText != null)
        {
            equippedItemNameText.text = localizedName;
        }
    }
}