using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    [Header("Daño del Arma")]
    public float damage = 25f; // Le quitará 25 de vida por golpe (4 golpes = muerte)

    private void OnTriggerEnter(Collider other)
    {
        // Nos dira en la consola todo lo que toca la pica
        Debug.Log("La pica toco un objeto llamado: " + other.gameObject.name + " el Tag es: " + other.tag);
        // Si golpea un objeto con etiqueta "Enemy"
        if (other.CompareTag("Enemy"))
        {
            // Busca el script de vida
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                // Apaga el collider para no hacer daño 2 veces en un solo movimiento
                GetComponent<Collider>().enabled = false;
            }
            else
            {
                Debug.LogWarning("Toco a un Enemy pero no se encuentra el script EnemyHealth");
            }
        }
    }
}