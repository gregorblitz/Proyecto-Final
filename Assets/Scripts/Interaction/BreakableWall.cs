// Fausto A. Gómez
using UnityEngine;
using System.Collections;

// Controla la pared rompible: muestra la versión completa al inicio,
// y cuando se usa el taladro (ID: "Drill"), activa los fragmentos con física.
// Requiere prefab con: hijo "Completa" (mesh entero) e hijo "Fragmentos" (trozos).
// El jugador activa esto presionando E con el taladro en mano, igual que con las puertas.
public class BreakableWall : MonoBehaviour, IInteractable
{
    [Header("Referencias del Prefab")]
    public GameObject wallComplete;    // El hijo "Completa"
    public GameObject wallFragments;   // El hijo "Fragmentos"

    [Header("Configuración")]
    public string objectName = "Pared";
    public string requiredToolID = "Drill"; // Debe coincidir con ItemData.interactionID del taladro

    [Header("Fragmentos - Física")]
    public float explosionForce = 300f;
    public float explosionRadius = 2f;
    public float fragmentLifetime = 5f; // Segundos hasta que desaparecen los fragmentos

    [Header("Estado")]
    public bool isBroken = false;

    private void Start()
    {
        // Estado inicial: pared entera visible, fragmentos ocultos
        if (wallComplete != null) wallComplete.SetActive(true);
        if (wallFragments != null) wallFragments.SetActive(false);
    }

    // Mensaje que ve el jugador al acercarse (mismo patrón que DoorController)
    public string GetInteractionMessage()
    {
        if (isBroken) return "";
        return $"Equipa el Taladro y presiona E para romper";
    }

    // Implementación obligatoria de IInteractable
    // PlayerInteractor llama esto con el selectedItem del jugador
    public void Interact(ItemData itemInHand = null)
    {
        if (isBroken) return;

        if (itemInHand == null || itemInHand.interactionID != requiredToolID)
        {
            Debug.Log($"[{objectName}] Necesitas el Taladro para romper esto.");
            return;
        }

        Debug.Log($"[{objectName}] ¡Taladro activado! Rompiendo pared...");
        StartCoroutine(BreakWall());
    }

    private IEnumerator BreakWall()
    {
        isBroken = true;

        // Ocultamos la pared entera
        if (wallComplete != null)
            wallComplete.SetActive(false);

        // Activamos los fragmentos y les aplicamos física
        if (wallFragments != null)
        {
            wallFragments.SetActive(true);

            Vector3 explosionCenter = transform.position;

            foreach (Transform fragment in wallFragments.transform)
            {
                // Si el fragmento no tiene Rigidbody, lo añadimos en tiempo de ejecución
                Rigidbody rb = fragment.GetComponent<Rigidbody>();
                if (rb == null)
                    rb = fragment.gameObject.AddComponent<Rigidbody>();

                rb.isKinematic = false;

                // Offset aleatorio para que cada fragmento salga diferente
                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.3f, 0.3f),
                    Random.Range(0f,    0.3f),
                    Random.Range(-0.3f, 0.3f)
                );

                rb.AddExplosionForce(
                    explosionForce,
                    explosionCenter + randomOffset,
                    explosionRadius,
                    1f,
                    ForceMode.Impulse
                );
            }
        }

        // Limpiamos los fragmentos después de un tiempo
        yield return new WaitForSeconds(fragmentLifetime);

        if (wallFragments != null)
            Destroy(wallFragments);

        Debug.Log($"[{objectName}] Fragmentos limpiados de la escena.");
    }

    // Visualización en el editor (mismo patrón que Monster.cs)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(1.5f, 2f, 0.5f));
    }
}