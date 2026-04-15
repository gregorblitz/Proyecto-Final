using UnityEngine;

// Script creado por Danna. Contanctar con ella antes de hacer cambios significativos para evitar conflictos.
public class DamageZone : MonoBehaviour
{
    public enum EffectType { HealthDamage, OxygenLoss, MadnessGain, SanityGain }
    public EffectType effect;
    public float amountPerSecond = 10f;
    public bool continuous = true;
    
    [Header("Gizmos Visualization")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);
    public bool showGizmo = true;

    private PlayerStatus playerStatus;
    private Collider zoneCollider;

    void Start()
    {
        playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatus>();
        zoneCollider = GetComponent<Collider>();
        if (zoneCollider == null)
            Debug.LogWarning("DamageZone necesita un Collider (preferiblemente Trigger)");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (continuous) InvokeRepeating("ApplyEffect", 0f, 1f);
            else ApplyEffect();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && continuous) 
            CancelInvoke("ApplyEffect");
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