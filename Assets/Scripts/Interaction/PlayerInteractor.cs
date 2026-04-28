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

    // OBJETOS EN MANO 
    [Header("Objetos en manos")]
    public GameObject pickaxeInHand;    // Poner aqui la pica
    public GameObject flashlightInHand; // Poner aqui la lampara
    private Animator animator;

    /*modificacion del metodo start para incluir el arma
    private void Start() => systems = GetComponent<SystemsController>();
    */

    private void Awake()
    {
        systems = GetComponent<SystemsController>();

        // Buscamos la linterna en los hijos del jugador
        // Agrego el true para que encuentre la lampara incluso si el obj empieza OFF/desequipado
        flashlightController = GetComponentInChildren<FlashlightController>();
        if (flashlightController == null)
            Debug.LogWarning("PlayerInteractor: no encontró FlashlightController en los hijos");

    }

    private void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        useInventoryAction = InputSystem.actions.FindAction("UseInventory");
        flashlightAction = InputSystem.actions.FindAction("Flashlight");

        // *****Llave no funciona porque inventory panel UI esta apagado
        // Usas FindAnyObjectByType incluyendo objetos inactivos para que 
        // lo encuentre si el inventario esta cerrado al empezar a jugar
        inventoryPanel = FindAnyObjectByType<InventoryPanelUI>(FindObjectsInactive.Include);
        //inventoryPanel = GameObject.FindFirstObjectByType<InventoryPanelUI>();

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
            // ***BUG llave no abre puerta 
            //Busca en el slotsParent de forma segura
            if (inventoryPanel != null && inventoryPanel.slotsParent != null)
            {
                if (inventoryPanel.slotsParent.childCount > inventoryPanel.selectedSlotIndex)
                {
                    InventorySlotUI slot = inventoryPanel.slotsParent.GetChild(inventoryPanel.selectedSlotIndex).GetComponent<InventorySlotUI>();
                    if (slot != null) slot.UseThisItem();
                }
            }
            //inventoryPanel.inventoryPanel.transform.GetChild(inventoryPanel.selectedSlotIndex)
            //    .GetComponent<InventorySlotUI>().UseThisItem();

            // FIX: ahora sí llama al FlashlightController real
            if (flashlightAction.WasPressedThisFrame()) ExecuteFlashLight();
    }

    public void ExecuteInteraction()
    {
        if (currentInteractable != null)
        {
            //***BUG llave no abre puerta
            ItemData itemToUse = null;

            // Extraemos el ítem de forma segura, usando el slotsParent en lugar del inventoryPanel visual
            if (inventoryPanel != null && inventoryPanel.slotsParent != null)
            {
                if (inventoryPanel.slotsParent.childCount > inventoryPanel.selectedSlotIndex)
                {
                    InventorySlotUI slot = inventoryPanel.slotsParent.GetChild(inventoryPanel.selectedSlotIndex).GetComponent<InventorySlotUI>();
                    if (slot != null)
                    {
                        itemToUse = slot.currentItem;
                    }
                }
            }

            // Enviamos el ítem seleccionado a la puerta (si tus manos están vacías, le enviará un 'null', 
            // y la puerta simplemente te dirá que necesitas la llave).
            currentInteractable.Interact(itemToUse);
            /*
            currentInteractable.Interact(
                inventoryPanel.inventoryPanel.transform
                    .GetChild(inventoryPanel.selectedSlotIndex)
                    .GetComponent<InventorySlotUI>().currentItem
            );
            */
            //*******************
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

    // Apaga todo lo que haya en las manos (desequipar)
    public void DesequiparTodo()
    {
        if (pickaxeInHand != null) pickaxeInHand.SetActive(false);
        if (flashlightInHand != null) flashlightInHand.SetActive(false);

        // Baja las manos si se desequipa la pica
        if (animator != null)
        {
            animator.SetBool("isArmed", false);
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

    // Equipar la lampara
    public void EquipFlashlight()
    {

        if (flashlightInHand != null)
        {
            flashlightInHand.SetActive(true); // Enciende el modelo 3D lampara
            Debug.Log("Lampara sostenida en la mano");
        }
        else
        {
            Debug.LogWarning("No hay ningun objeto lampara en el script");
        }
    }

    // Para desequiparlos manualmente 
    
    public void UnequipPickaxe()
    {
        if (pickaxeInHand != null) pickaxeInHand.SetActive(false);
        if (animator != null) animator.SetBool("isArmed", false);
    }

    public void UnequipFlashlight()
    {
        if (flashlightInHand != null) flashlightInHand.SetActive(false);
    }
}