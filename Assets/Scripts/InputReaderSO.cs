using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "DressUp/Input Reader")]
public class InputReaderSO : ScriptableObject, GameInputs.IPlayerActions
{
    // Evento disparado enviando a coordenada da tela onde ocorreu o clique/toque
    public event Action<Vector2> OnInteract;

    private GameInputs inputActions;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            // GameInputs é a classe C# gerada automaticamente pelo seu Input Action Asset
            inputActions = new GameInputs();
            inputActions.Player.SetCallbacks(this);
        }
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    // Método obrigatório da interface IPlayerActions que criamos no Asset
    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Lê a posição exata do Pointer atual (Mouse ou Dedo)
            Vector2 pointerPosition = Pointer.current.position.ReadValue();
            OnInteract?.Invoke(pointerPosition);
        }
    }
}