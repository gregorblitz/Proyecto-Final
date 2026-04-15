//Fauto A. Gomez
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Importante para detectar clicks

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler // Implementamos para click AAA
{
    [Header("Componentes UI")]
    public Image icon;
    
    [Header("Datos del Slot")]
    public ItemData currentItem;
    
    private FA_InventoryPanelUI inventoryPanel;

    [Header("Visualización de Selección")]
    //public GameObject selectionVisual; // Arrastra aquí un objeto hijo que sea el borde resaltado
    public Color selectionColor = Color.yellow;

    private void Awake()
    {
        inventoryPanel = GetComponentInParent<FA_InventoryPanelUI>();
        
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

    // He añadido esto para dar un toque AAA con el Nuevo Input System y detección de ratón
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        int myIndex = transform.GetSiblingIndex();
        inventoryPanel.selectedSlotIndex = myIndex;
        inventoryPanel.UpdateSlotSelection();

        // CLICK IZQUIERDO: Equipar o Usar
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            /*
            // 1. Buscamos el sistema en el jugador y le pasamos el ítem
            SystemsController systems = FindFirstObjectByType<SystemsController>();
            if (systems != null)
            {
                systems.EquipItem(currentItem);
            }
            
            // Ahora preguntamos si el TIPO es Consumable
            if (currentItem.type == ItemData.ItemType.Consumable)
            {
                InventoryManager.Instance.RemoveItem(currentItem);
                ClearSlot();
            }
            
            // 3. Cerramos el inventario automáticamente tras elegir
            if (inventoryPanel != null) 
            {
                inventoryPanel.ToggleInventory(); 
            }
            */
            UseThisItem();
        }
        // CLICK DERECHO: Soltar (Ya lo tienes implementado)
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            DropItem();
        }

        

    }

    private void DropItem()
    {
        if (currentItem != null && currentItem.dropPrefab != null)
        {
            // AAA Touch: Tirar el ítem al suelo frente al jugador, no en un punto fijo.
            // Para esto necesitamos el transform del jugador. Usaré la Tag "Player".
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Instanciar el prefab 1.5 metros frente al jugador, 1 metro arriba.
                Vector3 spawnPos = player.transform.position + player.transform.forward * 1.5f + Vector3.up * 1f;
                Instantiate(currentItem.dropPrefab, spawnPos, Quaternion.identity);

                // Borrarlo del manager y de la UI
                InventoryManager.Instance.RemoveItem(currentItem);
                ClearSlot();
            }
            else
            {
                Debug.LogError("No se encontró un objeto con la Tag 'Player' para tirar el ítem.");
            }
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            icon.color = selectionColor; // Cambia el color del ícono para indicar selección
        }
    }

    public void UseThisItem()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        ItemEffectHandler effectHandler = player.GetComponent<ItemEffectHandler>();

        if (effectHandler != null && currentItem != null)
        {
            effectHandler.UseItem(currentItem);

            if (currentItem.isConsumable)
            {
                InventoryManager.Instance.RemoveItem(currentItem);
                ClearSlot();
            }
        }
    }
}