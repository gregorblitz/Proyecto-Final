using UnityEngine;

// Script creado por Danna. Contanctar con ella antes de hacer cambios significativos para evitar conflictos.
[CreateAssetMenu(fileName = "PlayerStatusData", menuName = "Scriptable Objects/PlayerStatusData")]
public class PlayerStatusData : ScriptableObject
{
    [Header("Valores iniciales")]
    public float maxHealth = 100f;
    public float maxOxygen = 100f;
    public float maxMadness = 100f;
    public float tickSpeed = 1f;

    [Header("Regeneración")]
    public float healthRegenRate = 5f;      // por segundo si oxígeno>80 y locura<20
    public float oxygenDecayRate = 2f;      // pérdida base por segundo
    public float madnessIncreaseRate = 1.5f; // ganancia base por segundo
    public float madnessDamageOxygenRate = 10f; 

    [Header("Umbrales")]
    public float healthLowTreeshold = 20f; // oxígeno perdido por segundo si locura=100
    public float madnessLowTreeshold = 20f;     
    public float oxygenLowTreeshold = 20f;
    public float oxygenHighTreeshold = 80f; 
    public float madnessSpawnsEnemiesThreshold = 70f;    
    
    
    [Header("Efectos de zona")]
    public float damageZoneHealthLoss = 15f;     // por segundo
    public float oxygenZoneLoss = 20f;           // por segundo
    public float madnessZoneGain = 25f;          // por segundo
    public float sanityZoneLoss = 20f;           // zona que reduce locura
}
