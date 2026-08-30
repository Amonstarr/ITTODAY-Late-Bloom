using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public Vector2 PointerPosition;

    public event Action<Vector2> LeftMouseClicked;

    private InputSystem _inputSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _inputSystem = new InputSystem();
    }

    private void OnEnable()
    {
        if (_inputSystem == null)
            return;

        _inputSystem.GameInput.ClickItem.performed += OnClickItem;
        _inputSystem.GameInput.Pointer.performed += OnPointerPosition;

        _inputSystem.Enable();
    }

    private void OnDisable()
    {
        if (_inputSystem == null)
            return;

        _inputSystem.GameInput.ClickItem.performed -= OnClickItem;
        _inputSystem.GameInput.Pointer.performed -= OnPointerPosition;

        _inputSystem.Disable();
    }

    private void OnDestroy()
    {
        _inputSystem?.Dispose();
    }

    private void OnClickItem(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        LeftMouseClicked?.Invoke(PointerPosition);
    }

    private void OnPointerPosition(InputAction.CallbackContext context)
    {
        PointerPosition = context.ReadValue<Vector2>();
    }
}