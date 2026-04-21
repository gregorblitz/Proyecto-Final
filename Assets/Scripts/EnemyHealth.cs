using UnityEngine;
using UnityEngine.UI; // Para usar Sliders de UI
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

    private void Start()
    {
        currentHealth = maxHealth;

        // Configura la barra al inicio
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
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
        
        // Poner animacion de muerte
        // Destruir por el momento
        Destroy(gameObject);
    }
}