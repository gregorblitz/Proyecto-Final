using UnityEngine;
using UnityEngine.Events;

// Script creado por Danna. Contanctar con ella antes de hacer cambios significativos para evitar conflictos.
public class DamageZone : MonoBehaviour
{

    public static UnityEvent OnHealthDamageArea = new UnityEvent();
    public static UnityEvent OnOxygenLossArea = new UnityEvent();
    public static UnityEvent OnMadnessGainArea = new UnityEvent();
    public static UnityEvent OnSanityGainArea = new UnityEvent();
    public static UnityEvent OnOutOfArea = new UnityEvent();

    public static int test = 5;

    public enum EffectType { HealthDamage, OxygenLoss, MadnessGain, SanityGain }
    public EffectType effect;
    public float amountPerSecond = 10f;
    public bool continuous = true;

    [Header("Gizmos Visualization")]
    public Material particleMatForArea;
    public Color particleColor;

    
    [Header("Gizmos Visualization")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);
    public bool showGizmo = true;

    private PlayerStatus playerStatus;
    private Collider zoneCollider;
    private ParticleSystemRenderer TEST;


    private void OnEnable() {
        playerStatus.OnPlayerDeath.AddListener(RemoveEffect);
    }
    private void Awake() {
        playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatus>();
    }
    void Start()
    {
        
        zoneCollider = GetComponent<Collider>();
        if (zoneCollider == null)
            Debug.LogWarning("DamageZone necesita un Collider (preferiblemente Trigger)");

        //GetComponent<MeshRenderer>().material.color = gizmoColor; // Para que el objeto también sea visible en juego, no solo en el editor

        if (particleMatForArea == null) GetComponent<ParticleSystemRenderer>().enabled = false;
        else GetComponent<ParticleSystemRenderer>().material.color = particleColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (continuous) InvokeRepeating("ApplyEffect", 0f, 1f);
            else ApplyEffect();
        }

        switch (effect)
        {
            case EffectType.HealthDamage: OnHealthDamageArea?.Invoke(); break;
            case EffectType.MadnessGain: OnMadnessGainArea?.Invoke(); break;
            case EffectType.OxygenLoss: OnOxygenLossArea?.Invoke(); break;
            case EffectType.SanityGain: OnSanityGainArea?.Invoke(); break;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) RemoveEffect();
        DamageZone.OnOutOfArea?.Invoke();

    }

    public void RemoveEffect()
    {
        CancelInvoke("ApplyEffect");
        Debug.Log("Efecto removido");
        
        
    }

    void ApplyEffect()
    {
        if (playerStatus == null) return;
        switch (effect)
        {
            case EffectType.HealthDamage: 
                playerStatus.ModifyHealth(-amountPerSecond); 
                break;
            case EffectType.OxygenLoss: 
                playerStatus.ModifyOxygen(-amountPerSecond); 
                break;
            case EffectType.MadnessGain: 
                playerStatus.ModifyMadness(amountPerSecond); 
                break;
            case EffectType.SanityGain: 
                playerStatus.ModifyMadness(-amountPerSecond); 
                break;
        }
    }

    // Gizmos para visualizar la zona en el Editor
    void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        Collider zoneCollider = GetComponent<Collider>();
        if (zoneCollider == null) return;

        Gizmos.color = gizmoColor;

        // Guardamos la matriz original para no afectar a otros Gizmos
        Matrix4x4 oldMatrix = Gizmos.matrix;
        
        // Aplicamos la matriz de transformación del objeto (Posición, Rotación y Escala)
        Gizmos.matrix = transform.localToWorldMatrix;

        if (zoneCollider is BoxCollider box)
        {
            // Al usar localToWorldMatrix, box.center ya está en el lugar correcto
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (zoneCollider is SphereCollider sphere)
        {
            // Dibujamos usando coordenadas locales
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
        else if (zoneCollider is CapsuleCollider capsule)
        {
            // Para cápsulas, usamos el centro local
            Vector3 center = capsule.center;
            float halfH = (capsule.height / 2) - capsule.radius;

            Gizmos.DrawWireSphere(center, capsule.radius);
            Gizmos.DrawWireSphere(center + Vector3.up * halfH, capsule.radius);
            Gizmos.DrawWireSphere(center - Vector3.up * halfH, capsule.radius);
        }

        // Restauramos la matriz
        Gizmos.matrix = oldMatrix;

        // GUI de texto (esta sí requiere coordenadas de mundo para WorldToGUIPoint)
        #if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();
        // transform.position sigue siendo correcto aquí porque Handles.BeginGUI no usa la Gizmos.matrix
        Vector3 screenPos = UnityEditor.HandleUtility.WorldToGUIPoint(transform.position);
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = gizmoColor;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        
        GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 30, 200, 20), $"{effect}: {amountPerSecond}/s", style);
        UnityEditor.Handles.EndGUI();
        #endif
    }
}