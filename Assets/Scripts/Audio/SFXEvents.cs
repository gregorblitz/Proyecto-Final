// Fausto A. Gómez
// Centraliza los disparadores de sonido para objetos interactuables:
// pickup de ítems, checkpoint, taladro y pico.
// NO modifica scripts de otros compañeros. Se conecta llamando sus métodos
// desde los scripts existentes (BreakableWall, ItemCollectible, AreaDetection).
//
// CÓMO USARLO:
//   • Pickup:     SFXEvents.instance.OnItemPickup();
//   • Checkpoint: SFXEvents.instance.OnCheckpointReached();
//   • Taladro:    SFXEvents.instance.OnDrillUsed(transform.position);
//   • Pico:       SFXEvents.instance.OnPickaxeUsed(transform.position);
using UnityEngine;

public class SFXEvents : MonoBehaviour
{
    public static SFXEvents instance;

    private void Awake()
    {
        // Singleton ligero (no necesita persistir entre escenas como AudioManager)
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // -------------------------------------------------------
    // Métodos públicos — llamar desde los scripts del proyecto
    // -------------------------------------------------------

    // Llamar desde ItemCollectible.cs cuando se recoge un ítem
    public void OnItemPickup()
    {
        if (AudioManager.instance == null) return;
        AudioManager.instance.PlayPickup();
        Debug.Log("[SFXEvents] Sonido de pickup");
    }

    // Llamar desde AreaDetection.cs en el case Checkpoint
    public void OnCheckpointReached()
    {
        if (AudioManager.instance == null) return;
        AudioManager.instance.PlayCheckpoint();
        Debug.Log("[SFXEvents] Sonido de checkpoint");
    }

    // Llamar desde BreakableWall.cs al romper la pared con el taladro
    public void OnDrillUsed(Vector3 position)
    {
        if (AudioManager.instance == null) return;
        AudioManager.instance.PlayDrill(position);
        Debug.Log("[SFXEvents] Sonido de taladro en " + position);
    }

    // Llamar desde cualquier script que use el pico
    public void OnPickaxeUsed(Vector3 position)
    {
        if (AudioManager.instance == null) return;
        AudioManager.instance.PlayPickaxe(position);
        Debug.Log("[SFXEvents] Sonido de pico en " + position);
    }

    // Llamar cuando el enemigo golpea al jugador (desde MonsterAnimation.cs si se quiere)
    public void OnEnemyHit(Vector3 position)
    {
        if (AudioManager.instance == null) return;
        AudioManager.instance.PlayEnemyHit(position);
        Debug.Log("[SFXEvents] Sonido de golpe del enemigo");
    }
}