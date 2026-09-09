using TMPro;
using UnityEngine;

public class UIPrompt : MonoBehaviour
{
    public static UIPrompt Instance { get; private set; }

    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector3 worldOffset;

    private RectTransform promptRectTransform;

    private void Awake()
    {
        Instance = this;

        if (promptText == null)
            promptText = GetComponent<TMP_Text>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        promptRectTransform = promptText != null
            ? promptText.rectTransform
            : GetComponent<RectTransform>();

        Hide();
    }

    public void Show(string message, Vector3 worldPosition)
    {
        if (promptText == null || canvas == null)
            return;

        promptText.text = message;
        SetPosition(worldPosition + worldOffset);
        promptText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    private void SetPosition(Vector3 worldPosition)
    {
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            promptRectTransform.position = worldPosition;
            return;
        }

        Camera targetCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            targetCamera,
            out Vector2 localPosition);
        promptRectTransform.localPosition = localPosition;
    }
}
