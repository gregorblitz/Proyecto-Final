using UnityEngine;

//***SCRIPT QUE SIRVE DE PUENTE EN EL MODELO 3D PARA QUE ESCUCHE
//***AL ANIMATOR Y SE COMUNIQUE CON EL SCRIPT PRINCIPAL (PLAYERCONTROLLER OBJ PADRE)  
public class AnimationEventRelay : MonoBehaviour
{
    // Variable para guardar al jugador principal
    private PlayerController playerController;

    private void Awake()
    {
        // Esto le dice al modelo 3D: "Busca hacia arriba y encuentra al PlayerController en mi Padre"
        playerController = GetComponentInParent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogWarning("El mensajero no encontró el PlayerController en el padre.");
        }
    }

    // El Animator del modelo 3D llamará a esta función
    public void ActivarDaño()
    {
        if (playerController != null)
        {
            playerController.ActivarDaño(); // Le pasa el recado al padre
        }
    }

    // El Animator del modelo 3D llamará a esta otra
    public void DesactivarDaño()
    {
        if (playerController != null)
        {
            playerController.DesactivarDaño(); // Le pasa el recado al padre
        }
    }
}