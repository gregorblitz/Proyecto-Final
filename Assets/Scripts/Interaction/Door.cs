using UnityEngine;

public class FA_Door : MonoBehaviour, IInteractable
{
    public string requiredID = "Llave_Sotano";

    public void Interact(ItemData itemInHand)
    {
        if (itemInHand == null)
        {
            Debug.LogWarning("¡INTERACCIÓN FALLIDA! No tienes NADA equipado. Abre el inventario y haz clic en la llave.");
            return;
        }

        Debug.Log("Intentando abrir con: " + itemInHand.interactionID + " | Se necesita: " + requiredID);

        if (itemInHand.interactionID == requiredID)
        {
            Debug.Log("¡ÉXITO! Las IDs coinciden.");
            gameObject.SetActive(false); 
        }
        else
        {
            Debug.LogError("¡ERROR! El ID no coincide. Revisa las mayúsculas en el ScriptableObject y en la Puerta.");
        }
    }
}