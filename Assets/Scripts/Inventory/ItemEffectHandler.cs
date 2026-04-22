using UnityEngine;
using UnityEngine.InputSystem; // Requerido para el nuevo Input System

public class ItemEffectHandler : MonoBehaviour
{
    private PlayerStatus playerStatus;
    private InventoryPanelUI inventoryPanel;
    

    // SISTEMA DE ARMAS
    [Header("Sistema de Armas")]
    public GameObject pickaxeInHand; // va la pica apagada en la mano
    private Animator animator;
    private void Awake()
    {
        // Buscamos el componente PlayerStatus en el jugador
        playerStatus = GetComponent<PlayerStatus>();
        animator = GetComponentInChildren<Animator>(); // Busca el Animator para cambiar de postura

        
    }
    private void Start() {
        inventoryPanel = GameObject.FindFirstObjectByType<InventoryPanelUI>();
    }

    public void UseItem(ItemData item)
    {
        Debug.Log("playerStatus = " + playerStatus);
        Debug.Log("item = " + item);

        if (item == null || playerStatus == null) 
        {
            Debug.Log("El slot estaba vacio o playerStatus no existe");
            return;
        }
        // Lógica para Consumibles (Salud, Oxígeno, Locura)
        if (item.isConsumable)
        {
            switch (item.effectType)
            {
                case ItemData.ItemEffect.Heal:
                    playerStatus.ModifyHealth(item.effectAmount);
                    break;
                case ItemData.ItemEffect.RestoreOxygen:
                    playerStatus.ModifyOxygen(item.effectAmount);
                    break;
                case ItemData.ItemEffect.ReduceMadness:
                    playerStatus.ModifyMadness(-item.effectAmount);
                    break;
            }
        }
        else
        {
            // Lógica para Herramientas (Pico, Linterna, etc.)
            Debug.Log($"Activando herramienta: {item.itemName}");
            // Aquí llamarías al sistema de equipo que están desarrollando tus compañeros

            // PICO
            // Verifica si el item es Pica o Pico segun este en ItemData
            if (item.itemName == "Pica" || item.itemName == "Pico")
            {
                EquipPickaxe();
            }

            // LINTERNA
            // Revisa si tipo objeto es linterna
            else if (item.type == ItemData.ItemType.Flashlight)
            {
                // Busca PlayerInteractor para que la encienda en la mano
                PlayerInteractor interactor = GetComponent<PlayerInteractor>();
                if (interactor != null)
                {
                    interactor.EquipFlashlight();
                }
            }
        }
    }

    // FUNCION PARA EQUIPAR
    // Recoge la pica del suelo -> Va al inventario -> clic izq -> llama a UseThisItem() -> 
    // llama a ItemEffectHandler -> SetActive(true) activa la pica invisible de la mano
    private void EquipPickaxe()
    {
        if (pickaxeInHand != null)
        {
            pickaxeInHand.SetActive(true); // Enciende la pica visualmente
            
            if (animator != null)
            {
                animator.SetBool("isArmed", true); // Opcional: Le avisa al Animator que tienes arma
            }
            
            Debug.Log("Pica en la mano");
        }
        else
        {
            Debug.LogWarning("No hay GameObject pica en el script ItemEffectHandler");
        }
    }
}
