using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public struct CategoryRendererMapping
{
    public ClothingCategory category;
    public SpriteRenderer renderer;
}

public class DressUpView : MonoBehaviour
{
    // Evento disparado quando o jogador clica em um botão
    public event Action<string> OnEquipButtonClicked;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI equippedItemNameText;

    [Header("UI Setup")]
    [SerializeField] private Transform buttonsContainer; // O GameObject com o Grid/Horizontal Layout
    [SerializeField] private ItemButtonView buttonPrefab;

    [Header("Visuals Setup")]
    // Uma lista para mapearmos todas as partes do corpo no Inspector
    [SerializeField] private List<CategoryRendererMapping>rendererMappings;

    // Conectado aos botões de UI no Inspector
    public void ClickEquipButton(string itemId)
    {
        OnEquipButtonClicked?.Invoke(itemId);
    }

    // O Presenter chama este método e manda a lista de tudo que está vestido
    public void UpdateCharacterVisuals(Dictionary<ClothingCategory, ClothingItemSO> currentEquipment)
    {
        // Percorremos todos os nossos Renderers (Camisa, Calça, Chapéu...)
        foreach (CategoryRendererMapping mapping in rendererMappings)
        {
            // Se o Model diz que temos um item equipado nessa categoria:
            if (currentEquipment.TryGetValue(mapping.category, out ClothingItemSO equippedItem))
            {
                mapping.renderer.sprite = equippedItem.itemSprite;

                // Opcional: Garante que a cor base é branca (sem os Decorators por enquanto)
                mapping.renderer.color = Color.white;
            }
            else
            {
                // Se a categoria foi desequipada (ou apagada pelo Strategy do Vestido), limpamos o sprite
                mapping.renderer.sprite = null;
            }
        }
    }

    // Método que o Presenter vai chamar quando a tradução chegar
    public void UpdateEquippedItemName(string localizedName)
    {
        if (equippedItemNameText != null)
        {
            equippedItemNameText.text = localizedName;
        }
    }

    // Chamado pelo Presenter ao abrir uma categoria (ex: "Camisas")
    public void PopulateCategoryMenu(List<ClothingItemSO> categoryItems)
    {
        foreach (Transform child in buttonsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (ClothingItemSO item in categoryItems)
        {
            ItemButtonView newButton = Instantiate(buttonPrefab, buttonsContainer);
            newButton.Setup(item, HandleItemButtonClicked);
        }
    }

    private void HandleItemButtonClicked(string itemID)
    {
        // Repassa o clique para o Presenter (que já validará o AD e equipará no Model)
        OnEquipButtonClicked?.Invoke(itemID);
    }
}