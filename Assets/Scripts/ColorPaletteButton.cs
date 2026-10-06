using UnityEngine;
using UnityEngine.UI;

public class ColorPaletteButton : MonoBehaviour
{
    [SerializeField] private Color colorToApply = Color.white;
    [SerializeField] private DressUpView mainView;
    [SerializeField] private Button buttonComponent;
    [SerializeField] private Image buttonImage;

    private void OnValidate()
    {
        // Pinta a imagem do botão automaticamente no Editor para facilitar a sua vida
        if (buttonImage != null)
        {
            buttonImage.color = colorToApply;
        }
    }

    private void Start()
    {
        // Quando clicado, envia a cor para a View principal
        buttonComponent.onClick.AddListener(() => mainView.SelectColor(colorToApply));
    }
}