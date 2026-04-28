// Fausto A. Gómez
using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Configuración de Movimiento")]
    public Transform doorPivot;
    public float openAngle = 90f;
    public float speed = 2f;

    [Header("Seguridad")]
    public bool requiresKey = false;
    public string requiredID; // El ID que debe coincidir con el de la llave

    [Header("UI e Información")]
    public string objectName = "Puerta";

    // NUEVO: sonido de la puerta
    [Header("Audio")]
    public AudioClip doorSound; // Arrastrar el clip desde Assets/Audio/SFX
    private bool isOpen = false;
    private bool isMoving = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private AudioSource audioSource; // NUEVO

    private void Start()
    {
        if (doorPivot == null) doorPivot = transform;
        
        closedRotation = doorPivot.localRotation;
        openRotation = Quaternion.Euler(0, openAngle, 0) * closedRotation;

         // NUEVO: preparar el AudioSource local en la puerta (sonido 3D posicional)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake  = false;
    }

    // Método para que el PlayerInteractor sepa qué mensaje mostrar
    public string GetInteractionMessage()
    {
        if (isOpen) return "Presiona E para cerrar";
        if (requiresKey) return $"Necesitas la {requiredID}";
        return "Presiona E para abrir";
    }

    // Implementación obligatoria de la interfaz
    public void Interact(ItemData itemInHand = null)
    {
        if (isMoving) return;

        // Si la puerta está cerrada y requiere llave
        if (!isOpen && requiresKey)
        {
            if (itemInHand != null && itemInHand.interactionID == requiredID)
            {
                Debug.Log("¡Llave correcta! Abriendo...");
                StartCoroutine(RotateDoor());
            }
            else
            {
                Debug.Log("La puerta está bloqueada. Necesitas: " + requiredID);
            }
        }
        else
        {
            // Si ya está abierta o no requiere llave, se mueve normalmente
            StartCoroutine(RotateDoor());
        }
    }

    private IEnumerator RotateDoor()
    {
        isMoving = true;

        // NUEVO: reproducir sonido al empezar a moverse
        if (doorSound != null && audioSource != null)
            audioSource.PlayOneShot(doorSound);
            
        Quaternion targetRotation = isOpen ? closedRotation : openRotation;

        while (Quaternion.Angle(doorPivot.localRotation, targetRotation) > 0.01f)
        {
            doorPivot.localRotation = Quaternion.Slerp(
                doorPivot.localRotation,
                targetRotation,
                Time.deltaTime * speed
            );
            yield return null;
        }

        doorPivot.localRotation = targetRotation;
        isOpen = !isOpen;
        isMoving = false;
    }
}