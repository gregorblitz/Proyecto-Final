// Fausto A. Gómez
using UnityEngine;

// Lógica del Taladro como herramienta equipable.
// Este script vive en el jugador (mismo objeto que PlayerInteractor).
// InventorySlotUI llama a EquipDrill() cuando el jugador selecciona el taladro.
// El uso real ocurre cuando el jugador presiona E cerca de una BreakableWall:
// PlayerInteractor ya pasa el selectedItem a Interact(), así que no hace falta tecla extra.
public class DrillController : MonoBehaviour
{
    [Header("Estado")]
    public bool isDrillEquipped = false;

    private SystemsController systems;

    private void Start()
    {
        systems = GetComponent<SystemsController>();
        if (systems == null)
            Debug.LogWarning("[DrillController] No se encontró SystemsController en el jugador.");
    }

    // Llamado desde InventorySlotUI.UseThisItem() cuando el jugador equipa el taladro
    public void EquipDrill()
    {
        isDrillEquipped = true;
        Debug.Log("[Taladro] Equipado. Acércate a una pared rompible y presiona E.");
    }

    // Llamado si el jugador suelta o cambia de herramienta (por ejemplo al hacer Drop)
    public void UnequipDrill()
    {
        isDrillEquipped = false;
        Debug.Log("[Taladro] Desequipado.");
    }
}