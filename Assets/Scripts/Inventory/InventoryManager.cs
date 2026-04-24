// Fausto A. Gómez
// MODIFICADO: se añadió el método HasSpace() que necesita CraftingSystem
// para saber si hay espacio antes de añadir el resultado del crafteo.

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    // Patrón Singleton: acceso global desde cualquier script
    public static InventoryManager Instance { get; private set; }

    [Header("Configuración del Inventario")]
    public int inventorySize = 8; // Número máximo de ítems

    // Lista de ítems actuales (es pública para que CraftingSystem pueda consultarla)
    public List<ItemData> currentItems = new List<ItemData>();

    [Header("Referencias UI")]
    public InventoryPanelUI inventoryUI;

    private void Awake()
    {
        // Configuramos el Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Intenta agregar un ítem al inventario. Devuelve true si lo logró.
    public bool AddItem(ItemData itemToAdd)
    {
        if (currentItems.Count >= inventorySize)
        {
            Debug.Log("[InventoryManager] Inventario lleno. No se pudo añadir: " + itemToAdd.itemName);
            return false;
        }

        currentItems.Add(itemToAdd);

        // Buscar un slot vacío en la UI y asignarle el ítem
        GameObject availableSlot = inventoryUI?.GetAvailableSlot();
        if (availableSlot != null)
        {
            availableSlot.GetComponent<InventorySlotUI>().SetSlot(itemToAdd);
        }

        Debug.Log("[InventoryManager] Ítem añadido: " + itemToAdd.itemName);
        return true;
    }

    // Elimina un ítem del inventario lógico (la UI se actualiza por separado)
    public void RemoveItem(ItemData itemToRemove)
    {
        if (currentItems.Contains(itemToRemove))
        {
            currentItems.Remove(itemToRemove);
            Debug.Log("[InventoryManager] Ítem eliminado: " + itemToRemove.itemName);
        }
        else
        {
            Debug.LogWarning("[InventoryManager] No se encontró el ítem para eliminar: " + itemToRemove?.itemName);
        }
    }

    // NUEVO: Devuelve true si hay al menos un espacio libre en el inventario.
    // CraftingSystem lo usa antes de agregar el resultado del crafteo.
    public bool HasSpace()
    {
        return currentItems.Count < inventorySize;
    }

    // Verifica si el inventario contiene un ítem específico
    public bool HasItem(ItemData item)
    {
        return currentItems.Contains(item);
    }
}