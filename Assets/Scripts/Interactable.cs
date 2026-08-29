using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string interactMessage = "Press E to interact";

    public virtual void Interact()
    {
        Debug.Log("Interacting with base object: " + gameObject.name);
    }
}