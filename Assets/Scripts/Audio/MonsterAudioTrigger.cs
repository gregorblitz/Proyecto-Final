// Fausto A. Gómez
// Script que se pone en el mismo GameObject que Monster.cs.
// Detecta cuándo el monstruo empieza a perseguir y le avisa al AudioManager.
// También maneja los pasos del enemigo y el sonido de golpe.
// NO modifica Monster.cs (de otro compañero).
using UnityEngine;
using System.Collections;

//[RequireComponent(typeof(Monster))]
public class MonsterAudioTrigger : MonoBehaviour
{
    [Header("Clips de Sonido del Monstruo")]
    public AudioClip roarClip;           // Rugido al detectar al jugador
    public AudioClip attackClip;         // Sonido de golpe al jugador (Enemy Hit/hit-flesh)
    public AudioClip footstepClip;       // Pasos del monstruo (Enemy Steps/monster_growl)

    [Header("Configuración de Pasos del Monstruo")]
    [Tooltip("Tiempo entre cada paso del monstruo mientras persigue")]
    public float stepInterval = 0.55f;
    public float stepVolume   = 0.8f;

    [Header("Configuración General")]
    [Tooltip("Cada cuántos segundos revisa si el monstruo está persiguiendo")]
    public float checkInterval = 0.3f;

    private Monster monsterScript;
    private AudioSource audioSource;

    private bool wasChasing  = false;
    private bool roarPlayed  = false;
    private float checkTimer = 0f;
    private float stepTimer  = 0f;

    private void Start()
    {
        monsterScript = GetComponent<Monster>();
        audioSource   = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Sonido 3D: se escucha desde la posición del monstruo en el mundo
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode  = AudioRolloffMode.Linear;
        audioSource.maxDistance  = 22f;
        audioSource.minDistance  = 1f;
    }

    private void Update()
    {
        if (monsterScript == null || AudioManager.instance == null) return;

        // --- Revisión de estado de persecución ---
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckChaseState();
        }

        // --- Pasos del monstruo mientras persigue ---
        if (monsterScript.isRunning)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                if (footstepClip != null)
                    audioSource.PlayOneShot(footstepClip, stepVolume);
            }
        }
        else
        {
            stepTimer = 0f; // reinicia para que el primer paso suene bien
        }
    }

    private void CheckChaseState()
    {
        // Leemos isRunning de Monster.cs sin tocarlo
        bool isCurrentlyChasing = monsterScript.isRunning;

        if (isCurrentlyChasing && !wasChasing)
        {
            wasChasing = true;
            AudioManager.instance.SetChaseMode(true);
            Debug.Log("[MonsterAudioTrigger] Persecución iniciada → música de chase");

            // Rugido una sola vez al detectar
            if (!roarPlayed && roarClip != null)
            {
                audioSource.PlayOneShot(roarClip);
                roarPlayed = true;
            }
        }
        else if (!isCurrentlyChasing && wasChasing)
        {
            wasChasing = false;
            roarPlayed = false;
            AudioManager.instance.SetChaseMode(false);
            Debug.Log("[MonsterAudioTrigger] Persecución terminada → música de ambiente");
        }
    }

    // Llámalo desde el Animator del monstruo, igual que ApplyDamageToPlayer en MonsterAnimation.cs
    // En el evento de animación de ataque, agregar este método además del de daño
    public void PlayAttackSound()
    {
        AudioClip clip = attackClip != null ? attackClip : AudioManager.instance.enemyHitClip;
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}