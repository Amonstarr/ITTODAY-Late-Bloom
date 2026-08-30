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

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit2D rayhit = Physics2D.GetRayIntersection(ray);

        if (rayhit.collider != null)
        {
            Debug.Log("Clicked on: " + rayhit.collider.name);
        }
    }
}