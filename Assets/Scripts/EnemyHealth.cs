using UnityEngine;
using UnityEngine.UI; // Para usar Sliders de UI
public class EnemyHealth : MonoBehaviour
{
    [Header("Vida del Monstruo")]
    public float maxHealth = 100f;
    private float currentHealth;

    // Referencia a la barra visual
    [Header("Interfaz UI")]
    public Slider healthBar;

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
        currentHealth -= damageAmount;
        Debug.Log("Monstruo recibio " + damageAmount + " de daño. Vida restante: " + currentHealth);

        // Actualiza la barra visual
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Monstruo derrotado");
        
        // Poner animacion de muerte
        // Destruir por el momento
        Destroy(gameObject);
    }
}