using UnityEngine;
using System;



public class Item : MonoBehaviour, IInteractable, IHoverable
{
    [SerializeField] private float hoverScaleMultiplier = 1.1f;
    [SerializeField, TextArea] private string promptMessage = "Item dipilih";
    private Vector3 originalScale;
    [SerializeField] private ItemType type;
    [SerializeField] private float value;

    public event Action<ItemType, float> OnItemUsed;
    private bool isHovered;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnHoverEnter()
    {
        if (isHovered)
            return;

        Debug.Log("Hovering over: " + gameObject.name);
        transform.localScale = originalScale * hoverScaleMultiplier;
        isHovered = true;
    }

    public void OnHoverExit()
    {
        if (!isHovered)
            return;

        Debug.Log("Stopped hovering over: " + gameObject.name);
        transform.localScale = originalScale;
        isHovered = false;
    }



    public void Interact()
    {
        UIPrompt.Instance?.Show(promptMessage, transform.position);
        OnItemUsed?.Invoke(type, value);
    }
}
