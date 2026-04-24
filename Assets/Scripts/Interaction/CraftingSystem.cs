// Fausto A. Gómez
// Sistema de crafteo ampliado con recetas visuales en UI.
// Recetas disponibles:
//   1. Linterna + Batería          → Recarga la linterna (receta original)
//   2. Parte1Taladro + Parte2Taladro → Taladro Completo
//   3. LámparaOriginal + LuzEspectral → Lámpara Reveladora

using UnityEngine;
using System.Collections.Generic;

public class CraftingSystem : MonoBehaviour
{
    // ─── RECETA ORIGINAL: Linterna + Batería ────────────────────────────────
    [Header("Receta - Linterna")]
    public ItemData flashlightItem;
    public ItemData batteryItem;

    // ─── RECETA: Taladro ─────────────────────────────────────────────────────
    [Header("Receta - Taladro Completo")]
    public ItemData parte1Taladro;       // Pieza 1 del taladro
    public ItemData parte2Taladro;       // Pieza 2 del taladro
    public ItemData taladroCompleto;     // Resultado del crafteo

    // ─── RECETA: Lámpara Reveladora ──────────────────────────────────────────
    [Header("Receta - Lámpara Reveladora")]
    public ItemData lamparaOriginal;     // Lámpara base
    public ItemData luzEspectral;        // Componente especial
    public ItemData lamparaReveladora;   // Resultado del crafteo

    // ─── REFERENCIAS ─────────────────────────────────────────────────────────
    private InventoryManager inventory;
    private FlashlightController flashlightController;

    // Lista de todas las recetas para que la UI pueda mostrarlas
    // Cada receta es: (ingrediente1, ingrediente2, resultado)
    [System.Serializable]
    public class Receta
    {
        public ItemData ingrediente1;
        public ItemData ingrediente2;
        public ItemData resultado; // null = receta especial (ej: recarga de linterna)
        public string nombreReceta; // Para mostrar en la UI
    }

    // La UI de crafteo (CraftingUI.cs) lee esto para mostrar las recetas disponibles
    [HideInInspector]
    public List<Receta> todasLasRecetas = new List<Receta>();

    // ─── INICIO ───────────────────────────────────────────────────────────────
    private void Start()
    {
        inventory = InventoryManager.Instance;

        // Buscamos la linterna en el jugador (solo para la receta de recarga)
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            flashlightController = player.GetComponentInChildren<FlashlightController>();

        // Registramos las recetas para que la UI pueda mostrarlas
        RegistrarRecetas();
    }

    // Llena la lista de recetas; la UI la usa para mostrar al jugador qué se puede craftear
    private void RegistrarRecetas()
    {
        todasLasRecetas.Clear();

        // Receta 1: Linterna + Batería (recarga)
        todasLasRecetas.Add(new Receta
        {
            ingrediente1 = flashlightItem,
            ingrediente2 = batteryItem,
            resultado = null, // resultado especial: recarga
            nombreReceta = "Recargar Linterna"
        });

        // Receta 2: Taladro Completo
        todasLasRecetas.Add(new Receta
        {
            ingrediente1 = parte1Taladro,
            ingrediente2 = parte2Taladro,
            resultado = taladroCompleto,
            nombreReceta = "Taladro Completo"
        });

        // Receta 3: Lámpara Reveladora
        todasLasRecetas.Add(new Receta
        {
            ingrediente1 = lamparaOriginal,
            ingrediente2 = luzEspectral,
            resultado = lamparaReveladora,
            nombreReceta = "Lámpara Reveladora"
        });
    }

    // ─── CRAFTEAR POR ÍNDICE ──────────────────────────────────────────────────
    // La UI llama esto cuando el jugador hace clic en "Craftear" de una receta
    public bool TryCraft(int recetaIndex)
    {
        if (recetaIndex < 0 || recetaIndex >= todasLasRecetas.Count)
        {
            Debug.LogWarning("[CraftingSystem] Índice de receta inválido: " + recetaIndex);
            return false;
        }

        Receta receta = todasLasRecetas[recetaIndex];

        // Caso especial: recarga de linterna (receta 0)
        if (receta.resultado == null)
        {
            return TryCraftFlashlightWithBattery();
        }

        // Caso general: dos piezas → un resultado
        return TryCraftGenerico(receta);
    }

    // ─── RECETA ORIGINAL: Linterna + Batería ─────────────────────────────────
    public bool TryCraftFlashlightWithBattery()
    {
        bool hasFlashlight = inventory.currentItems.Contains(flashlightItem);
        bool hasBattery    = inventory.currentItems.Contains(batteryItem);

        if (hasFlashlight && hasBattery)
        {
            // Consumimos la batería, la linterna queda en el inventario
            inventory.RemoveItem(batteryItem);

            // Recargamos la linterna al máximo
            if (flashlightController != null)
            {
                flashlightController.RechargeBattery(flashlightController.maxBattery);
                Debug.Log("[CraftingSystem] ¡Crafteo exitoso! Linterna recargada al máximo.");
            }

            RefrescarSlotUI(batteryItem); // Actualizamos visualmente el slot de la batería
            return true;
        }
        else
        {
            if (!hasFlashlight) Debug.Log("[CraftingSystem] Crafteo fallido: no tienes la linterna.");
            if (!hasBattery)    Debug.Log("[CraftingSystem] Crafteo fallido: no tienes la batería.");
            return false;
        }
    }

    // ─── CRAFTEO GENÉRICO: Pieza1 + Pieza2 → Resultado ───────────────────────
    private bool TryCraftGenerico(Receta receta)
    {
        bool tieneIngrediente1 = inventory.currentItems.Contains(receta.ingrediente1);
        bool tieneIngrediente2 = inventory.currentItems.Contains(receta.ingrediente2);

        if (!tieneIngrediente1 || !tieneIngrediente2)
        {
            // Le decimos al jugador qué le falta
            if (!tieneIngrediente1)
                Debug.Log($"[CraftingSystem] Te falta: {receta.ingrediente1?.itemName ?? "Ingrediente 1"}");
            if (!tieneIngrediente2)
                Debug.Log($"[CraftingSystem] Te falta: {receta.ingrediente2?.itemName ?? "Ingrediente 2"}");
            return false;
        }

        // Si hay espacio en el inventario para el resultado
        if (!inventory.HasSpace())
        {
            Debug.Log("[CraftingSystem] Inventario lleno. No hay espacio para el resultado.");
            return false;
        }

        // Consumimos las dos piezas
        inventory.RemoveItem(receta.ingrediente1);
        inventory.RemoveItem(receta.ingrediente2);

        // Actualizamos la UI de los slots consumidos
        RefrescarSlotUI(receta.ingrediente1);
        RefrescarSlotUI(receta.ingrediente2);

        // Añadimos el resultado al inventario
        inventory.AddItem(receta.resultado);

        Debug.Log($"[CraftingSystem] ¡Crafteo exitoso! Obtuviste: {receta.resultado.itemName}");
        return true;
    }

    // ─── HELPER: Refrescar visualmente un slot ────────────────────────────────
    // Busca el slot en la UI que tenga ese ítem y lo limpia
    private void RefrescarSlotUI(ItemData itemParaLimpiar)
    {
        InventoryPanelUI panel = FindFirstObjectByType<InventoryPanelUI>();
        if (panel == null) return;

        foreach (Transform child in panel.slotsParent)
        {
            InventorySlotUI slot = child.GetComponent<InventorySlotUI>();
            if (slot != null && slot.currentItem == itemParaLimpiar)
            {
                slot.ClearSlot();
                break;
            }
        }
    }

    // ─── HELPER: Verificar si una receta es crafteable con el inventario actual ─
    // La UI usa esto para activar/desactivar el botón de craftear
    public bool PuedeCraftear(int recetaIndex)
    {
        if (recetaIndex < 0 || recetaIndex >= todasLasRecetas.Count) return false;
        Receta receta = todasLasRecetas[recetaIndex];

        bool tieneUno = receta.ingrediente1 != null && inventory.currentItems.Contains(receta.ingrediente1);
        bool tieneDos = receta.ingrediente2 != null && inventory.currentItems.Contains(receta.ingrediente2);

        return tieneUno && tieneDos;
    }
}