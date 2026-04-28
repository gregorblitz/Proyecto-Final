// Fausto A. Gómez
using UnityEngine;

// Controla todos los sonidos del jugador: pasos, salto/aterrizaje y arrastre.
// Se engancha a PlayerController leyendo su estado sin modificarlo.
// Colocar en el mismo GameObject que PlayerController.
// Los clips se asignan directamente desde AudioManager (no hacen falta referencias extra).
[RequireComponent(typeof(PlayerController))]
public class PlayerAudioController : MonoBehaviour
{
    public enum SurfaceType { Gravel, Concrete, Stone }

    [Header("Tipo de superficie (cambiar según la escena)")]
    public SurfaceType currentSurface = SurfaceType.Stone;

    [Header("Configuración de Pasos")]
    [Tooltip("Tiempo en segundos entre cada paso caminando")]
    public float stepIntervalWalk = 0.45f;

    [Tooltip("Tiempo en segundos entre cada paso corriendo")]
    public float stepIntervalRun  = 0.28f;

    [Tooltip("Tiempo en segundos entre sonidos de arrastre")]
    public float crawlInterval    = 0.6f;

    [Header("Volúmenes")]
    public float stepVolume   = 0.7f;
    public float landVolume   = 0.9f;
    public float crawlVolume  = 0.5f;

    private PlayerController playerController;
    private Rigidbody rb;
    private CapsuleCollider col;

    private float stepTimer  = 0f;
    private bool  wasInAir   = false;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        rb  = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (AudioManager.instance == null) return;

        HandleFootsteps();
        HandleJumpLanding();
    }

    private void HandleFootsteps()
    {
        // Leemos la velocidad horizontal para saber si el jugador se mueve
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool isMoving = horizontalVel.magnitude > 0.3f;
        bool isGrounded = IsGrounded();

        if (!isGrounded || !isMoving) 
        {
            stepTimer = 0f;
            return;
        }

        bool isRunning  = horizontalVel.magnitude > 5.5f;
        bool isCrawling = IsCrawling();

        if (isCrawling)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= crawlInterval)
            {
                stepTimer = 0f;
                AudioManager.instance.PlaySFX(AudioManager.instance.crawlClip, crawlVolume);
            }
            return;
        }

        float interval = isRunning ? stepIntervalRun : stepIntervalWalk;
        stepTimer += Time.deltaTime;

        if (stepTimer >= interval)
        {
            stepTimer = 0f;
            PlayFootstepForSurface();
        }
    }

    private void HandleJumpLanding()
    {
        bool isGrounded = IsGrounded();

        if (!isGrounded)
        {
            wasInAir = true;
        }
        else if (wasInAir && isGrounded)
        {
            wasInAir = false;
            AudioManager.instance.PlaySFX(AudioManager.instance.jumpLandingClip, landVolume);
            Debug.Log("[PlayerAudio] Aterrizaje");
        }
    }

    private void PlayFootstepForSurface()
    {
        AudioClip clip = null;

        switch (currentSurface)
        {
            case SurfaceType.Gravel:
                clip = AudioManager.instance.footstepGravelClip;
                break;
            case SurfaceType.Concrete:
                clip = AudioManager.instance.footstepConcreteClip;
                break;
            case SurfaceType.Stone:
                clip = AudioManager.instance.footstepStoneClip;
                break;
        }

        AudioManager.instance.PlaySFX(clip, stepVolume);
    }

    private bool IsGrounded()
    {
        if (col == null) return false;
        float dist = col.bounds.extents.y;
        return Physics.Raycast(transform.position, Vector3.down, dist + 0.15f);
    }

    private bool IsCrawling()
    {
        if (col == null) return false;
        return col.height < 1.5f;
    }
}