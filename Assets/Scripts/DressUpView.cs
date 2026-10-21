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
    [SerializeField] private AudioClip[] clickClips;
    [SerializeField] private AudioClip[] equipClips;
    [SerializeField] private ParticleSystem equipParticles;

    [Header("Animations")]
    [SerializeField] private CanvasGroup menuCanvasGroup; // Controla a transparência da lista
    [SerializeField] private float fadeDuration = 0.15f;

    [Header("Fonts Setup")]
    [SerializeField] private TMP_InputField characterNameInput; // Arraste o seu InputField aqui
    [SerializeField] private FontButtonView fontButtonPrefab;   // Arraste o novo prefab aqui

    [Header("Audio UI")]
    [Tooltip("Arraste o GameObject do ícone de bloqueio (o traço/X) aqui")]
    [SerializeField] private GameObject audioBlockIcon;

    [Header("Camera Zoom Settings")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("Um objeto vazio posicionado exatamente no rosto do personagem")]
    [SerializeField] private Transform faceZoomTarget;
    [SerializeField] private float zoomedOrthographicSize = 3f;
    [SerializeField] private float zoomSpeed = 6f;

    [Header("Pet & Decor System")]
    [SerializeField] private GameObject petPrefab; // Arraste o Prefab do Pet aqui
    [SerializeField] private Transform petSpawnPointBase; // O alvo que já movemos no LayoutAdapter

    [Header("Pet Context Menu")]
    [SerializeField] private GameObject petContextMenu;
    [SerializeField] private UnityEngine.UI.Button btnConfirmPet;
    [SerializeField] private UnityEngine.UI.Button btnFlipPet;
    [SerializeField] private UnityEngine.UI.Button btnDeletePet;

    public event Action<string> OnEquipButtonClicked;
    public event Action<ClothingCategory> OnCategoryTabClicked;
    public event Action OnFontsTabClicked;
    public event Action<string> OnFontEquipClicked;
    public event Action<Color> OnColorSelected;
    public event Action OnAudioToggleClicked;

    private Coroutine tabTransitionCoroutine;

    private int lastClickIndex = -1;
    private int lastEquipIndex = -1;

    private bool isCurrentlyZoomed = false;
    private Vector3 savedNormalPosition;
    private float savedNormalSize;
    private Coroutine zoomCoroutine;

    private DraggablePet activePet; // Memoriza qual pet está com o menu aberto

    private void Start() // Ou Awake, dependendo de como está o seu script
    {
        // Liga os botões do menu contextual
        if (btnConfirmPet) btnConfirmPet.onClick.AddListener(ClosePetMenu);
        if (btnFlipPet) btnFlipPet.onClick.AddListener(FlipActivePet);
        if (btnDeletePet) btnDeletePet.onClick.AddListener(DeleteActivePet);
    }

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

                // Se este slot for o Pet, reseta a posição
                DraggablePet petScript = mapping.renderer.GetComponent<DraggablePet>();
                if (petScript != null)
                {
                    //petScript.ResetPosition(visual.GetSprite());
                }
            }
            else
            {
                mapping.renderer.sprite = null;
                // Resetar a cor ao tirar a roupa
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
        if (audioSource != null && clickClips.Length > 0)
        {
            AudioClip clipToPlay = GetRandomUniqueClip(clickClips, ref lastClickIndex);
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    public void PlayEquipFeedback()
    {
        if (audioSource != null && equipClips.Length > 0)
        {
            AudioClip clipToPlay = GetRandomUniqueClip(equipClips, ref lastEquipIndex);
            audioSource.PlayOneShot(clipToPlay);
        }

        if (equipParticles != null)
        {
            equipParticles.Stop();
            equipParticles.Play();
        }
    }

    public void HideNameInput()
    {
        if (characterNameInput != null)
        {
            characterNameInput.gameObject.SetActive(false);
        }
    }

    public void ChangeNameFont(TMP_FontAsset newFont)
    {
        if (characterNameInput != null)
        {
            // Se o campo está invisível (é a primeira vez que clica numa fonte)
            if (!characterNameInput.gameObject.activeSelf)
            {
                // Ativa o campo na tela
                characterNameInput.gameObject.SetActive(true);
                // Define o nome padrão
                characterNameInput.text = "Wonder Doll";
            }

            // Aplica a nova fonte escolhida (mantendo o nome que lá estiver)
            characterNameInput.textComponent.font = newFont;
        }
    }

    public void PopulateFontsMenu(List<FontItemSO> fontItems, Func<FontItemSO, bool> isUnlockedMethod)
    {
        if (tabTransitionCoroutine != null) StopCoroutine(tabTransitionCoroutine);
        tabTransitionCoroutine = StartCoroutine(FadeAndPopulateFontsRoutine(fontItems, isUnlockedMethod));
    }

    public void SetCameraZoom(bool wantsZoom)
    {
        // Evita reiniciar a animação se já estiver no estado correto
        if (wantsZoom == isCurrentlyZoomed) return;

        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);

        if (wantsZoom)
        {
            // Memoriza a posição e o tamanho normal (útil caso o PC/Mobile tenham tamanhos diferentes)
            if (mainCamera != null)
            {
                savedNormalPosition = mainCamera.transform.position;
                savedNormalSize = mainCamera.orthographicSize;
            }
        }

        isCurrentlyZoomed = wantsZoom;
        zoomCoroutine = StartCoroutine(ZoomRoutine(wantsZoom));
    }

    public void SpawnPet(ClothingItemSO petItem)
    {
        if (petPrefab == null || petSpawnPointBase == null) return;

        // Cria o clone no ponto do mundo
        GameObject newPetGo = Instantiate(petPrefab, petSpawnPointBase.position, Quaternion.identity);
        DraggablePet petScript = newPetGo.GetComponent<DraggablePet>();

        // Pega a imagem usando a interface padrão e envia para o Pet
        IClothingVisual visual = new DefaultClothingVisual(petItem);
        petScript.Setup(visual.GetSprite());

        // Fica a ouvir quando este clone for clicado
        petScript.OnPetClicked += OpenPetMenu;
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

    public void ClickAudioToggleButton()
    {
        OnAudioToggleClicked?.Invoke();
    }

    public void UpdateAudioButtonIcon(bool isMuted)
    {
        if (audioBlockIcon != null)
        {
            // Se estiver mutado, ativa o ícone de bloqueio. Se tiver som, desativa.
            audioBlockIcon.SetActive(isMuted);
        }
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

    private AudioClip GetRandomUniqueClip(AudioClip[] clipsArray, ref int lastIndex)
    {
        // Proteção: se o array só tiver 1 som, toca-o direto para não criar um loop infinito
        if (clipsArray.Length == 1) return clipsArray[0];

        int randomIndex = lastIndex;

        // Continua a sortear enquanto o número for igual ao último que tocou
        while (randomIndex == lastIndex)
        {
            randomIndex = UnityEngine.Random.Range(0, clipsArray.Length);
        }

        // Atualiza a memória com o novo som que vai tocar agora
        lastIndex = randomIndex;
        return clipsArray[randomIndex];
    }

    private IEnumerator ZoomRoutine(bool isZoomed)
    {
        if (mainCamera == null) yield break;

        // Se quiser zoom, vai para o alvo do rosto. Se não, volta para o salvo.
        Vector3 targetPos = isZoomed && faceZoomTarget != null ?
            new Vector3(faceZoomTarget.position.x, faceZoomTarget.position.y, savedNormalPosition.z) :
            savedNormalPosition;

        float targetSize = isZoomed ? zoomedOrthographicSize : savedNormalSize;

        // Move e redimensiona suavemente
        while (Mathf.Abs(mainCamera.orthographicSize - targetSize) > 0.01f || Vector3.Distance(mainCamera.transform.position, targetPos) > 0.01f)
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        // Garante o valor final exato
        mainCamera.orthographicSize = targetSize;
        mainCamera.transform.position = targetPos;
    }

    private void OpenPetMenu(DraggablePet clickedPet)
    {
        activePet = clickedPet;
        if (petContextMenu) petContextMenu.SetActive(true);
    }

    private void ClosePetMenu()
    {
        if (petContextMenu) petContextMenu.SetActive(false);
        activePet = null;
    }

    private void FlipActivePet()
    {
        if (activePet != null) activePet.Flip();
    }

    private void DeleteActivePet()
    {
        if (activePet != null)
        {
            Destroy(activePet.gameObject);
            ClosePetMenu();
        }
    }
}