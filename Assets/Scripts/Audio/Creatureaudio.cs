// Fausto A. Gómez
// Script SEPARADO de CreatureController. No lo modifica en absoluto.
// Se engancha a los UnityEvents públicos que ya expone CreatureController
// (OnAlert, OnHunting, OnStalking, OnAttacking, OnFleeing, OnIdleOrPatrolling)
// para reproducir sonidos según el estado de la criatura.
//
// COLOCAR en el mismo GameObject que CreatureController.
// CONECTAR en el Inspector: arrastrar cada método al UnityEvent correspondiente.
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CreatureController))]
public class CreatureAudio : MonoBehaviour
{
    // ── Clips por estado ─────────────────────────────────────────────────
    [Header("Clips — Detección")]
    [Tooltip("Sonido al detectar al jugador. Corto e impactante. (ej: Monster Roar — Universfield)")]
    public AudioClip alertClip;

    [Header("Clips — Persecución")]
    [Tooltip("Pasos de la criatura mientras persigue. (ej: Monster Footstep — LordSonny)")]
    public AudioClip huntingStepClip;
    [Tooltip("Gruñido grave mientras persigue. (ej: Monster Growl — DRAGON-STUDIO 0:05)")]
    public AudioClip huntingGrowlClip;

    [Header("Clips — Acecho (Stalking)")]
    [Tooltip("Loop suave de tensión durante el acecho. (ej: Horror Sound: Lurking Horror Monster — AlesiaDavina)")]
    public AudioClip stalkingLoopClip;

    [Header("Clips — Ataque")]
    [Tooltip("Sonido del jumpscare. (ej: Angry Beast — freesound_community 0:13)")]
    public AudioClip attackClip;

    [Header("Clips — Fuga")]
    [Tooltip("Sonido al huir / esconderse. Opcional. (ej: Monster Growl — freesound_community 0:07)")]
    public AudioClip fleeClip;

    // ── Parámetros ───────────────────────────────────────────────────────
    [Header("Pasos en Hunting")]
    public float stepInterval = 0.55f;
    public float stepVolume   = 0.85f;

    [Header("Gruñidos en Hunting")]
    [Tooltip("Cada cuántos segundos suena el gruñido mientras persigue")]
    public float growlInterval = 4f;
    public float growlVolume   = 0.7f;

    [Header("Acecho")]
    [Tooltip("Volumen del loop de acecho")]
    public float stalkingVolume = 0.55f;
    [Tooltip("Duración del fade in/out del loop (segundos)")]
    public float stalkingFadeDuration = 1.2f;

    // ── Referencias internas ─────────────────────────────────────────────
    // sfxSource: sonidos puntuales (alert, pasos, ataque)
    // loopSource: exclusivo para el loop de acecho
    private AudioSource sfxSource;
    private AudioSource loopSource;

    private Coroutine stepRoutine;
    private Coroutine growlRoutine;
    private Coroutine fadeRoutine;

    // ── Estado actual ────────────────────────────────────────────────────
    private bool isHunting  = false;
    private bool isStalking = false;

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Primer AudioSource: sonidos puntuales
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.spatialBlend = 1f;
        sfxSource.playOnAwake  = false;
        sfxSource.loop         = false;

        // Segundo AudioSource: loop de acecho
        // Si ya hay más de uno en el objeto lo reutilizamos para no crear duplicados
        AudioSource[] allSources = GetComponents<AudioSource>();
        loopSource = allSources.Length > 1 ? allSources[1] : gameObject.AddComponent<AudioSource>();

        loopSource.spatialBlend = 1f;
        loopSource.playOnAwake  = false;
        loopSource.loop         = true;
        loopSource.volume       = 0f;
    }

    // ─────────────────────────────────────────────────────────────────────
    // MÉTODOS PÚBLICOS — conectar desde el Inspector a los UnityEvents
    // de CreatureController (OnAlert, OnHunting, OnStalking, OnAttacking,
    // OnFleeing, OnIdleOrPatrolling)
    // ─────────────────────────────────────────────────────────────────────

    // → Conectar a: OnAlert
    public void OnAlert()
    {
        StopHunting();
        StopStalking();

        if (alertClip != null)
            sfxSource.PlayOneShot(alertClip);

        // Avisar al AudioManager para activar música de persecución
        if (AudioManager.instance != null)
            AudioManager.instance.SetChaseMode(true);

        Debug.Log("[CreatureAudio] Estado: ALERT");
    }

    // → Conectar a: OnHunting
    public void OnHunting()
    {
        StopStalking();

        if (!isHunting)
        {
            isHunting    = true;
            stepRoutine  = StartCoroutine(StepLoop());
            growlRoutine = StartCoroutine(GrowlLoop());
        }

        // La música de chase sigue activa desde Alert
        Debug.Log("[CreatureAudio] Estado: HUNTING");
    }

    // → Conectar a: OnStalking
    public void OnStalking()
    {
        StopHunting();

        if (!isStalking)
        {
            isStalking = true;
            StartStalkingLoop();
        }

        Debug.Log("[CreatureAudio] Estado: STALKING");
    }

    // → Conectar a: OnAttacking
    public void OnAttacking()
    {
        StopHunting();
        StopStalking();

        if (attackClip != null)
            sfxSource.PlayOneShot(attackClip);

        Debug.Log("[CreatureAudio] Estado: ATTACKING");
    }

    // → Conectar a: OnFleeing
    public void OnFleeing()
    {
        StopHunting();
        StopStalking();

        if (fleeClip != null)
            sfxSource.PlayOneShot(fleeClip, 0.6f);

        // Música vuelve al ambiente
        if (AudioManager.instance != null)
            AudioManager.instance.SetChaseMode(false);

        Debug.Log("[CreatureAudio] Estado: FLEEING");
    }

    // → Conectar a: OnIdleOrPatrolling
    public void OnIdleOrPatrolling()
    {
        StopHunting();
        StopStalking();

        // Música vuelve al ambiente solo si venía de perseguir
        if (AudioManager.instance != null)
            AudioManager.instance.SetChaseMode(false);

        Debug.Log("[CreatureAudio] Estado: IDLE/PATROLLING");
    }

    // ─────────────────────────────────────────────────────────────────────
    // LOOPS INTERNOS
    // ─────────────────────────────────────────────────────────────────────

    private IEnumerator StepLoop()
    {
        while (isHunting)
        {
            if (huntingStepClip != null)
                sfxSource.PlayOneShot(huntingStepClip, stepVolume);
            yield return new WaitForSeconds(stepInterval);
        }
    }

    private IEnumerator GrowlLoop()
    {
        // Primer gruñido con un pequeño delay para no solapar con el alertClip
        yield return new WaitForSeconds(2f);
        while (isHunting)
        {
            if (huntingGrowlClip != null)
                sfxSource.PlayOneShot(huntingGrowlClip, growlVolume);
            yield return new WaitForSeconds(growlInterval);
        }
    }

    private void StartStalkingLoop()
    {
        if (stalkingLoopClip == null) return;
        loopSource.clip   = stalkingLoopClip;
        loopSource.volume = 0f;
        loopSource.Play();

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeLoop(0f, stalkingVolume, stalkingFadeDuration));
    }

    private void StopHunting()
    {
        if (!isHunting) return;
        isHunting = false;
        if (stepRoutine  != null) { StopCoroutine(stepRoutine);  stepRoutine  = null; }
        if (growlRoutine != null) { StopCoroutine(growlRoutine); growlRoutine = null; }
    }

    private void StopStalking()
    {
        if (!isStalking) return;
        isStalking = false;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeLoop(loopSource.volume, 0f, stalkingFadeDuration * 0.6f));
    }

    private IEnumerator FadeLoop(float from, float to, float duration)
    {
        float time = 0f;
        loopSource.volume = from;

        while (time < duration)
        {
            time += Time.deltaTime;
            loopSource.volume = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }

        loopSource.volume = to;
        if (to <= 0f) loopSource.Stop();
    }

    // ─────────────────────────────────────────────────────────────────────
    // LIMPIEZA
    // ─────────────────────────────────────────────────────────────────────
    private void OnDisable()
    {
        StopHunting();
        StopStalking();

        if (AudioManager.instance != null)
            AudioManager.instance.SetChaseMode(false);
    }
}