using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAnimator : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float popScale = 1.1f; // Expande 10%
    [SerializeField] private float animationDuration = 0.1f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        Vector3 targetScale = originalScale * popScale;
        float elapsed = 0f;

        // Fase 1: Expande
        while (elapsed < animationDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / animationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fase 2: Volta ao tamanho original
        elapsed = 0f;
        while (elapsed < animationDuration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / animationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}