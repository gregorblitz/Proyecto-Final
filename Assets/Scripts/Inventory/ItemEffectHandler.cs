using UnityEngine;
using UnityEngine.InputSystem; // Requerido para el nuevo Input System

public class ItemEffectHandler : MonoBehaviour
{
    private PlayerStatus playerStatus;


    public void UseItem(ItemData item)
    {
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
        }
    }

}
