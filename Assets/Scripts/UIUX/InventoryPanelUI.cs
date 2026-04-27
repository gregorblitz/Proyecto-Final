// Fausto A. Gomez
// MODIFICADO: 
//   - Se eliminó el scroll por teclado cuando el inventario está abierto.
//   - Se añadió bloqueo del Input de cámara cuando el inventario está abierto
//     para que el mouse mueva el inventario y NO la cámara.
//   - La selección de slots solo se hace con clic/scroll del mouse (sin teclas).

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

    // Referencia al PlayerInput para desactivar el mapa de acción de movimiento
    // cuando el inventario está abierto (así el mouse no mueve la cámara)
    [Header("Control de Input")]
    [Tooltip("Arrastra aquí el componente PlayerInput del jugador")]
    public PlayerInput playerInput;

    // Nombre del Action Map del jugador (el que controla movimiento y cámara)
    // Ajusta este nombre según tu proyecto (por defecto suele ser "Player")
    [Tooltip("Nombre del Action Map de movimiento/cámara del jugador")]
    public string playerActionMapName = "Player";

    // Nombre del Action Map de UI (para que el scroll del mouse funcione en el inventario)
    [Tooltip("Nombre del Action Map de UI")]
    public string uiActionMapName = "UI";

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
        // Solo procesamos el scroll cuando el inventario está abierto
        if (isInventoryOpen)
        {
            HandleScrollInput();

            // Escuchar el clic de la rueda globalmente
            if (Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame)
            {
                EjecutarAccionSlotSeleccionado();
            }

            // Clic Derecho para Tirar el objeto seleccionado 
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                TirarObjetoSeleccionado();
            }
        
        }
    }

    // PARA TIRAR o desequipar
    private void TirarObjetoSeleccionado()
    {
        // Buscas el slot donde esta el cuadro amarillo actualmente
        if (slotsParent.childCount > 0)
        {
            InventorySlotUI slotActual = slotsParent.GetChild(selectedSlotIndex).GetComponent<InventorySlotUI>();

            // Si el slot existe y tiene un objeto adentro lo tira
            if (slotActual != null && slotActual.currentItem != null)
            {
                slotActual.DropItem();
                
                // Re-evalua los brillos de crafteo por si se tira un ingrediente
                if (CraftingManager.Instance != null)
                {
                    InventorySlotUI[] allSlots = slotsParent.GetComponentsInChildren<InventorySlotUI>(true);
                    CraftingManager.Instance.EvaluateCraftableItemsGlow(allSlots);
                }
            }
        }
    }

    private void EjecutarAccionSlotSeleccionado()
    {
        // Busca el slot donde esta parado actualmente (resaltado amarillo)
        if (slotsParent.childCount > 0)
        {
            InventorySlotUI slotActual = slotsParent.GetChild(selectedSlotIndex).GetComponent<InventorySlotUI>();

            if (slotActual != null && CraftingManager.Instance != null)
            {
                // Manda este slot al cerebro de crafteo sin importar donde mire la camara
                CraftingManager.Instance.HandleMiddleClick(slotActual);
            }
        }
    }

    // Inicializa la UI creando slots vacíos según el tamaño del Manager
    private void InitializeInventoryUI()
    {
        // Limpiar slots viejos primero (seguridad)
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

        foreach (InventorySlotUI child in slotsParent.GetComponentsInChildren<InventorySlotUI>())
        {
            Debug.Log("Reinició slot " + child);
            child.icon.enabled = false;
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

        if (isInventoryOpen)
        {
            // Mostramos el cursor para que el jugador pueda hacer clic en los slots
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
            //*****INICIO - MODIFICACIONES PARA CRAFTEO******
            // Evaluar brillos de crafteo al abrir
            InventorySlotUI[] allSlots = slotsParent.GetComponentsInChildren<InventorySlotUI>(true);
            if (CraftingManager.Instance != null)
            {
                CraftingManager.Instance.EvaluateCraftableItemsGlow(allSlots);
            }
        }
        else
        {
            // Volvemos a bloquear el cursor al cerrar el inventario
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
    }

    // Activa o desactiva el Action Map del jugador para que el mouse
    // no afecte la cámara cuando el inventario está abierto
    private void BloquearInputJugador(bool bloquear)
    {
        if (playerInput == null)
        {
            // Intentamos encontrarlo si no fue asignado en el Inspector
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                playerInput = player.GetComponent<PlayerInput>();
        }

        if (playerInput == null) return;

        if (bloquear)
        {
            // Cambiamos al mapa de UI para que el mouse solo interactúe con la UI
            playerInput.SwitchCurrentActionMap(uiActionMapName);
        }
        else
        {
            // Volvemos al mapa del jugador
            playerInput.SwitchCurrentActionMap(playerActionMapName);
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

    // Maneja el scroll del mouse para cambiar el slot seleccionado
    // SOLO funciona cuando el inventario está abierto y se usa el scroll (ruedita del mouse)
    private void HandleScrollInput()
    {
        float scrollValue = scrollAction.ReadValue<Vector2>().y;

        if (scrollValue != 0)
        {
            // Cambiamos el índice (hacia arriba o hacia abajo)
            selectedSlotIndex = scrollValue > 0 ? selectedSlotIndex - 1 : selectedSlotIndex + 1;

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