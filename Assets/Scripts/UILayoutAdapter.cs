using UnityEngine;

public class UILayoutAdapter : MonoBehaviour
{
    [Header("Os 3 Paineis da UI")]
    [SerializeField] private RectTransform colorPanel;
    [SerializeField] private RectTransform tabsPanel;
    [SerializeField] private RectTransform itemsPanel;
    [SerializeField] private RectTransform screenshotPanel;

    [Header("Contentores no PC (Paisagem)")]
    [SerializeField] private RectTransform landscapeColorParent;
    [SerializeField] private RectTransform landscapeTabsParent;
    [SerializeField] private RectTransform landscapeItemsParent;
    [SerializeField] private RectTransform landscapeScreenshotParent;


    [Header("Contentores no Mobile (Retrato)")]
    [SerializeField] private RectTransform portraitColorParent;
    [SerializeField] private RectTransform portraitTabsParent;
    [SerializeField] private RectTransform portraitItemsParent;
    [SerializeField] private RectTransform portraitScreenshotParent;

    [Header("Personagem")]
    [Tooltip("O objeto pai que contém o corpo e as roupas")]
    [SerializeField] private Transform characterRoot;
    [SerializeField] private Transform landscapeCharacterTarget;
    [SerializeField] private Transform portraitCharacterTarget;

    [Header("Camera Framing")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float landscapeCameraY = 0f;
    [SerializeField] private float portraitCameraY = 0f;

    [Header("Nome do Personagem")]
    [SerializeField] private Transform characterNameInput;
    [SerializeField] private Transform namePositionLandscape;
    [SerializeField] private Transform namePositionPortrait;

    [Header("Pet Spawn Point")]
    [Tooltip("O ponto de origem oficial que está ligado ao script DraggablePet")]
    [SerializeField] private Transform petSpawnPointBase;
    [SerializeField] private Transform petSpawnTargetLandscape;
    [SerializeField] private Transform petSpawnTargetPortrait;

    private bool isCurrentlyPortrait;

    private void Start()
    {
        ForceUpdateLayout(Screen.height > Screen.width);
    }

    private void Update()
    {
        bool isPortrait = Screen.height > Screen.width;
        if (isPortrait != isCurrentlyPortrait)
        {
            ForceUpdateLayout(isPortrait);
        }
    }

    private void ForceUpdateLayout(bool portraitMode)
    {
        isCurrentlyPortrait = portraitMode;

        if (isCurrentlyPortrait)
        {
            // UI para Mobile
            MovePanel(colorPanel, portraitColorParent);
            MovePanel(tabsPanel, portraitTabsParent);
            MovePanel(itemsPanel, portraitItemsParent);
            MovePanel(screenshotPanel, portraitScreenshotParent);
            MoveName(namePositionPortrait);
            MovePet(petSpawnTargetPortrait);

            // Personagem para Mobile
            MoveCharacter(portraitCharacterTarget);
            AdjustCamera(portraitCameraY);
        }
        else
        {
            // UI para PC
            MovePanel(colorPanel, landscapeColorParent);
            MovePanel(tabsPanel, landscapeTabsParent);
            MovePanel(itemsPanel, landscapeItemsParent);
            MovePanel(screenshotPanel, landscapeScreenshotParent);
            MoveName(namePositionLandscape);
            MovePet(petSpawnTargetLandscape);

            // Personagem para PC
            MoveCharacter(landscapeCharacterTarget);
            AdjustCamera(landscapeCameraY);
        }
    }

    private void MovePanel(RectTransform panel, RectTransform newParent)
    {
        if (panel == null || newParent == null) return;
        panel.SetParent(newParent, false);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.sizeDelta = Vector2.zero;
        panel.anchoredPosition = Vector2.zero;
    }

    private void MoveCharacter(Transform target)
    {
        if (characterRoot == null || target == null) return;

        // Copia a posição e a escala exata da âncora alvo
        characterRoot.position = target.position;
        characterRoot.localScale = target.localScale;
    }

    private void MoveName(Transform target)
    {
        if (characterNameInput == null || target == null) return;
        // Copia a posição e a escala exata da âncora alvo
        characterNameInput.position = target.position;
        characterNameInput.localScale = target.localScale;
    }

    private void MovePet(Transform target)
    {
        if (petSpawnPointBase == null || target == null) return;
        // Copia a posição e a escala exata da âncora alvo
        petSpawnPointBase.position = target.position;
        petSpawnPointBase.localScale = target.localScale;
    }

    private void AdjustCamera(float targetY)
    {
        if (mainCamera != null)
        {
            Vector3 camPos = mainCamera.transform.position;
            camPos.y = targetY;
            mainCamera.transform.position = camPos;
        }
    }
}