using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class DraggablePet : MonoBehaviour
{
    private Vector2 offset;
    private Camera mainCamera;
    private BoxCollider2D col;
    private SpriteRenderer spr;

    private bool isDragging = false;
    private bool wasDragged = false;
    private Vector2 pointerDownPos;

    // Variável estática (partilhada por todos os pets) para gerir quem está na frente
    private static int topSortingOrder = 50;

    public event Action<DraggablePet> OnPetClicked;

    private void Awake()
    {
        mainCamera = Camera.main;
        col = GetComponent<BoxCollider2D>();
        spr = GetComponent<SpriteRenderer>();
    }

    public void Setup(Sprite sprite)
    {
        spr.sprite = sprite;
        col.size = sprite.bounds.size;
        BringToFront(); // O novo pet nasce sempre por cima de todos
    }

    public void Flip()
    {
        spr.flipX = !spr.flipX;
    }

    // Puxa este pet para a frente de tudo
    private void BringToFront()
    {
        topSortingOrder++;
        spr.sortingOrder = topSortingOrder;
    }

    private void Update()
    {
        if (mainCamera == null || Pointer.current == null) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPos);

        // PRESSIONOU
        if (Pointer.current.press.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            // Se o rato tocou neste pet
            if (col.OverlapPoint(worldPos))
            {
                // Verifica todos os colisores que estão debaixo do rato neste exato milissegundo
                Collider2D[] allHits = Physics2D.OverlapPointAll(worldPos);
                bool amITheTopOne = true;

                // Compara a camada deste pet com os outros que também foram tocados
                foreach (Collider2D hit in allHits)
                {
                    if (hit != col) // Se não for eu mesmo
                    {
                        SpriteRenderer otherSpr = hit.GetComponent<SpriteRenderer>();
                        if (otherSpr != null && otherSpr.sortingOrder > spr.sortingOrder)
                        {
                            amITheTopOne = false; // Tem alguém acima de mim!
                            break;
                        }
                    }
                }

                // Só arrasta se for o pet mais acima
                if (amITheTopOne)
                {
                    BringToFront(); // Ao clicar, garante que ele salta para a frente dos outros
                    isDragging = true;
                    wasDragged = false;
                    pointerDownPos = screenPos;
                    offset = (Vector2)transform.position - worldPos;
                }
            }
        }

        // SOLTOU
        if (Pointer.current.press.wasReleasedThisFrame)
        {
            if (isDragging)
            {
                if (Vector2.Distance(pointerDownPos, screenPos) < 15f && !wasDragged)
                {
                    OnPetClicked?.Invoke(this);
                }
            }
            isDragging = false;
        }

        // ARRASTANDO
        if (isDragging)
        {
            if (Vector2.Distance(pointerDownPos, screenPos) > 15f) wasDragged = true;
            transform.position = new Vector3(worldPos.x + offset.x, worldPos.y + offset.y, transform.position.z);
        }
    }
}