using UnityEngine;
using UnityEngine.InputSystem; // Requerido para el nuevo Input System

public class ItemEffectHandler : MonoBehaviour
{
    private PlayerStatus playerStatus;

    public InputActionAsset inputActions;
    private InputAction useInventoryAction;
    private InputAction flashlightAction;

    private InventoryPanelUI inventoryPanel;
    private bool isFlashlightOn = false;

    private void Awake()
    {
        // Buscamos el componente PlayerStatus en el jugador
        playerStatus = GetComponent<PlayerStatus>();
        useInventoryAction = InputSystem.actions.FindAction("UseInventory");
        flashlightAction = InputSystem.actions.FindAction("Flashlight");
    }

    private void Start() {
        inventoryPanel = GameObject.FindFirstObjectByType<InventoryPanelUI>();
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
