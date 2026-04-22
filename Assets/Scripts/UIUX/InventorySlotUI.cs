// Fausto A. Gomez
// MODIFICADO: se añadió soporte para el tipo Drill (Taladro)
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
                Vector3 spawnPos = player.transform.position + player.transform.forward * -0.5f;
                Instantiate(currentItem.dropPrefab, spawnPos, Quaternion.identity);

                InventoryManager.Instance.RemoveItem(currentItem);
                ClearSlot();
                
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

        // Marcar como item en mano
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

                FlashlightUI flashUI = FindFirstObjectByType<FlashlightUI>();
                if (flashUI != null) flashUI.ShowFlashlightHUD();
            }

            //return; // NO se consume
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
            return;
        }

        // =========================
        // TALADRO (Drill)
        // =========================
        // El taladro NO se consume, solo se equipa en la mano.
        // Para usarlo, el jugador presiona E cerca de una BreakableWall.
        // PlayerInteractor ya pasa el selectedItem al Interact(), así que no hay que hacer nada extra aquí.
        if (currentItem.type == ItemData.ItemType.Tool)
        {
            // Revisar si es el taladro por su nombre o interactionID
            // (se puede refinar si se crea un subtipo, por ahora usamos el ID)
            if (currentItem.interactionID == "Drill")
            {
                Debug.Log($"[Taladro] '{currentItem.itemName}' listo para usar. Acércate a una pared rompible y presiona E.");

                // Si el jugador tiene un DrillController, notificarlo
                DrillController drillCtrl = player.GetComponent<DrillController>();
                if (drillCtrl != null)
                    drillCtrl.EquipDrill();

                return; // NO se consume
            }

            // Otras herramientas tipo Tool van aquí en el futuro
            Debug.Log($"[Herramienta] {currentItem.itemName} equipada.");
            return;
        }

        // =========================
        // Efectos normales (salud, oxígeno, etc.)
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