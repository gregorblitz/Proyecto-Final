// Fausto A. Gómez
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    public float range = 3.5f;
    public LayerMask interactLayer;
    public Transform cam;
    private SystemsController systems;

    // SISTEMA DE ARMA PARA ATAQUE
    [Header("Arma para ataque")]
    public GameObject pickaxeInHand; // Poner aqui la pica
    private Animator animator;

    /*modificacion del metodo start para incluir el arma
    private void Start() => systems = GetComponent<SystemsController>();
    */
    private void Start()
    {
        systems = GetComponent<SystemsController>();

        // Busca el animator en el modelo 3D del jugador
        animator = GetComponentInChildren<Animator>();
    }

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
    
    // METODO PARA EQUIPAR PICA
    // Funcion que se llama desde el sistema de inventario cuando selecciona la pica
    public void EquipPickaxe()
    {
        if (pickaxeInHand != null)
        {
            // Encendemos la pica visualmente
            pickaxeInHand.SetActive(true);
            
            if (animator != null)
            {
                // Le decimos al Animator que cambie la postura de las manos
                animator.SetBool("isArmed", true); 
            }
            
            Debug.Log("Pica equipada y lista para atacar");
        }
        else
        {
            Debug.LogWarning("No hay ningun objeto pica en el script ");
        }
    }
}