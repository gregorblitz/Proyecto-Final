// Fausto A. Gómez
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

// Gestor central de audio del juego. Singleton que persiste entre escenas.
// Controla la música de ambiente y persecución, y expone métodos para los efectos de sonido.
// Objeto recomendado en escena: "_Audio" (vacío) con este script + 2 AudioSources configurados.
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

    // Para que el Monster pueda activar el modo persecución
    private bool isChasing = false;

    private void Awake()
    {
        // Patrón Singleton clásico
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
            ambientSource.Play();

        // La música de persecución comienza en silencio
        if (chaseSource != null)
        {
            chaseSource.volume = 0f;
            chaseSource.Play();
        }
    }

    // --- MÚSICA ---

    // Llámalo desde Monster.cs cuando detecta al jugador
    public void SetChaseMode(bool active)
    {
        if (isChasing == active) return; // evitamos activar dos veces
        isChasing = active;

        StopAllCoroutines();
        StartCoroutine(CrossfadeMusic(active));

        Debug.Log("[AudioManager] Modo persecución: " + (active ? "ACTIVO" : "INACTIVO"));
    }

    private IEnumerator CrossfadeMusic(bool chasing)
    {
        float targetChase   = chasing ? 1f : 0f;
        float targetAmbient = chasing ? 0.4f : 1f; // el ambiente baja pero no se silencia del todo

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

        // Forzamos los valores finales por precisión
        chaseSource.volume   = targetChase;
        ambientSource.volume = targetAmbient;
    }

    // --- EFECTOS DE SONIDO ---

    // Reproduce un clip una vez en la posición del mundo (sonido 3D)
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    // Reproduce un clip en la fuente central de SFX (sin posición 3D)
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // --- FILTRO DE OCLUSIÓN (para efecto de "sonido amortiguado por paredes") ---
    // Llámalo desde AudioOcclusion.cs para ajustar el filtro del mixer SFX

    // cutoff: 0 = completamente amortiguado, 22000 = normal sin filtro
    public void SetSFXLowPassCutoff(float cutoff)
    {
        if (mainMixer == null) return;
        mainMixer.SetFloat("sfxCutoff", cutoff);
    }

    // Restaura el sonido SFX a normal
    public void ResetSFXFilter()
    {
        if (mainMixer == null) return;
        mainMixer.SetFloat("sfxCutoff", 22000f);
    }
}