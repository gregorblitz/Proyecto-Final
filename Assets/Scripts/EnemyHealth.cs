using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida del Monstruo")]
    public float maxHealth = 100f;
    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    // Se llamara cuando la pica lo golpee
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Monstruo recibio " + damageAmount + " de daño. Vida restante: " + currentHealth);

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