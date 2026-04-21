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

    // Se activa cuando la locura llegue a 70
    void SpawnMonsterNearPlayer()
    {
        if (monsterPrefab == null)
        {
            Debug.LogWarning("No se ha asignado monstruo al spawner");
            return;
        }

        // Aparece frente al jugador
        // Elige distancia aparicion entre minSpawn y maxSpawn
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
        // Calcula un punto en linea recta hacia donde mira el jugador
        Vector3 targetPosition = transform.position + (transform.forward * randomDistance);

        // Agrega un ligero desvio aleatorio (izq o der) (transform.right)
        float randomSideOffset = Random.Range(-3f, 3f); // Desvio max 3 metros a los lados
        targetPosition += transform.right * randomSideOffset;

        NavMeshHit hit;
        // Busca punto caminable mas cercano en el NavMesh
        if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
        {
            // Punto valido encontrado -- Instancia al monstruo y guarda en variable
            GameObject clon = Instantiate(monsterPrefab, hit.position, Quaternion.identity);
            
            // Fuerza al clon a ver el jugador
            Monster scriptClon = clon.GetComponent<Monster>();
            if (scriptClon != null)
            {
                scriptClon.detectDistance = 1000f; // Vision infinita para que vea el jugador forzadamente
            }

            Debug.Log("Un monstruo ha aparecido frente a ti");
        }
        else
        {
            // Plan de emergencia
            Vector3 fallbackPos = transform.position + transform.forward * minSpawnDistance;
            GameObject clonFallback = Instantiate(monsterPrefab, fallbackPos, Quaternion.identity);
            
            // Fuerza al clon a ver el jugador (Fallback) ---
            Monster scriptClon = clonFallback.GetComponent<Monster>();
            if (scriptClon != null) scriptClon.detectDistance = 1000f;

            Debug.Log("Un monstruo ha aparecido frente a ti (Fallback)");
        }
    }
}