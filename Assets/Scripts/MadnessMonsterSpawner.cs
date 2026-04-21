using UnityEngine;
using UnityEngine.AI; // Necesario para NavMesh

public class MadnessMonsterSpawner : MonoBehaviour
{
    [Header("Config Monstruo")]
    public GameObject monsterPrefab; // Arrastrar prefab del monstruo
    
    [Header("Config de aparicion")]
    public float minSpawnDistance = 4f;  // Distancia min aparicion
    public float maxSpawnDistance = 7f; // Distancia máxima aparicion

    private PlayerStatus playerStatus;

    void Start()
    {
        // Busca el script PlayerStatus en objeto player 
        playerStatus = GetComponent<PlayerStatus>();

        if (playerStatus != null)
        {
            // conecta este script con el playerstatus al evento de la locura
            //cuando se activa el evento ejecuta la funcion SpawnMonsterNearPlayer
            playerStatus.OnMadnessSpawnTreesholdReached.AddListener(SpawnMonsterNearPlayer);
        }
        else
        {
            Debug.LogError("No se encontró el PlayerStatus. Pon este script en el Player.");
        }
    }

    // Esta función se activará solita cuando la locura llegue a 70
    void SpawnMonsterNearPlayer()
    {
        if (monsterPrefab == null)
        {
            Debug.LogWarning("¡Falta asignar el prefab del monstruo en el Spawner!");
            return;
        }

        // 1. Calculamos una dirección aleatoria alrededor del jugador
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
        // 2. Convertimos ese círculo 2D a una posición 3D en el mundo
        Vector3 randomPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y) * randomDistance;

        NavMeshHit hit;
        // 3. Buscamos el punto caminable más cercano en el NavMesh (para evitar paredes)
        // Busca en un radio de 5 metros alrededor del punto aleatorio
        if (NavMesh.SamplePosition(randomPosition, out hit, 5f, NavMesh.AllAreas))
        {
            // ¡Punto válido encontrado! Instanciamos al monstruo.
            Instantiate(monsterPrefab, hit.position, Quaternion.identity);
            Debug.Log("¡Locura crítica! Un monstruo ha aparecido cerca de ti.");
        }
        else
        {
            // Plan de emergencia: Si por alguna razón no encuentra suelo, lo pone un poco lejos enfrente
            Vector3 fallbackPos = transform.position + transform.forward * minSpawnDistance;
            Instantiate(monsterPrefab, fallbackPos, Quaternion.identity);
            Debug.Log("¡Locura crítica! Monstruo apareció enfrente (Fallback).");
        }
    }
}