using UnityEngine;

public class ItemEffectHandler : MonoBehaviour
{
    private PlayerStatus playerStatus;

    private void Awake()
    {
        // Buscamos el componente PlayerStatus en el jugador
        playerStatus = GetComponent<PlayerStatus>();
    }

    public void UseItem(ItemData item)
    {
        if (item == null || playerStatus == null) return;

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
