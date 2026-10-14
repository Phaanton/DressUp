using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class ScreenshotManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void DownloadImageFromBrowser(string filename, string base64);

    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Camera mainCamera;

    [Header("Limites do Personagem (Mundo)")]
    [Tooltip("Coloque aqui o ponto que marca o canto inferior esquerdo")]
    [SerializeField] private Transform bottomLeftPoint;
    [Tooltip("Coloque aqui o ponto que marca o canto superior direito")]
    [SerializeField] private Transform topRightPoint;

    public void TakeScreenshot()
    {
        StartCoroutine(CaptureAndDownload());
    }

    private IEnumerator CaptureAndDownload()
    {
        CanvasGroup canvasGroup = uiCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = uiCanvas.gameObject.AddComponent<CanvasGroup>();

        // 1. A grande diferença: Converte a posição do mundo para pixels lógicos da tela
        Vector3 blScreen = mainCamera.WorldToScreenPoint(bottomLeftPoint.position);
        Vector3 trScreen = mainCamera.WorldToScreenPoint(topRightPoint.position);

        // 2. Esconde a UI
        float originalAlpha = canvasGroup.alpha;
        canvasGroup.alpha = 0f;

        yield return new WaitForEndOfFrame();

        // 3. Captura a tela inteira em pixels físicos
        Texture2D fullScreen = ScreenCapture.CaptureScreenshotAsTexture();

        // Restaura a UI
        canvasGroup.alpha = originalAlpha;

        // 4. Calcula a proporção entre os pixels físicos da foto gigante e os pixels da Unity
        float ratioX = (float)fullScreen.width / (float)Screen.width;
        float ratioY = (float)fullScreen.height / (float)Screen.height;

        // 5. Calcula as coordenadas finais do recorte
        int startX = Mathf.RoundToInt(blScreen.x * ratioX);
        int startY = Mathf.RoundToInt(blScreen.y * ratioY);
        int width = Mathf.RoundToInt((trScreen.x - blScreen.x) * ratioX);
        int height = Mathf.RoundToInt((trScreen.y - blScreen.y) * ratioY);

        // 6. Previne erros caso os pontos saiam da tela
        startX = Mathf.Clamp(startX, 0, fullScreen.width);
        startY = Mathf.Clamp(startY, 0, fullScreen.height);
        width = Mathf.Clamp(width, 1, fullScreen.width - startX);
        height = Mathf.Clamp(height, 1, fullScreen.height - startY);

        // 7. Recorta a foto final
        Color[] pixels = fullScreen.GetPixels(startX, startY, width, height);
        Texture2D croppedImage = new Texture2D(width, height, TextureFormat.RGB24, false);
        croppedImage.SetPixels(pixels);
        croppedImage.Apply();

        Destroy(fullScreen);

        // 8. Baixa a imagem
        byte[] imageBytes = croppedImage.EncodeToPNG();
        string base64String = System.Convert.ToBase64String(imageBytes);
        Destroy(croppedImage);

#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadImageFromBrowser("MeuPersonagem.png", base64String);
#else
        Debug.Log($"Foto OK! Recortado do Mundo: L:{width} A:{height}");
        System.IO.File.WriteAllBytes(Application.dataPath + "/MeuPersonagem.png", imageBytes);
#endif
    }
}