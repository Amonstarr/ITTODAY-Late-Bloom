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
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(InputManager.Instance.PointerPosition);
        mousePosition.z = 0f; // Set the z-coordinate to 0 for 2D
        transform.position = mousePosition;

        Collider2D hit =
       Physics2D.OverlapPoint(mousePosition);

        currentHoverable = null;

        if (hit != null)
        {
            currentHoverable =
                hit.GetComponent<IHoverable>();
        }
        if (currentHoverable != null)
        {
            currentHoverable.OnHoverEnter();
        }
        else
        {
            if (currentHoverable != null)
            {
                currentHoverable.OnHoverExit();
            }
        }
    }
}