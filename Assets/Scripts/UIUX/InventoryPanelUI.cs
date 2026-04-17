// Fausto A. Gomez
using UnityEngine;
using UnityEngine.InputSystem; // OBLIGATORIO para Nuevo Input System

public class InventoryPanelUI : MonoBehaviour
{
    [Header("Componentes del Panel")]
    public GameObject inventoryPanel; // El panel principal que se oculta/muestra
    public Transform slotsParent;    // El objeto padre que contiene los slots (ej: un Grid Layout Group)

    [Header("Prefabs")]
    public GameObject slotPrefab;

    [Header("Estado")]
    public bool isInventoryOpen = false;

    [Header("Selección")]
    public int selectedSlotIndex = 0;
    private InputAction scrollAction;

    private void Awake()
    {
        // Buscamos la acción de Scroll del ratón
        scrollAction = InputSystem.actions.FindAction("ScrollWheel");
    }

    private void Start()
    {
        InitializeInventoryUI();
    }

    private void Update()
    {
        if (isInventoryOpen)
        {
            HandleScrollInput();
        }
    }

    // Inicializa la UI creando slots vacíos según el tamaño del Manager
    private void InitializeInventoryUI()
    {
        // Limpiar slots viejos primero (AAA: seguridad)
        if (slotsParent == null) return;

        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }

        // Crear nuevos slots vacíos basados en la capacidad del Manager
        if (InventoryManager.Instance != null)
        {
            for (int i = 0; i < InventoryManager.Instance.inventorySize; i++)
            {
                Instantiate(slotPrefab, slotsParent);
            }
        }

        // Ocultar el inventario al inicio
        isInventoryOpen = false;
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    // --- MÉTODOS PARA EL NUEVO INPUT SYSTEM ---

    /// <summary>
    /// Esta es la función que aparecerá en el PlayerInput bajo "Dynamic CallbackContext"
    /// </summary>
    public void OnToggleInventory(InputAction.CallbackContext context)
    {
        // 'started' indica que la tecla fue presionada (evita que se active al soltarla)
        if (context.started)
        {
            ToggleInventory();
        }
    }

    // También mantenemos esta por compatibilidad o uso manual por código
    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        // AAA Touch: Manejar el cursor y el estado del juego
        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // TIP: Podrías añadir Time.timeScale = 0; si quieres pausar el juego
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // TIP: Podrías añadir Time.timeScale = 1; para reanudar
        }
    }

    public GameObject GetAvailableSlot()
    {
        foreach (Transform child in slotsParent)
        {
            InventorySlotUI slot = child.GetComponent<InventorySlotUI>();
            // Si el slot existe y está vacío (sin ítem asignado)
            if (slot != null && slot.currentItem == null)
            {
                return child.gameObject;
            }
        }
        return null; // No hay slots vacíos
    }

    private void HandleScrollInput()
    {
        float scrollValue = scrollAction.ReadValue<Vector2>().y;

        if (scrollValue != 0)
        {
            // Cambiamos el índice (hacia arriba o hacia abajo)
            selectedSlotIndex = scrollValue > 0 ? selectedSlotIndex - 1 : selectedSlotIndex + 1;
            //if (scrollValue > 0) selectedSlotIndex--;
            //else selectedSlotIndex++;

            // Aseguramos que el índice se mantenga dentro del rango de slots
            int totalSlots = slotsParent.childCount;
            if (selectedSlotIndex < 0) selectedSlotIndex = totalSlots - 1;
            if (selectedSlotIndex >= totalSlots) selectedSlotIndex = 0;

            UpdateSlotSelection();
        }
    }

    public void UpdateSlotSelection()
    {
        for (int i = 0; i < slotsParent.childCount; i++)
        {
            InventorySlotUI slot = slotsParent.GetChild(i).GetComponent<InventorySlotUI>();
            if (slot != null)
            {
                // Activamos o desactivamos el resaltado visual
                
                slot.SetSelected(i == selectedSlotIndex);
            }
        }
    }
}