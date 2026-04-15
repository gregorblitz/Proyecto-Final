// Fausto A. Gómez
using UnityEngine;
using UnityEngine.InputSystem;

public class SystemsController : MonoBehaviour
{
    [Header("Configuración Linterna")]
    public GameObject flashlightLight; 
    public bool hasFlashlight = false;
    private bool isFlashlightOn = false;

    [Header("Estado del Jugador")]
    public ItemData selectedItem; // El ítem que el jugador "activó" en el inventario

    // Acción vinculada a la linterna (Sugerencia: Tecla F)
    public void OnToggleFlashlight(InputAction.CallbackContext context)
    {
        if (context.started && hasFlashlight)
        {
            isFlashlightOn = !isFlashlightOn;
            flashlightLight.SetActive(isFlashlightOn);
        }
    }

    public void EquipItem(ItemData item)
    {
        selectedItem = item;
        if (item.type == ItemData.ItemType.Flashlight) hasFlashlight = true;
        Debug.Log($"Equipado: {item.itemName}");
    }
}