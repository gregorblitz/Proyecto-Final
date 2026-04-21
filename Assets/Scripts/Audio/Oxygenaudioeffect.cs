// Fausto A. Gómez
// Se conecta con PlayerStatus (de Danna) sin modificarlo.
// Escucha el evento OnOxygenChanged y aplica efectos de audio según el nivel de oxígeno.
// Colocar este script en el mismo GameObject que tiene PlayerStatus.
using UnityEngine;
using UnityEngine.Audio;

public class OxygenAudioEffect : MonoBehaviour
{
    [Header("Referencia al estado del jugador")]
    public PlayerStatus playerStatus; // Arrastra el script PlayerStatus aquí

    [Header("Sonidos de Asfixia")]
    public AudioClip breathingHeavyClip;    // Respiración agitada (oxígeno bajo)
    public AudioClip gaspClip;               // Jadeo cuando el oxígeno se acaba

    [Header("Configuración")]
    [Tooltip("Porcentaje de oxígeno (0-1) a partir del cual empieza el efecto de asfixia")]
    public float lowOxygenThreshold = 0.3f; // 30% de oxígeno
    [Tooltip("Porcentaje donde el efecto es máximo (pitido del mixer, etc.)")]
    public float criticalOxygenThreshold = 0.1f; // 10% de oxígeno

    [Header("Efecto en el Mixer")]
    public AudioMixer mainMixer;
    [Tooltip("Nombre del parámetro expuesto en el Mixer para el Master cutoff")]
    public string masterCutoffParam = "masterCutoff";
    public float normalCutoff   = 22000f;
    public float suffocateCutoff = 500f;  // Sonido muy apagado en estado crítico
    public float filterSmoothing = 3f;

    private AudioSource breathingSource;
    private float targetCutoff;
    private float currentCutoff;
    private bool wasLowOxygen = false; // Para no spam de logs

    private void Awake()
    {
        // Añadimos un AudioSource en este objeto para la respiración
        breathingSource = gameObject.AddComponent<AudioSource>();
        breathingSource.loop   = true;
        breathingSource.volume = 0f;
        breathingSource.spatialBlend = 0f; // Sonido 2D (en la "cabeza" del jugador)

        if (breathingHeavyClip != null)
            breathingSource.clip = breathingHeavyClip;

        currentCutoff = normalCutoff;
        targetCutoff  = normalCutoff;
    }

    private void Start()
    {
        if (playerStatus == null)
        {
            Debug.LogWarning("[OxygenAudioEffect] No hay referencia a PlayerStatus. Búscalo automáticamente...");
            playerStatus = GetComponent<PlayerStatus>();
        }

        if (playerStatus != null)
        {
            // Nos suscribimos al evento de oxígeno de Danna sin tocar su script
            playerStatus.OnOxygenChanged.AddListener(OnOxygenChanged);
            Debug.Log("[OxygenAudioEffect] Conectado a PlayerStatus.OnOxygenChanged");
        }
        else
        {
            Debug.LogError("[OxygenAudioEffect] No se encontró PlayerStatus. El efecto de asfixia no funcionará.");
        }
    }

    // Este método se llama automáticamente cada vez que el oxígeno cambia (gracias al evento de Danna)
    private void OnOxygenChanged(float current, float max)
    {
        float ratio = current / max; // 0 = sin oxígeno, 1 = lleno

        // --- Efecto de respiración agitada ---
        if (ratio <= lowOxygenThreshold)
        {
            // Cuanto menos oxígeno, más fuerte la respiración agitada
            float breathVolume = Mathf.InverseLerp(lowOxygenThreshold, 0f, ratio);
            breathingSource.volume = breathVolume;

            if (!breathingSource.isPlaying)
                breathingSource.Play();

            if (!wasLowOxygen)
            {
                wasLowOxygen = true;
                Debug.Log("[OxygenAudioEffect] Oxígeno bajo: activando efecto de asfixia");
            }
        }
        else
        {
            // Oxígeno normal: apagamos la respiración gradualmente
            breathingSource.volume = 0f;
            if (breathingSource.isPlaying && ratio > lowOxygenThreshold + 0.05f)
                breathingSource.Stop();

            wasLowOxygen = false;
        }

        // --- Filtro del Mixer: sonido apagado en estado crítico ---
        if (ratio <= criticalOxygenThreshold)
        {
            // Mapea ratio de criticalThreshold→0 a normalCutoff→suffocateCutoff
            float t = Mathf.InverseLerp(criticalOxygenThreshold, 0f, ratio);
            targetCutoff = Mathf.Lerp(normalCutoff, suffocateCutoff, t);
        }
        else
        {
            targetCutoff = normalCutoff;
        }
    }

    private void Update()
    {
        // Aplicamos el filtro suavemente
        if (mainMixer == null) return;

        currentCutoff = Mathf.Lerp(currentCutoff, targetCutoff, Time.deltaTime * filterSmoothing);
        mainMixer.SetFloat(masterCutoffParam, currentCutoff);
    }

    private void OnDestroy()
    {
        // Limpieza del listener para evitar errores si el jugador muere
        if (playerStatus != null)
            playerStatus.OnOxygenChanged.RemoveListener(OnOxygenChanged);

        // Restauramos el filtro del mixer
        if (mainMixer != null)
            mainMixer.SetFloat(masterCutoffParam, normalCutoff);
    }
}