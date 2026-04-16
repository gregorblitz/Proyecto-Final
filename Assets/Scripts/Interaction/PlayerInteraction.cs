// Fausto A. Gómez
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float range = 3.5f;
    public LayerMask interactLayer;
    public Transform cam;
    private SystemsController systems;

    private void Start() => systems = GetComponent<SystemsController>();

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        Ray ray = new Ray(cam.position, cam.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, interactLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                // AQUÍ ESTÁ LA CLAVE: Enviamos el ítem seleccionado a la puerta
                interactable.Interact(systems.selectedItem); 
            }
        }
    }
}