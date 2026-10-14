using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct CategoryRendererMapping
{
    public ClothingCategory category;
    public SpriteRenderer renderer;
}

public class DressUpView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI equippedItemNameText;

    [Header("UI Setup")]
    [SerializeField] private Transform buttonsContainer; // O GameObject com o Grid/Horizontal Layout
    [SerializeField] private ItemButtonView buttonPrefab;

    [Header("Visuals Setup")]
    // Uma lista para mapearmos todas as partes do corpo no Inspector
    [SerializeField] private List<CategoryRendererMapping>rendererMappings;

    [Header("Polish & Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip equipClip;
    [SerializeField] private ParticleSystem equipParticles;

    [Header("Animations")]
    [SerializeField] private CanvasGroup menuCanvasGroup; // Controla a transparência da lista
    [SerializeField] private float fadeDuration = 0.15f;



    [Header("Fonts Setup")]
    [SerializeField] private TMP_InputField characterNameInput; // Arraste o seu InputField aqui
    [SerializeField] private FontButtonView fontButtonPrefab;   // Arraste o novo prefab aqui

    private Coroutine tabTransitionCoroutine;

    public event Action<string> OnEquipButtonClicked;
    public event Action<ClothingCategory> OnCategoryTabClicked;
    public event Action OnFontsTabClicked;
    public event Action<string> OnFontEquipClicked;
    public event Action<Color> OnColorSelected;

    // Conectado aos botões de UI no Inspector
    public void ClickEquipButton(string itemId)
    {
        OnEquipButtonClicked?.Invoke(itemId);
    }

    public void UpdateCharacterVisuals(Dictionary<ClothingCategory, IClothingVisual> currentEquipment)
    {
        foreach (CategoryRendererMapping mapping in rendererMappings)
        {
            // Se o Model diz que temos um visual (Decorator ou Padrão) nesta categoria:
            if (currentEquipment.TryGetValue(mapping.category, out IClothingVisual visual))
            {
                // A View não sabe se a roupa tem uma cor base ou foi alterada, ela apenas obedece à interface!
                mapping.renderer.sprite = visual.GetSprite();
                mapping.renderer.color = visual.GetColor();
            }
            else
            {
                mapping.renderer.sprite = null;
                // Opcional: resetar a cor ao tirar a roupa
                mapping.renderer.color = Color.white;
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
    public void PopulateCategoryMenu(List<ClothingItemSO> categoryItems, Func<ClothingItemSO, bool> isUnlockedMethod)
    {
        // Se já houver uma animação a correr (clique rápido em várias abas), paramos a anterior
        if (tabTransitionCoroutine != null)
        {
            StopCoroutine(tabTransitionCoroutine);
        }

        tabTransitionCoroutine = StartCoroutine(FadeAndPopulateRoutine(categoryItems, isUnlockedMethod));
    }

    public void OnTabButtonClicked(string categoryName)
    {
        PlayClickSound();

        if (categoryName == "Fonts")
        {
            OnFontsTabClicked?.Invoke();
            return; // Sai da função sem tentar converter para o Enum
        }

        if (Enum.TryParse(categoryName, out ClothingCategory parsedCategory))
        {
            OnCategoryTabClicked?.Invoke(parsedCategory);
        }
        else
        {
            Debug.LogError($"Categoria inválida: {categoryName}");
        }
    }

    public void SelectColor(Color color)
    {
        OnColorSelected?.Invoke(color);
    }

    public void PlayClickSound()
    {
        if (audioSource != null && clickClip != null)
        {
            audioSource.PlayOneShot(clickClip);
        }
    }

    public void PlayEquipFeedback()
    {
        if (audioSource != null && equipClip != null)
        {
            audioSource.PlayOneShot(equipClip);
        }

        if (equipParticles != null)
        {
            // Reinicia e toca as partículas
            equipParticles.Stop();
            equipParticles.Play();
        }
    }

    public void ChangeNameFont(TMP_FontAsset newFont)
    {
        if (characterNameInput != null)
        {
            characterNameInput.textComponent.font = newFont;
        }
    }

    public void PopulateFontsMenu(List<FontItemSO> fontItems, Func<FontItemSO, bool> isUnlockedMethod)
    {
        if (tabTransitionCoroutine != null) StopCoroutine(tabTransitionCoroutine);
        tabTransitionCoroutine = StartCoroutine(FadeAndPopulateFontsRoutine(fontItems, isUnlockedMethod));
    }

    private IEnumerator FadeAndPopulateRoutine(List<ClothingItemSO> categoryItems, Func<ClothingItemSO, bool> isUnlockedMethod)
    {
        // 1. FADE OUT (Desaparece gradualmente)
        if (menuCanvasGroup != null)
        {
            float elapsed = 0f;
            float startAlpha = menuCanvasGroup.alpha;
            while (elapsed < fadeDuration)
            {
                menuCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            menuCanvasGroup.alpha = 0f;
        }

        // 2. TROCA DE ITENS (Feita enquanto a lista está invisível)
        foreach (Transform child in buttonsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (ClothingItemSO item in categoryItems)
        {
            ItemButtonView newButton = Instantiate(buttonPrefab, buttonsContainer);
            bool isUnlocked = isUnlockedMethod(item);
            newButton.Setup(item, isUnlocked, HandleItemButtonClicked, this);
        }

        // Pequena pausa para garantir que a Unity instanciou tudo visualmente
        yield return new WaitForEndOfFrame();

        // 3. FADE IN (Aparece gradualmente)
        if (menuCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                menuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            menuCanvasGroup.alpha = 1f;
        }
    }

    private void HandleItemButtonClicked(string itemID)
    {
        // Repassa o clique para o Presenter (que já validará o AD e equipará no Model)
        OnEquipButtonClicked?.Invoke(itemID);
    }

    private IEnumerator FadeAndPopulateFontsRoutine(List<FontItemSO> fontItems, Func<FontItemSO, bool> isUnlockedMethod)
    {
        // 1. FADE OUT
        if (menuCanvasGroup != null)
        {
            float elapsed = 0f;
            float startAlpha = menuCanvasGroup.alpha;
            while (elapsed < fadeDuration)
            {
                menuCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            menuCanvasGroup.alpha = 0f;
        }

        // 2. TROCA DE ITENS
        foreach (Transform child in buttonsContainer) Destroy(child.gameObject);

        foreach (FontItemSO item in fontItems)
        {
            FontButtonView newButton = Instantiate(fontButtonPrefab, buttonsContainer);
            bool isUnlocked = isUnlockedMethod(item);
            newButton.Setup(item, isUnlocked, (id) => OnFontEquipClicked?.Invoke(id));
        }

        yield return new WaitForEndOfFrame();

        // 3. FADE IN
        if (menuCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                menuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            menuCanvasGroup.alpha = 1f;
        }
    }
}