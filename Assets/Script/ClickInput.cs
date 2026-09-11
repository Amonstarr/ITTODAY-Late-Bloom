using UnityEngine;

public class ClickInput : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (InputManager.Instance != null)
        {
            InputManager.Instance.LeftMouseClicked += OnClick;
        }
    }



    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.LeftMouseClicked -= OnClick;
        }
    }

    private void OnClick(Vector2 screenPosition)
    {
        if (mainCamera == null)
        {
            return;
        }

        // Jangan proses klik jika UI popup sedang terbuka
        if (LateBloom.Raising.UI.ActionResultPopupUI.IsOpen)
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit2D rayhit = Physics2D.GetRayIntersection(ray);

        if (rayhit.collider != null)
        {
            Debug.Log("Clicked on: " + rayhit.collider.name);

            IInteractable interactable =
                rayhit.collider.GetComponentInParent<IInteractable>();
            interactable?.Interact();
        }
    }
}