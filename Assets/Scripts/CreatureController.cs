using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class CreatureController : MonoBehaviour
{

#region VARIABLES
    public enum CreatureState { Idle, Patrolling, Alert, Stalking, Hunting, Attacking, Fleeing, Hidden }
    
    [Header("Configuración de Acecho")]
    public float stalkingRange = 7f; // Distancia a la que se detiene a observar
     public float sanityLossValue = 7f;
    

    [Header("Configuración de Estado")]
    public CreatureState currentState = CreatureState.Idle;
    CreatureState lastState = CreatureState.Hidden;
    
    
    [Header("Tiempos y Rangos /")]
    public float alertDuration = 2.5f;
    public float attackRange = 2.0f;
    public float certainDetectionRange = 5f;
    public float fieldOfViewDetectionRange = 15f;
    public float fieldOfViewAngle = 90f;
    

    [Header("Detección")]

    public LayerMask playerLayer;
    public LayerMask obstructionLayer;

    [Header("Movimiento")]
    public Transform[] patrolWaypoints;
    public float patrolSpeed = 2.5f;
    public float huntSpeed = 5.5f;
    public Transform hideLocation;

    [Header("Sonido")]
    public AudioClip[] whisperSounds;
    public float minWhisperInterval = 5f;
    public float maxWhisperInterval = 15f;

    // ── NUEVO: clips por estado ──────────────────────────────────────────
    [Header("Sonido — Estados de la Criatura")]
    public AudioClip alertSound;       // Al detectar al jugador (Alert)
    public AudioClip stalkingSound;    // Loop suave durante el acecho (Stalking)
    public AudioClip huntingStepClip;  // Pasos mientras persigue (Hunting)
    public AudioClip attackSound;      // Jumpscare / ataque

    [Header("Sonido — Pasos en Hunting")]
    public float stepInterval = 0.5f;  // Segundos entre cada paso de la criatura
    public float stepVolume   = 0.85f;

    [Header("Sonido — Acecho")]
    [Tooltip("Volumen del loop de acecho")]
    public float stalkingVolume = 0.6f;
    // ────────────────────────────────────────────────────────────────────

    [Header("Jumpsacre")]
    public GameObject jumpscareUI;

#region REFERENCIAS PRIVADAS
    private NavMeshAgent agent;
    private AudioSource audioSource;

    // ───── NUEVO: segunda fuente para loops (acecho) sin interrumpir whispers
    private AudioSource loopSource;
    // ────────────────────────────────────────────────────────────────────
    
    private Transform player;
    private PlayerStatus playerStatus;
    private float stateTimer;

    private bool isTakingSanity;
    public  float distanceToPlayer {get; private set;} 
#endregion

    // ───── NUEVO: control interno de pasos ──────────────────────────────
    private float stepTimer = 0f;
    // ────────────────────────────────────────────────────────────────────

#region EVENTOS
    public UnityEvent OnAlert;
    public UnityEvent OnHunting;
    public UnityEvent OnFleeing; 
    public UnityEvent OnAttacking; 
    public UnityEvent OnStalking; 
    public UnityEvent OnIdleOrPatrolling;
    public UnityEvent OnHidding;
#endregion

#endregion

#region MONOBEHAVIOUR

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();


        // ───── NUEVO: creamos una segunda AudioSource para el loop de acecho
        // así no interrumpe los susurros ni otros sonidos puntuales
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop         = true;
        loopSource.spatialBlend = 1f;
        loopSource.playOnAwake  = false;
        loopSource.volume       = 0f;
        // ────────────────────────────────────────────────────────────────────


        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerStatus = playerObj.GetComponent<PlayerStatus>();
            playerStatus.OnPlayerDeath.AddListener(() => ChangeState(CreatureState.Fleeing));
        }
    }

    void Start()
    {
        StartCoroutine(WhisperRoutine());
    }


    void Update()
    {
        if (currentState == CreatureState.Hidden) return;

        // ACTUALIZACIÓN CRÍTICA: Calculamos la distancia siempre que el jugador exista
        if (player != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
        }

        switch (currentState)
        {
            case CreatureState.Idle: HandleIdle(); break;
            case CreatureState.Patrolling: HandlePatrolling(); break;
            case CreatureState.Alert: HandleAlert(); break;
            case CreatureState.Stalking: HandleStalking(); break;
            case CreatureState.Hunting: HandleHunting(); break;
            case CreatureState.Attacking: HandleAttacking(); break;
            case CreatureState.Fleeing: HandleFleeing(); break;
        }

        // El resto de la detección base permanece igual
        if (currentState == CreatureState.Idle || currentState == CreatureState.Patrolling)
        {
            if (CanSeePlayer()) ChangeState(CreatureState.Alert);
        }

        lastState = currentState;
    }
#endregion


#region LOGICA DE ESTADOS
    // --- LÓGICA DE ESTADOS ---
    private void HandleAlert()
    {
        agent.isStopped = true;
        // La criatura mira lentamente al jugador
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        stateTimer += Time.deltaTime;
        if (distanceToPlayer <= stalkingRange)
        {
            ChangeState(CreatureState.Stalking);
            isTakingSanity= true;
            StartCoroutine(TakeSanity());
        }

        else if (stateTimer >= alertDuration)
        {
            ChangeState(CreatureState.Hunting);
        }

        // Si el jugador escapa de la vista durante la alerta, vuelve a patrullar
        if (!CanSeePlayer() && stateTimer < alertDuration * 0.5f) 
        {
            ChangeState(CreatureState.Patrolling);
        }
        
        //if(CheckIfIsNewEvent()) OnAlert;
        //CheckIfIsNewEvent(OnAlert);
        //OnAlert?.Invoke();
    }

    private void HandleIdle()
    {
        agent.isStopped = true;
        stateTimer += Time.deltaTime;
        if (stateTimer >= 3f) ChangeState(CreatureState.Patrolling);
    }

    private void HandlePatrolling()
    {
        agent.isStopped = false;
        agent.stoppingDistance = 0f;
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetRandomPatrolDestination();
            ChangeState(CreatureState.Idle);
        }
    }

    private void HandleHunting()
    {
        agent.isStopped = false;
        agent.speed = huntSpeed; 
        agent.SetDestination(player.position);


         // ───── NUEVO: pasos de la criatura mientras persigue ────────────
        stepTimer += Time.deltaTime;
        if (stepTimer >= stepInterval)
        {
            stepTimer = 0f;
            if (huntingStepClip != null)
                audioSource.PlayOneShot(huntingStepClip, stepVolume);
        }
        // ────────────────────────────────────────────────────────────────────

        // Si entra en rango de acecho, se detiene[cite: 12]
        
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(CreatureState.Attacking);
        }

        if (distanceToPlayer <= stalkingRange)
        {
            ChangeState(CreatureState.Stalking);
            isTakingSanity= true;
            StartCoroutine(TakeSanity());
        }
    }

    private void HandleAttacking()
    {
        agent.isStopped = true;
        
        // Ejecutar Jumpscare
        if (jumpscareUI != null) jumpscareUI.SetActive(true);
        
        // Aplicar daño al PlayerStatus (según script de Danna)
        playerStatus.ModifyHealth(-100f); 

        // Esperar un momento y huir
        Invoke("FinishAttack", 1.5f);

        //CheckIfIsNewEvent(OnAttacking);
        //OnAttacking?.Invoke();
    }

    private void FinishAttack()
    {
        if (jumpscareUI != null) jumpscareUI.SetActive(false);
        ChangeState(CreatureState.Fleeing);
    }

    private void HandleFleeing()
    {
        agent.isStopped = false;
        agent.SetDestination(hideLocation.position);
        if (agent.remainingDistance < 1f) ChangeState(CreatureState.Hidden);

        //CheckIfIsNewEvent(OnFleeing);
        //OnFleeing?.Invoke();
    }

    private void HandleStalking()
    {
        // La criatura NO se mueve bajo ninguna circunstancia en este estado
        agent.isStopped = true;
        
        // Rotación suave para seguir al jugador con la "mirada"
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);

        // CRÍTICO: Si el jugador se acerca demasiado, es el fin.
        if (distanceToPlayer <= attackRange)
        {
            CancelInvoke("ApplyEffect");
            ChangeState(CreatureState.Attacking);
            isTakingSanity= false;
            return;
        }

        // LÓGICA DE ESCAPE:
        // Si el jugador rompe la visión O se aleja más allá del rango de acecho
        if (!CanSeePlayer() || distanceToPlayer > stalkingRange)
        {
            stateTimer += Time.deltaTime;

            // Reutilizamos alertDuration como tiempo de gracia para huir
            if (stateTimer >= alertDuration)
            {
                isTakingSanity= false;
                Debug.Log("Jugador escapó del acecho. Regresando a Idle.");
                ChangeState(CreatureState.Idle);
            }
        }
        else
        {
            // Si el jugador se queda quieto dentro del rango y es visible, 
            // el timer se reinicia. La criatura no se va si te está viendo.
            stateTimer = 0;
        }
    }
#endregion

#region UTILIDADES

    IEnumerator TakeSanity() {
        while (isTakingSanity) {
            playerStatus.ModifyMadness(sanityLossValue);
            yield return new WaitForSeconds(1f);
        }
}

    public void ChangeState(CreatureState newState)
    {
        currentState = newState;
        stateTimer = 0;


        // ── NUEVO: lógica de audio por cambio de estado ──────────────────
        OnCreatureStateChanged(newState);
        // ────────────────────────────────────────────────────────────────


        if(newState == CreatureState.Hidden)
        {
            // Desactivar visuales para "desespawnear" 
            GetComponentInChildren<Renderer>().enabled = false;
            agent.enabled = false;
        }
        

        switch (currentState)
        {
            case CreatureState.Alert: OnAlert?.Invoke(); break;
            case CreatureState.Hunting: OnHunting?.Invoke(); break;
            case CreatureState.Stalking: OnStalking?.Invoke(); break; // Notificar acecho
            case CreatureState.Attacking: OnAttacking?.Invoke(); break;
            case CreatureState.Fleeing: OnFleeing?.Invoke(); break;
            case CreatureState.Idle: OnIdleOrPatrolling?.Invoke(); break;
            case CreatureState.Patrolling: OnIdleOrPatrolling?.Invoke(); break;
        }
    }


     // ── NUEVO: método separado para no mezclar audio con la lógica de estados ──────────────────
    private void OnCreatureStateChanged(CreatureState newState)
    {
        // Detener el loop de acecho por defecto; se reactiva solo en Stalking
        StopStalkingLoop();

        switch (newState)
        {
            case CreatureState.Alert:
                // Sonido puntual de detección
                if (alertSound != null)
                    audioSource.PlayOneShot(alertSound);

                // Avisar al AudioManager para activar música de persecución
                if (AudioManager.instance != null)
                    AudioManager.instance.SetChaseMode(true);

                stepTimer = 0f;
                break;

            case CreatureState.Hunting:
                // La música de chase ya está activa desde Alert
                // Solo reiniciamos el timer de pasos
                stepTimer = 0f;
                break;

            case CreatureState.Stalking:
                // Loop de acecho suave
                StartStalkingLoop();
                break;

            case CreatureState.Attacking:
                // Sonido de ataque/jumpscare puntual
                if (attackSound != null)
                    audioSource.PlayOneShot(attackSound);
                break;

            case CreatureState.Fleeing:
            case CreatureState.Hidden:
            case CreatureState.Idle:
            case CreatureState.Patrolling:
                // Al escapar o esconderse, volvemos a música de ambiente
                if (AudioManager.instance != null)
                    AudioManager.instance.SetChaseMode(false);
                break;
        }
    }

    // ─── NUEVO: inicia el loop de acecho con fade-in suave ──────────────────
    private void StartStalkingLoop()
    {
        if (stalkingSound == null) return;
        loopSource.clip   = stalkingSound;
        loopSource.volume = 0f;
        loopSource.Play();
        StartCoroutine(FadeLoopVolume(0f, stalkingVolume, 1.5f));
    }

    // ───── NUEVO: detiene el loop con fade-out suave ────────────────────────────────
    private void StopStalkingLoop()
    {
        if (loopSource != null && loopSource.isPlaying)
            StartCoroutine(FadeLoopVolume(loopSource.volume, 0f, 0.8f));
    }

    // ───── NUEVO: fade genérico del loopSource ────────────────────────────────────
    private IEnumerator FadeLoopVolume(float from, float to, float duration)
    {
        float time = 0f;
        loopSource.volume = from;

        while (time < duration)
        {
            time += Time.deltaTime;
            loopSource.volume = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }

        loopSource.volume = to;
        if (to <= 0f) loopSource.Stop();
    }
    // ──────────────────────────────────────────────────────────────────────────────────────────


    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        if (distanceToPlayer < fieldOfViewDetectionRange)
        {
            if(distanceToPlayer < certainDetectionRange) return true;

            if (Vector3.Angle(transform.forward, directionToPlayer) < fieldOfViewAngle / 2)
            {
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstructionLayer))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void SetRandomPatrolDestination()
    {
        if (patrolWaypoints.Length == 0) return;
        agent.SetDestination(patrolWaypoints[Random.Range(0, patrolWaypoints.Length)].position);
    }

    // --- CORRUTINA DE SONIDOS ---

    IEnumerator WhisperRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minWhisperInterval, maxWhisperInterval));
            
            if (currentState != CreatureState.Hidden && whisperSounds.Length > 0)
            {
                AudioClip clip = whisperSounds[Random.Range(0, whisperSounds.Length)];
                audioSource.PlayOneShot(clip);
            }
        }
    }

    // Visualización en el editor (Senior Tip: ¡Indispensable para debug!)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fieldOfViewDetectionRange);
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * fieldOfViewDetectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * fieldOfViewDetectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, certainDetectionRange);
    }
#endregion
}
