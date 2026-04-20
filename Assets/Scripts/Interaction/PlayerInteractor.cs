// Fausto A. Gómez
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    private IInteractable currentInteractable;
    private SystemsController systems;
    private InventoryPanelUI inventoryPanel;

    public InputActionAsset inputActions;
    private InputAction interactAction;
    private InputAction useInventoryAction;
    private InputAction flashlightAction;
    public bool doCollect;
    public bool isFlashlightOn;

    // NUEVO: referencia directa al controlador de linterna
    private FlashlightController flashlightController;

    // SISTEMA DE ARMA PARA ATAQUE
    [Header("Arma para ataque")]
    public GameObject pickaxeInHand; // Poner aqui la pica
    private Animator animator;

    /*modificacion del metodo start para incluir el arma
    private void Start() => systems = GetComponent<SystemsController>();
    */

    private void Awake()
    {
        systems = GetComponent<SystemsController>();

        // Buscamos la linterna en los hijos del jugador
        flashlightController = GetComponentInChildren<FlashlightController>();
        if (flashlightController == null)
            Debug.LogWarning("PlayerInteractor: no encontró FlashlightController en los hijos");
            
    }

    private void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        useInventoryAction = InputSystem.actions.FindAction("UseInventory");
        flashlightAction = InputSystem.actions.FindAction("Flashlight");

        inventoryPanel = GameObject.FindFirstObjectByType<InventoryPanelUI>();

        // Busca el animator en el modelo 3D del jugador
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (interactAction.WasPressedThisFrame())
        {
            doCollect = true;
            ExecuteInteraction();
        }

        if (interactAction.WasReleasedThisFrame()) doCollect = false;
        if (useInventoryAction.WasPressedThisFrame())
            inventoryPanel.inventoryPanel.transform.GetChild(inventoryPanel.selectedSlotIndex)
                .GetComponent<InventorySlotUI>().UseThisItem();

        // FIX: ahora sí llama al FlashlightController real
        if (flashlightAction.WasPressedThisFrame()) ExecuteFlashLight();
    }

    public void ExecuteInteraction()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact(
                inventoryPanel.inventoryPanel.transform
                    .GetChild(inventoryPanel.selectedSlotIndex)
                    .GetComponent<InventorySlotUI>().currentItem
            );
        }
    }

    public void ExecuteFlashLight()
    {
        // Delegar al FlashlightController en vez de solo cambiar un bool
        if (flashlightController != null)
        {
            flashlightController.TryToggle();
            isFlashlightOn = flashlightController.isOn; // sincronizamos el bool visual
            Debug.Log("Linterna toggled. Estado: " + (isFlashlightOn ? "ON" : "OFF"));
        }
        else
        {
            Debug.LogWarning("No hay FlashlightController. ¿Equipaste la linterna primero?");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();

        if (interactable != null)
        {
            currentInteractable = interactable;

            DoorController door = other.GetComponentInParent<DoorController>();
            if (door != null)
                Debug.Log($"[{door.objectName}] : {door.GetInteractionMessage()}");
            else
                Debug.Log("Cerca de objeto interactuable: " + other.name);
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