using UnityEngine;

public class Item : MonoBehaviour, IInteractable, IHoverable
{
    public void OnHoverEnter()
    {
        Debug.Log("Hovering over: " + gameObject.name);
    }

    public void OnHoverExit()
    {
        Debug.Log("Stopped hovering over: " + gameObject.name);
    }



    public void Interact()
    {
        throw new System.NotImplementedException();
    }
}
