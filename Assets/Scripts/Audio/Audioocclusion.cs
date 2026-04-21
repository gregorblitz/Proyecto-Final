// Fausto A. Gómez
using UnityEngine;

// Oclusión de audio: si hay una pared entre este objeto (enemigo/fuente de sonido) y el jugador,
// aplica un filtro Low Pass al mezclador SFX para simular el efecto de sonido amortiguado.
// Colocar este script en el mismo GameObject que tiene el AudioSource del enemigo o sonido ambiente.
// Las paredes deben estar en la Layer "Obstacles" para que el raycast las detecte.
public class AudioOcclusion : MonoBehaviour
{
    [Header("Configuración")]
    public LayerMask wallLayer;          // Layer de paredes/obstáculos (ej: "Obstacles")
    public float checkInterval = 0.2f;  // Cada cuántos segundos revisa oclusión (rendimiento)

    [Header("Valores del Filtro")]
    public float cutoffOccluded = 800f;   // Frecuencia de corte cuando hay pared (muy amortiguado)
    public float cutoffOpen     = 22000f; // Frecuencia de corte sin obstrucción (sonido normal)
    public float filterSmoothing = 5f;   // Qué tan rápido cambia el filtro (lerp speed)

    private Transform playerTransform;
    private float currentCutoff;
    private float targetCutoff;
    private float timer = 0f;

    private void Start()
    {
        // Buscamos al jugador por tag (consistente con el resto del proyecto)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogWarning("[AudioOcclusion] No se encontró el jugador con tag 'Player'");

        currentCutoff = cutoffOpen;
        targetCutoff  = cutoffOpen;
    }

    private void Update()
    {
        if (playerTransform == null || AudioManager.instance == null) return;

        // No hacemos el raycast cada frame para ahorrar rendimiento
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            CheckOcclusion();
        }

        // Aplicamos el filtro suavemente con lerp
        currentCutoff = Mathf.Lerp(currentCutoff, targetCutoff, Time.deltaTime * filterSmoothing);
        AudioManager.instance.SetSFXLowPassCutoff(currentCutoff);
    }

    private void CheckOcclusion()
    {
        // Raycast desde este objeto hacia el jugador
        Vector3 direction = playerTransform.position - transform.position;
        float distance    = direction.magnitude;

        // Si el rayo golpea una pared antes de llegar al jugador, hay oclusión
        if (Physics.Raycast(transform.position, direction.normalized, distance, wallLayer))
        {
            targetCutoff = cutoffOccluded; // Amortiguamos el sonido
        }
        else
        {
            targetCutoff = cutoffOpen;     // Sonido normal
        }
    }

    private void OnDisable()
    {
        // Reseteamos el filtro si el objeto se desactiva
        if (AudioManager.instance != null)
            AudioManager.instance.ResetSFXFilter();
    }
}