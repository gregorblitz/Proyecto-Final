// Fausto A. Gómez
using UnityEngine;
using UnityEngine.InputSystem;

// Sistema de crafteo simple: Linterna + Batería = Linterna con batería llena
// Por ahora solo tiene esta receta, se puede expandir después
public class CraftingSystem : MonoBehaviour
{
    [Header("Recetas")]
    public ItemData flashlightItem;
    public ItemData batteryItem;

    [Header("Tecla de Crafteo")]
    public KeyCode craftKey = KeyCode.R;

    private InventoryManager inventory;
    private FlashlightController flashlightController;

    private void Start()
    {
        inventory = InventoryManager.Instance;
        flashlightController = GameObject.FindWithTag("Player")
                                         .GetComponentInChildren<FlashlightController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(craftKey))
        {
            Debug.Log("Intentando craftear... (tecla: " + craftKey + ")");
            TryCraftFlashlightWithBattery();
        }
    }

    public void TryCraftFlashlightWithBattery()
    {
        bool hasFlashlight = inventory.currentItems.Contains(flashlightItem);
        bool hasBattery = inventory.currentItems.Contains(batteryItem);

        if (hasFlashlight && hasBattery)
        {
            inventory.RemoveItem(batteryItem);

            if (flashlightController != null)
            {
                flashlightController.RechargeBattery(flashlightController.maxBattery);
                Debug.Log("¡Crafteo exitoso! Linterna recargada al máximo.");
            }

            RefreshBatterySlotUI();
        }
        else
        {
            if (!hasFlashlight) Debug.Log("Crafteo fallido: no tienes linterna en el inventario");
            if (!hasBattery) Debug.Log("Crafteo fallido: no tienes batería en el inventario");
        }
    }

    private void RefreshBatterySlotUI()
    {
        InventoryPanelUI panel = FindFirstObjectByType<InventoryPanelUI>();
        if (panel == null) return;

        foreach (Transform child in panel.slotsParent)
        {
            InventorySlotUI slot = child.GetComponent<InventorySlotUI>();
            if (slot != null && slot.currentItem == batteryItem)
            {
                slot.ClearSlot();
                break;
            }
        }
    }
}