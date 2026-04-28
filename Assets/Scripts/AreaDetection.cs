using UnityEngine;

public class AreaDetection : MonoBehaviour
{
    public enum AreaType { Effect, Victory, Death, Checkpoint}
    public AreaType areaType;

    private PlayerStatus playerStatus;

    private GameManager gameManager;

    private void Start() {
      //  gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        playerStatus = GameObject.FindWithTag("Player").GetComponent<PlayerStatus>();
        gameManager = GameObject.FindFirstObjectByType<GameManager>();

    }

    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag("Player"))
        {
            switch (areaType)
            {
                case AreaType.Effect:
                    Debug.Log("Area was of effect type");
                    break;

                case AreaType.Victory:
                    Debug.Log("Area was of victory type");
                    gameManager.OnGameVictory();
                    break;

                case AreaType.Death:
                    Debug.Log("Area was of death type");
                    playerStatus.ModifyHealth(-101);
                    break;

                case AreaType.Checkpoint:
                    Debug.Log("Area was of checkpoint type");
                    // Arreglo: Guardaba como checkpoint el cadaver del jugador y solo coordenadas (no stats)
                    // Ahora pasa gameObject (campamento) en lugar de 'other.gameObject' (jugador).
                    // envia playerStatus para que GameManager (metodo SetNewCheckpoint ) guarde stats.
                    //GameManager.SetNewCheckpoint(other.gameObject);
                    GameManager.SetNewCheckpoint(gameObject, playerStatus);

                     // NUEVA LÍNEA: dispara el sonido cuando el jugador alcanza un checkpoint
                    // Usa el operador ?. para evitar errores si SFXEvents.instance es null
                    SFXEvents.instance?.OnCheckpointReached();

                    break;

                default:
                    Debug.Log("How???????");
                    break;
            }
        }
        
    }

}
