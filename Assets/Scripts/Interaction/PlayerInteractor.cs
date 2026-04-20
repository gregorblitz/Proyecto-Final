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
        // FIX PRINCIPAL: delegar al FlashlightController en vez de solo cambiar un bool
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
}