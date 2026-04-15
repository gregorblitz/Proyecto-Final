using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    [Header("Configuración")]
    public Transform player;         // Arrastrar jugador aquí en el Inspector
    public float detectDistance = 10f; // Distancia deteccion
    public float chaseSpeed = 5f;    // Velocidad con la que corre

    private NavMeshAgent agent;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
    }

    void Update()
    {
        // Calcula la distancia entre el monstruo y el jugador
        float distance = Vector3.Distance(transform.position, player.position);

        // Si el jugador entra en el rango, empieza la persecucion
        if (distance <= detectDistance)
        {
            isChasing = true;
        }
        //Si esta en modo persecucion, le dice al agente que vaya hacia el jugador
        if (isChasing)
        {
            // El monstruo actualiza su destino a la posición del jugador constantemente
            agent.SetDestination(player.position);
        }
    }

    // para ver el círculo de detección en el editor de Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
}
