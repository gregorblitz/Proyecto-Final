//Fauto A. Gomez
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler 
{
    [Header("Componentes UI")]
    public Image icon;
    
    [Header("Datos del Slot")]
    public ItemData currentItem;
    
    private InventoryPanelUI inventoryPanel;

    [Header("Visualización de Selección")]
    public Color selectionColor = Color.yellow;

    private void Awake()
    {
        inventoryPanel = FindFirstObjectByType<InventoryPanelUI>();
    }

    public void SetSlot(ItemData item)
    {
        currentItem = item;
        icon.sprite = item.icon;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        currentItem = null;
        icon.sprite = null;
        icon.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        int myIndex = transform.GetSiblingIndex();
        inventoryPanel.selectedSlotIndex = myIndex;
        inventoryPanel.UpdateSlotSelection();

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            UseThisItem();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            DropItem();
        }
    }

    private void DropItem()
    {
        if (currentItem != null && currentItem.dropPrefab != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 spawnPos = player.transform.position + player.transform.forward * -0.5f ;
                Instantiate(currentItem.dropPrefab, spawnPos, Quaternion.identity);

                InventoryManager.Instance.RemoveItem(currentItem);
                ClearSlot();
                
                // Si tiras el ítem que tenías seleccionado, limpiamos la mano
                SystemsController systems = player.GetComponent<SystemsController>();
                if (systems != null && systems.selectedItem == currentItem) 
                    systems.selectedItem = null;
            }
        }
    }

    public void SetSelected(bool isSelected)
    {
        icon.color = isSelected ? selectionColor : Color.white;
    }

    public void UseThisItem()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // 1. Marcar como item en mano
        SystemsController systems = player.GetComponent<SystemsController>();
        if (systems != null)
        {
            systems.selectedItem = currentItem; 
            Debug.Log($"[Inventario] {currentItem.itemName} ahora está en la mano.");
        }

        // =========================
        // LINTERNA
        // =========================
        if (currentItem.type == ItemData.ItemType.Flashlight)
        {
            FlashlightController fc = player.GetComponentInChildren<FlashlightController>();
            if (fc != null)
            {
                fc.EquipFlashlight(currentItem);

                // Mostrar HUD
                FlashlightUI flashUI = FindFirstObjectByType<FlashlightUI>();
                if (flashUI != null) flashUI.ShowFlashlightHUD();
            }
            return; // NO se consume
        }

        // =========================
        // BATERÍA
        // =========================
        if (currentItem.type == ItemData.ItemType.Battery)
        {
            FlashlightController fc = player.GetComponentInChildren<FlashlightController>();
            if (fc != null)
            {
                fc.RechargeBattery(currentItem.batteryCapacity);

                InventoryManager.Instance.RemoveItem(currentItem);
                ClearSlot();
            }
            return; // ya se procesó
        }

        // =========================
        // 2. Efectos normales (salud, oxígeno, etc.)
        // =========================
        ItemEffectHandler effectHandler = player.GetComponent<ItemEffectHandler>();
        if (effectHandler != null && currentItem != null)
        {
            effectHandler.UseItem(currentItem);

            if (currentItem.isConsumable)
            {
                InventoryManager.Instance.RemoveItem(currentItem);
                ClearSlot();
                
                if (systems != null) systems.selectedItem = null;
            }
        }
    }
}