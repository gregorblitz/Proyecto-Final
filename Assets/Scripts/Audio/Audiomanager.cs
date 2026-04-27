// Fausto A. Gómez
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

// Gestor central de audio del juego. Singleton que persiste entre escenas.
// Controla la música de ambiente y persecución, y expone métodos para los efectos de sonido.
// Objeto recomendado en escena: "_Audio" (vacío) con este script + AudioSources configurados.
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Mixer y Grupos")]
    public AudioMixer mainMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup uiGroup;

    [Header("Fuentes de Música")]
    public AudioSource ambientSource;   // Sonido de ambiente normal (loop)
    public AudioSource chaseSource;     // Música de persecución (loop)

    [Header("Fuente de SFX General")]
    public AudioSource sfxSource;       // Para efectos puntuales (pasos, objetos)

    [Header("Transición de Música")]
    public float crossfadeDuration = 1.5f;

    // -------------------------------------------------------
    // CLIPS DE AUDIO — arrastrar desde Assets/Audio/SFX
    // -------------------------------------------------------
    [Header("Clips — Ambiente")]
    public AudioClip caveAmbienceClip;          // Cave Ambience/darkdrone3

    [Header("Clips — Jugador")]
    public AudioClip footstepGravelClip;        // footsteps/footsteps-on-gravel
    public AudioClip footstepConcreteClip;      // footsteps/freesound_community-concrete
    public AudioClip footstepStoneClip;         // footsteps/freesound_community-stone
    public AudioClip jumpLandingClip;           // Salto/jumplanding
    public AudioClip crawlClip;                 // Arrastrar/cloth-rustle

    [Header("Clips — Herramientas")]
    public AudioClip drillClip;                 // Drill/drill_perforator
    public AudioClip pickaxeClip;               // Pico/creatorshome-pickaxe

    [Header("Clips — Objetos / UI")]
    public AudioClip pickupClip;                // Pickup/litupsubway-key-collect-sfx
    public AudioClip checkpointClip;            // Checkpoint/magical-sparkle-whoosh

    [Header("Clips — Enemigo Principal")]
    public AudioClip enemyHitClip;              // Enemy Hit/hit-flesh
    public AudioClip enemyStepsClip;            // Enemy Steps/monster_growl

    [Header("Clips — Creeps")]
    public AudioClip creepSoundClip;            // Creep Sounds/beast-scream-slower-ghostly-breath

    // -------------------------------------------------------
    private bool isChasing = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Arrancamos el ambiente desde el inicio
        if (ambientSource != null && !ambientSource.isPlaying)
        {
            if (caveAmbienceClip != null) ambientSource.clip = caveAmbienceClip;
            ambientSource.loop = true;
            ambientSource.Play();
        }

        // La música de persecución comienza en silencio
        if (chaseSource != null)
        {
            chaseSource.volume = 0f;
            chaseSource.loop   = true;
            chaseSource.Play();
        }
    }

    // -------------------------------------------------------
    // MÚSICA
    // -------------------------------------------------------

    // Llámalo desde MonsterAudioTrigger cuando el monstruo detecta al jugador
    public void SetChaseMode(bool active)
    {
        if (isChasing == active) return;
        isChasing = active;

        StopAllCoroutines();
        StartCoroutine(CrossfadeMusic(active));

        Debug.Log("[AudioManager] Modo persecución: " + (active ? "ACTIVO" : "INACTIVO"));
    }

    private IEnumerator CrossfadeMusic(bool chasing)
    {
        float targetChase   = chasing ? 1f : 0f;
        float targetAmbient = chasing ? 0.4f : 1f;

        float startChase   = chaseSource.volume;
        float startAmbient = ambientSource.volume;
        float time = 0f;

        while (time < crossfadeDuration)
        {
            time += Time.deltaTime;
            float t = time / crossfadeDuration;

            chaseSource.volume   = Mathf.Lerp(startChase,   targetChase,   t);
            ambientSource.volume = Mathf.Lerp(startAmbient, targetAmbient, t);

            yield return null;
        }

        chaseSource.volume   = targetChase;
        ambientSource.volume = targetAmbient;
    }

    // -------------------------------------------------------
    // EFECTOS DE SONIDO — métodos públicos para otros scripts
    // -------------------------------------------------------

    // Reproduce un clip una vez en una posición del mundo (sonido 3D)
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    // Reproduce un clip en la fuente central de SFX (sonido 2D)
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // Atajos para los sonidos más usados — así los otros scripts no necesitan la referencia del clip
    public void PlayPickup()    => PlaySFX(pickupClip);
    public void PlayCheckpoint()=> PlaySFX(checkpointClip);
    public void PlayDrill(Vector3 pos)   => PlaySFXAtPoint(drillClip,   pos);
    public void PlayPickaxe(Vector3 pos) => PlaySFXAtPoint(pickaxeClip, pos);
    public void PlayEnemyHit(Vector3 pos)=> PlaySFXAtPoint(enemyHitClip, pos);
    public void PlayCreepSound(Vector3 pos) => PlaySFXAtPoint(creepSoundClip, pos);

    // -------------------------------------------------------
    // FILTRO DE OCLUSIÓN (para AudioOcclusion.cs)
    // -------------------------------------------------------

    public void SetSFXLowPassCutoff(float cutoff)
    {
        if (mainMixer == null) return;
        mainMixer.SetFloat("sfxCutoff", cutoff);
    }

    public void ResetSFXFilter()
    {
        if (mainMixer == null) return;
        mainMixer.SetFloat("sfxCutoff", 22000f);
    }
}