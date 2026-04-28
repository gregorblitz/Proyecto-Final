using UnityEngine;
using UnityEngine.AI;
using System.Collections; // NUEVO: Necesario para usar Corrutinas (tiempos de espera)

public class Monster : MonoBehaviour
{
    [Header("Configuración")]
    private Transform player;         // Se asigna mediante codigo
    public float detectDistance = 10f; // Distancia deteccion
    public float chaseSpeed = 5f;    // Velocidad con la que corre

    [Header("Tiempos de Animación")]
    public float roarDuration = 4f; // Segundos que se queda quieto rugiendo

    [Header("Configuración de Ataque")]
    public float attackDamage = 15f; // Cantidad de vida que quita por golpe
    public float attackCooldown = 2f; // Segundos que tarda en volver a golpear
    public float attackReach = 2f; // El largo fisico real del brazo 
    private float lastAttackTime = 0f; // Controla cuándo fue el último golpe

    private NavMeshAgent agent;
    private Animator animator; // Creacion de la variable para el cerebro de animaciones
    // Logica de estados
    private bool hasDetectedPlayer = false;
    public bool isRunning = false;

    void Start()
    {
        // Busca al jugador por su Tag
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                // Indicador para saber a quien esta persiguiendo el clon
                Debug.Log("El clon esta persiguiendo a un objeto llamado: " + playerObj.name);
            }
            else
            {
                Debug.LogError("El monstruo no encuentra al jugador");
            }
        }
        agent = GetComponent<NavMeshAgent>();
        //Busca el componente animator en los objetos hijos del monstruo
        animator = GetComponentInChildren<Animator>();
        agent.speed = chaseSpeed;
    }

    void Update()
    {
        // Calcula la distancia entre el monstruo y el jugador
        float distance = Vector3.Distance(transform.position, player.position);

        // Si el jugador entra en el rango y no lo ha detectado
        if (distance <= detectDistance && hasDetectedPlayer == false)
        {
            hasDetectedPlayer = true; // Activa deteccion del jugador
            // Inicia la secuencia de Rugir y luego Correr
            StartCoroutine(RoarThenChase());
        }

        // Fase de Rugido, gira a mirar jugador pero no avanza
        if (hasDetectedPlayer == true && isRunning == false)
        {
            FacePlayer();
        }
        // Persecucion ocurre despues del rugido
        if (isRunning == true)
        {
            // Persigue al jugador
            agent.SetDestination(player.position);

            // Ataque
            // Si el monstruo llega al stopping distance ataca
            // Le suma 0.2f como margen de error para que no falle al calcular.
            if (distance <= agent.stoppingDistance + 0.2f)
            {
                // cuando ya pasa el tiempo de recarga desde el ultimo ataque
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    animator.SetTrigger("Attack"); // Dispara el golpe

                    lastAttackTime = Time.time; // Reinicia cronometro de ataque
                }
            }
        }
    }

    // Corrutina que controla el tiempo para rugir y luego correr
    IEnumerator RoarThenChase()
    {
        // Frena fisicamente al monstruo para que no se deslice ni se mueva
        agent.enabled = false; //apaga cerebro por completo
        // Dispara la animacion -- pasa de sniff a roar
        animator.SetBool("isChasing", true);

         // Espera exactamente los segundos que dura el rugido
        yield return new WaitForSeconds(roarDuration);
        
        // PRENDE EL CEREBRO DE NUEVO PARA QUE CORRA
        agent.enabled = true;
        agent.speed = chaseSpeed; // Regresa su velocidad original    
        agent.isStopped = false;
        isRunning = true;         // Activa la parte del Update que mueve al NavMesh
        
    }

    // Obliga al monstruo a mirar al jugador
    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Ignora el eje Y para no inclinarse al suelo

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 8f);
        }
    }

    // para ver el círculo de detección en el editor de Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
    
    // Llama la animacion en el momento exacto del golpe
    public void ApplyDamageToPlayer()
    {
        // Mide la distancia exacta en el milisegundo del golpe
        float currentDistance = Vector3.Distance(transform.position, player.position);

        // Usa el largo del brazo --> 2
        if (currentDistance <= attackReach)
        {
            PlayerStatus pStatus = player.GetComponent<PlayerStatus>();
            if (pStatus != null)
            {
                pStatus.ModifyHealth(-attackDamage);
                Debug.Log("El monstruo golpeo al jugador");
            }
        }
        else
        {
            // Si el jugador se aleja, el golpe falla
            Debug.Log("Golpe esquivado. Distancia al golpear: " + currentDistance + " (Brazo: " + attackReach + ")");
        }
    }
}
