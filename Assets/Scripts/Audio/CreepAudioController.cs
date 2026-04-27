// Fausto A. Gómez
// Controla el sonido de los Creeps (las criaturas pequeñas de la escena).
// Reproduce el clip de beast-scream en intervalos aleatorios para dar atmósfera.
// Colocar en el mismo GameObject que tenga el componente de lógica del Creep.
using UnityEngine;

public class CreepAudioController : MonoBehaviour
{
    [Header("Clip de Sonido")]
    [Tooltip("Si se deja vacío, usa el clip creepSoundClip del AudioManager")]
    public AudioClip creepClip;

    [Header("Intervalos Aleatorios")]
    [Tooltip("Tiempo mínimo en segundos entre cada sonido del creep")]
    public float minInterval = 4f;
    [Tooltip("Tiempo máximo en segundos entre cada sonido del creep")]
    public float maxInterval = 10f;

    [Header("Configuración de Audio")]
    public float volume      = 0.85f;
    public float maxDistance = 15f;
    public float minDistance = 1f;

    private AudioSource audioSource;
    private float timer     = 0f;
    private float nextTime  = 0f; // cuándo suena el siguiente grito

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Sonido 3D posicional
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode  = AudioRolloffMode.Linear;
        audioSource.maxDistance  = maxDistance;
        audioSource.minDistance  = minDistance;
        audioSource.volume       = volume;

        // Usamos el clip del AudioManager si no se asignó uno local
        if (creepClip == null && AudioManager.instance != null)
            creepClip = AudioManager.instance.creepSoundClip;

        // Primer sonido en un momento aleatorio para que no suenen todos a la vez
        nextTime = Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        if (creepClip == null) return;

        timer += Time.deltaTime;

        if (timer >= nextTime)
        {
            timer    = 0f;
            nextTime = Random.Range(minInterval, maxInterval);
            audioSource.PlayOneShot(creepClip, volume);
            Debug.Log("[CreepAudio] Sonido de creep reproducido");
        }
    }
}