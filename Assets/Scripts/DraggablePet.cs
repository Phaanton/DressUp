using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class DraggablePet : MonoBehaviour
{
    public Transform defaultSpawnPoint;

    private Vector2 offset;
    private Camera mainCamera;
    private BoxCollider2D col;
    private bool isDragging = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        col = GetComponent<BoxCollider2D>();
    }

    public void ResetPosition(Sprite newSprite)
    {
        if (defaultSpawnPoint != null) transform.position = defaultSpawnPoint.position;
        if (newSprite != null) col.size = newSprite.bounds.size;
    }

    private void Update()
    {
        if (mainCamera == null) return;

        // Proteção: Se não houver mouse ou tela de toque conectada, não faz nada
        if (Pointer.current == null) return;

        // Lê a posição do ponteiro (Mouse ou Touch) do Novo Input System
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPos);

        // Quando APERTA o botão do mouse ou toca na tela
        if (Pointer.current.press.wasPressedThisFrame)
        {
            // Proteção: Se clicou em cima de um botão ou painel da UI, ignora
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // Verifica se as coordenadas batem com a área do BoxCollider2D
            if (col.OverlapPoint(worldPos))
            {
                isDragging = true;
                offset = (Vector2)transform.position - worldPos;
            }
        }

        // Quando SOLTA o botão ou levanta o dedo
        if (Pointer.current.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        // Enquanto ESTÁ A ARRASTAR
        if (isDragging)
        {
            transform.position = new Vector3(worldPos.x + offset.x, worldPos.y + offset.y, transform.position.z);
        }
    }
}