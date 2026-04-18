using UnityEngine;
using UnityEngine.InputSystem; // Requerido para el nuevo Input System

public class ItemEffectHandler : MonoBehaviour
{
    private PlayerStatus playerStatus;

    public InputActionAsset inputActions;
    private InputAction useInventoryAction;
    private InputAction flashlightAction;

    private FA_InventoryPanelUI inventoryPanel;
    private bool isFlashlightOn = false;

    // SISTEMA DE ARMAS
    [Header("Sistema de Armas")]
    public GameObject pickaxeInHand; // va la pica apagada en la mano
    private Animator animator;

    private void Awake()
    {
        // Buscamos el componente PlayerStatus en el jugador
        playerStatus = GetComponent<PlayerStatus>();
        animator = GetComponentInChildren<Animator>(); // Busca el Animator para cambiar de postura

        useInventoryAction = InputSystem.actions.FindAction("UseInventory");
        flashlightAction = InputSystem.actions.FindAction("Flashlight");
        
    }

    private void Start() {
        inventoryPanel = GameObject.FindFirstObjectByType<FA_InventoryPanelUI>();
    }

    private void Update() {
        if (useInventoryAction.WasPressedThisFrame())
        {
            Debug.Log(inventoryPanel.selectedSlotIndex);
            inventoryPanel.inventoryPanel.transform.GetChild(inventoryPanel.selectedSlotIndex).GetComponent<InventorySlotUI>().UseThisItem();
        }
         if (flashlightAction.WasPressedThisFrame())
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
            
            //UseItem(inventoryPanel.gameObject.transform.GetChild(inventoryPanel.selectedSlotIndex).GetComponent<InventorySlotUI>().currentItem);
        }
    }

    public void UseItem(ItemData item)
    {
        if (item == null || playerStatus == null) 
        {
            Debug.Log("Slot was empty or playerStatus was missing");
            return;
        }
        // Lógica para Consumibles (Salud, Oxígeno, Locura)
        if (item.isConsumable)
        {
            switch (item.effectType)
            {
                case ItemData.ItemEffect.Heal:
                    playerStatus.ModifyHealth(item.effectAmount);
                    break;
                case ItemData.ItemEffect.RestoreOxygen:
                    playerStatus.ModifyOxygen(item.effectAmount);
                    break;
                case ItemData.ItemEffect.ReduceMadness:
                    playerStatus.ModifyMadness(-item.effectAmount);
                    break;
            }
        }
        else
        {
            // Lógica para Herramientas (Pico, Linterna, etc.)
            Debug.Log($"Activando herramienta: {item.itemName}");
            // Aquí llamarías al sistema de equipo que están desarrollando tus compañeros
            // Verifica si el item es Pica o Pico segun este en ItemData
            if (item.itemName == "Pica" || item.itemName == "Pico") 
            {
                EquipPickaxe();
            }
        }
    }

    // FUNCION PARA EQUIPAR
    // Recoge la pica del suelo -> Va al inventario -> clic izq -> llama a UseThisItem() -> 
    // llama a ItemEffectHandler -> SetActive(true) activa la pica invisible de la mano
    private void EquipPickaxe()
    {
        if (pickaxeInHand != null)
        {
            pickaxeInHand.SetActive(true); // Enciende la pica visualmente
            
            if (animator != null)
            {
                animator.SetBool("isArmed", true); // Opcional: Le avisa al Animator que tienes arma
            }
            
            Debug.Log("Pica en la mano");
        }
        else
        {
            Debug.LogWarning("No hay GameObject pica en el script ItemEffectHandler");
        }
    }

    //ENCENDIDO Y APAGADO DE LOS CONTROLES
    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable(); // Activacion mapa de acciones "Player"
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable(); // Se desactiva si el jugador muere o se pausa
    }
}
