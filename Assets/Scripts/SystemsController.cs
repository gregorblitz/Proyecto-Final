// Fausto A. Gómez
using UnityEngine;
using UnityEngine.InputSystem;

public class SystemsController : MonoBehaviour
{
    [Header("Configuración Linterna")]
    public GameObject flashlightLight;
    public bool hasFlashlight = false;
    private bool isFlashlightOn = false;

    // NUEVO: referencia al controlador de lógica
    private FlashlightController flashlightController;

    [Header("Estado del Jugador")]
    public ItemData selectedItem;

    private void Awake()
    {
        // Buscamos el FlashlightController en el jugador o sus hijos
        flashlightController = GetComponentInChildren<FlashlightController>();
        if (flashlightController == null)
            Debug.LogWarning("No se encontró FlashlightController en el jugador");
    }

    public void OnToggleFlashlight(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Ahora delega al controlador real
            if (flashlightController != null)
                flashlightController.TryToggle();
        }
    }

    public void EquipItem(ItemData item)
    {
        Debug.Log("Entro a EquipItem en SystemController");
        selectedItem = item;

        if (item.type == ItemData.ItemType.Flashlight)
        {
            hasFlashlight = true;
            // Le avisamos al controlador de linterna
            if (flashlightController != null)
                flashlightController.EquipFlashlight(item);
        }

        Debug.Log($"Equipado: {item.itemName}");
    }
}