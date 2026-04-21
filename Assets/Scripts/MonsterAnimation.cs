using UnityEngine;

//**SCRIPT COMUNICACION ENTRE LOS SCRIPTS DEL PADRE Y EL ANIMATOR (HIJO)

public class MonsterAnimation : MonoBehaviour
{
    private Monster mainScript;

    void Awake()
    {
        // Busca el cerebro principal del monstruo en el objeto Padre (Creep2)
        mainScript = GetComponentInParent<Monster>();
    }

    // El Animator del modelo invoca esta funcion porque estan en el mismo lugar
    public void ApplyDamageToPlayer()
    {
        if (mainScript != null)
        {
            // Le pasa el mensaje al script grande para que haga el daño
            mainScript.ApplyDamageToPlayer();
        }
    }
}