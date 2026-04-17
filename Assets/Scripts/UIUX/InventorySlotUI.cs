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
    
    private FA_InventoryPanelUI inventoryPanel;

    [Header("Visualización de Selección")]
    public Color selectionColor = Color.yellow;

    private void Awake()
    {
        inventoryPanel = FindFirstObjectByType<FA_InventoryPanelUI>();
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

        // --- CONEXIÓN SEGURA ---
        
        // 1. Buscamos el SystemsController (tu script) para marcar el ítem como "Activo"
        SystemsController systems = player.GetComponent<SystemsController>();
        if (systems != null)
        {
            systems.selectedItem = currentItem; 
            Debug.Log($"[Inventario] {currentItem.itemName} ahora está en la mano.");
        }

        // 2. Llamamos al efecto (salud, oxígeno, etc.) sin modificar el script de Danna
        ItemEffectHandler effectHandler = player.GetComponent<ItemEffectHandler>();
        if (effectHandler != null && currentItem != null)
        {
            effectHandler.UseItem(currentItem);

            if (currentItem.isConsumable)
            {
                InventoryManager.Instance.RemoveItem(currentItem);
                ClearSlot();
                
                // Si se consume, ya no lo tenemos en la mano
                if (systems != null) systems.selectedItem = null;
            }
        }
    }
}