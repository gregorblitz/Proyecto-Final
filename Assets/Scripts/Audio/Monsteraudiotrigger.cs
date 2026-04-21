// Fausto A. Gómez
// Script pequeño que se pone en el mismo GameObject que Monster.cs.
// Detecta cuándo el monstruo empieza a perseguir y le avisa al AudioManager.
// NO modifica Monster.cs (de otro compañero).
using UnityEngine;

// Necesitamos leer el estado del monstruo. Como Monster.cs es de otro compañero,
// lo referenciamos pero no lo modificamos.
[RequireComponent(typeof(Monster))]
public class MonsterAudioTrigger : MonoBehaviour
{
    [Header("Clips de Sonido del Monstruo")]
    public AudioClip roarClip;       // Rugido al detectar al jugador
    public AudioClip footstepClip;   // Paso del monstruo (si se quiere 3D)
    public AudioClip attackClip;     // Sonido de ataque

    [Header("Configuración")]
    [Tooltip("Cada cuántos segundos revisa si el monstruo está persiguiendo")]
    public float checkInterval = 0.3f;

    private Monster monsterScript;
    private AudioSource audioSource;
    private bool wasChasing = false;
    private bool roarPlayed = false;
    private float timer = 0f;

    private void Start()
    {
        monsterScript = GetComponent<Monster>();
        audioSource   = GetComponent<AudioSource>();

        // Si no tiene AudioSource, lo añadimos (con salida al grupo SFX del mixer)
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f; // Sonido 3D (se escucha desde la posición del monstruo)
        audioSource.rolloffMode  = AudioRolloffMode.Linear;
        audioSource.maxDistance  = 20f;
    }

    private void Update()
    {
        if (monsterScript == null || AudioManager.instance == null) return;

        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        // Leemos el campo público isRunning de Monster.cs para saber si está persiguiendo
        bool isCurrentlyChasing = monsterScript.isRunning;

        if (isCurrentlyChasing && !wasChasing)
        {
            // El monstruo acaba de empezar a perseguir
            wasChasing = true;
            AudioManager.instance.SetChaseMode(true);
            Debug.Log("[MonsterAudioTrigger] Persecución iniciada → música de chase");

            // El rugido se reproduce una sola vez al detectar
            if (!roarPlayed && roarClip != null)
            {
                audioSource.PlayOneShot(roarClip);
                roarPlayed = true;
            }
        }
        else if (!isCurrentlyChasing && wasChasing)
        {
            // El monstruo dejó de perseguir (si implementas lógica de "perdió al jugador")
            wasChasing = false;
            roarPlayed = false;
            AudioManager.instance.SetChaseMode(false);
            Debug.Log("[MonsterAudioTrigger] Persecución terminada → música de ambiente");
        }
    }

    // Llámalo desde la animación del monstruo (igual que ApplyDamageToPlayer)
    public void PlayAttackSound()
    {
        if (attackClip != null)
            audioSource.PlayOneShot(attackClip);
    }
}