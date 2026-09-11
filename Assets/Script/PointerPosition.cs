using UnityEngine;

public class PointerPosition : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    private IHoverable currentHoverable;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        if (InputManager.Instance == null)
        {
            return;
        }

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(InputManager.Instance.PointerPosition);
        mousePosition.z = 0f; // Set the z-coordinate to 0 for 2D
        transform.position = mousePosition;

        Collider2D hit = Physics2D.OverlapPoint(mousePosition);

        IHoverable previousHoverable = currentHoverable;
        currentHoverable = null;

        if (hit != null)
        {
            currentHoverable = hit.GetComponent<IHoverable>();
        }

        if (previousHoverable != null && previousHoverable != currentHoverable)
        {
            previousHoverable.OnHoverExit();
        }

        if (currentHoverable != null)
        {
            currentHoverable.OnHoverEnter();
        }
    }
}