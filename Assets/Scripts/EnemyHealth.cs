using UnityEngine;
using UnityEngine.UI; // Para usar Sliders de UI
using UnityEngine.AI; // Necesario para apagar el NavMeshAgent
public class EnemyHealth : MonoBehaviour
{
    [Header("Vida del Monstruo")]
    public float maxHealth = 100f;
    private float currentHealth;

    // Evita que el monstruo muera dos veces seguidas
    private bool isDead = false;

    // Referencia a la barra visual
    [Header("Interfaz UI")]
    public Slider healthBar;

    [Header("Recompensas")]
    public float madnessReduction = -20f; // cantidad de locura que quita al matar monstruos

    // Referencias a los componentes del monstruo
    private Animator animator;
    private NavMeshAgent agent;
    private Monster monsterScript;

    private void Start()
    {
        currentHealth = maxHealth;

        // Configura la barra al inicio
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        // Busca los componentes al iniciar
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        monsterScript = GetComponent<Monster>();
    }

    // Se llamara cuando la pica lo golpee
    public void TakeDamage(float damageAmount)
    {

        // Si el monstruo ya está muerto, ignoramos los golpes extra
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log("Monstruo recibio " + damageAmount + " de daño. Vida restante: " + currentHealth);

        // Actualiza la barra visual
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            isDead = true; // Lo marca como muerto
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Monstruo derrotado");

        // Busca al jugador por su Tag para darle la recompensa
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStatus pStatus = player.GetComponent<PlayerStatus>();
            if (pStatus != null)
            {
                // Le pasamos el -20 a la funcion player status
                pStatus.ModifyMadness(madnessReduction);
                Debug.Log("El jugador recupera 20 de cordura");
            }
        }

        // APAGA COMPORTAMIENTOS
        // Detiene el movimiento fisico para que no resbale muerto
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false; // El navemesh se apaga
        }

        // Apaga el script del monstruo para que no siga golpeando al aire
        if (monsterScript != null)
        {
            monsterScript.enabled = false;
        }

        // Apaga el collider principal para que el jugador no choque con un cuerpo muerto
        Collider miCollider = GetComponent<Collider>();
        //if (miCollider != null) miCollider.enabled = false;

        // ACTIVA ANIMACION MUERTE MONSTRUO
        if (animator != null)
        {
            animator.SetTrigger("Die");
            Debug.Log("El script encontrO el Animator y mandO la orden 'Die'");
        }
        else
        {
            Debug.LogError("El script EnemyHealth NO encuentra el Animator del monstruo");
        }

        // DESTRUIR CON RETRASO
        // 5s para que la animacion se vea y el cuerpo quede en el piso un momento
        Destroy(gameObject, 5f);
        
    }
}