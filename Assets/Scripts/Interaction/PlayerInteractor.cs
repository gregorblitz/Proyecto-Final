// Fausto A. Gómez
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    private IInteractable currentInteractable;
    private SystemsController systems;

    private void Awake()
    {
        // Obtenemos la referencia al controlador de ítems
        systems = GetComponent<SystemsController>();
    }

    // Este método se vincula en el Player Input (Mensaje: OnInteract)
    public void OnInteract(InputAction.CallbackContext context)
    {
        // Solo actuamos cuando se presiona la tecla (no cuando se suelta)
        if (!context.started) return;

        if (currentInteractable != null)
        {
            // Enviamos el ítem que tengas equipado en el SystemsController
            currentInteractable.Interact(systems.selectedItem);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Buscamos si el objeto o sus padres tienen la interfaz
        IInteractable interactable = other.GetComponentInParent<IInteractable>();
        
        if (interactable != null)
        {
            currentInteractable = interactable;

            // Verificamos si es específicamente una puerta para mostrar el mensaje de la llave
            DoorController door = other.GetComponentInParent<DoorController>();
            if (door != null)
            {
                Debug.Log($"[{door.objectName}] : {door.GetInteractionMessage()}");
            }
            else
            {
                Debug.Log("Cerca de objeto interactuable: " + other.name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable = null;
            Debug.Log("Te alejaste del objeto");
        }
    }
}