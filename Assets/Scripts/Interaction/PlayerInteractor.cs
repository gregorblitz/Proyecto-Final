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
    

    private void Awake()
    {
        // Obtenemos la referencia al controlador de ítems
        systems = GetComponent<SystemsController>();
    }

    private void Start() {
        interactAction = InputSystem.actions.FindAction("Interact");
        useInventoryAction = InputSystem.actions.FindAction("UseInventory");
        flashlightAction = InputSystem.actions.FindAction("Flashlight");

        inventoryPanel = GameObject.FindFirstObjectByType<InventoryPanelUI>();
    }

    private void Update() {
        if (interactAction.WasPressedThisFrame())
        {
            doCollect = true;
            ExecuteInteraction();        
        }

        if (interactAction.WasReleasedThisFrame()) doCollect = false;
        if (useInventoryAction.WasPressedThisFrame()) inventoryPanel.inventoryPanel.transform.GetChild(inventoryPanel.selectedSlotIndex).GetComponent<InventorySlotUI>().UseThisItem();        
        if (flashlightAction.WasPressedThisFrame()) ExecuteFlashLight();
 
        
    }

    // Este método se vincula en el Player Input (Mensaje: OnInteract)
    public void ExecuteInteraction()
    {
        // Solo actuamos cuando se presiona la tecla (no cuando se suelta)
        if (currentInteractable != null)
        {
            // Enviamos el ítem que tengas equipado en el SystemsController           
            currentInteractable.Interact(inventoryPanel.inventoryPanel.transform.GetChild(inventoryPanel.selectedSlotIndex).GetComponent<InventorySlotUI>().currentItem);
        }
    }

    public void ExecuteFlashLight()
    {
        if (isFlashlightOn)
            {
                Debug.Log("light is turned off");
                isFlashlightOn = false;
            }

            else
            {
                Debug.Log("light is turned on");
                isFlashlightOn = true;
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