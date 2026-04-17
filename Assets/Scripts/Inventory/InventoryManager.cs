//Fauto A. Gomez
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Datos de Inventario")]
    public List<ItemData> currentItems = new List<ItemData>();
    
    [Header("Configuración")]
    [Tooltip("Cantidad máxima de slots disponibles en el inventario.")]
    public int inventorySize = 4;

    [Header("Dependencias")]
    public InventoryPanelUI inventoryUI;

    [Header("Eventos")]
    public UnityEvent OnItemAdded;
    public UnityEvent OnItemRemoved;
    public UnityEvent OnInventoryFull;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Opcional: NoDestroyOnLoad(gameObject);
        }
    }

    public bool AddItem(ItemData itemToAdd)
    {
        if (currentItems.Count < inventorySize)
        {
            currentItems.Add(itemToAdd);

            // Intentamos buscar un slot visualmente
            GameObject availableSlot = inventoryUI.GetAvailableSlot();
            if (availableSlot != null)
            {
                // Asignamos el ítem al slot visual
                availableSlot.GetComponent<InventorySlotUI>().SetSlot(itemToAdd);
                
                // Disparamos el evento de que se añadió
                OnItemAdded?.Invoke();
                return true;
            }
            else
            {
                Debug.LogError("Inventario lógico tiene espacio, pero la UI no tiene slots disponibles.");
                return false;
            }
        }
        else
        {
            Debug.Log("El inventario está lleno.");
            OnInventoryFull?.Invoke();
            return false;
        }
    }

    public void RemoveItem(ItemData itemToRemove)
    {
        if (currentItems.Contains(itemToRemove))
        {
            currentItems.Remove(itemToRemove);
            OnItemRemoved?.Invoke();
            Debug.Log($"Ítem {itemToRemove.itemName} eliminado del inventario.");
        }
    }
}