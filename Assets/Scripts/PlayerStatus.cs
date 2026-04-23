using UnityEngine;
using UnityEngine.Events;

// Script creado por Danna. Contanctar con ella antes de hacer cambios significativos para evitar conflictos.
public class PlayerStatus : MonoBehaviour
{
    public PlayerStatusData playerStatusData;
    
    public UnityEvent<float, float> OnHealthChanged;   // (current, max)
    public UnityEvent<float, float> OnOxygenChanged;
    public UnityEvent<float, float> OnMadnessChanged;
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnMadnessSpawnTreesholdReached;       // al superar 70
    public UnityEvent OnMadnessMaxReached;             // al llegar a 100

    private float currentHealth;
    private float currentOxygen;
    private float currentMadness;
    public bool isAlive = true;
    private bool thresholdEventSent = false;

    void Start()
    {
        currentHealth = playerStatusData.maxHealth;
        currentOxygen = playerStatusData.maxOxygen;
        currentMadness = 0f;
        InvokeRepeating("TickStatus", playerStatusData.tickSpeed, playerStatusData.tickSpeed);
    }

    void TickStatus()
    {
        if (!isAlive) return;

        // Oxígeno decae con el tiempo
        ModifyOxygen(-playerStatusData.oxygenDecayRate);

        // Locura aumenta con el tiempo
        ModifyMadness(playerStatusData.madnessIncreaseRate);

        // Regeneración de salud si oxígeno alto y locura baja
        if (currentOxygen > playerStatusData.oxygenHighTreeshold && currentMadness < playerStatusData.madnessLowTreeshold)
            ModifyHealth(playerStatusData.healthRegenRate);
    }

    public void ModifyHealth(float amount)
    {
        if (!isAlive) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, playerStatusData.maxHealth);
        OnHealthChanged?.Invoke(currentHealth, playerStatusData.maxHealth);
        if (currentHealth <= 0) Die();
    }

    public void ModifyOxygen(float amount)
    {
        if (!isAlive) return;
        currentOxygen = Mathf.Clamp(currentOxygen + amount, 0, playerStatusData.maxOxygen);
        OnOxygenChanged?.Invoke(currentOxygen, playerStatusData.maxOxygen);
        if (currentOxygen <= 0) ModifyHealth(-5f); // Daño por falta de oxígeno (ajustable)
    }

    public void ModifyMadness(float amount)
    {
        if (!isAlive) return;
        currentMadness = Mathf.Clamp(currentMadness + amount, 0, playerStatusData.maxMadness);
        OnMadnessChanged?.Invoke(currentMadness, playerStatusData.maxMadness);

        // Umbral 70
        if (currentMadness >= playerStatusData.madnessSpawnsEnemiesThreshold && !thresholdEventSent)
        {
            thresholdEventSent = true;
            OnMadnessSpawnTreesholdReached?.Invoke();
        }
        else if (currentMadness < playerStatusData.madnessSpawnsEnemiesThreshold)
        {
            thresholdEventSent = false;
        }

        // Locura máxima: pérdida rápida de oxígeno
        if (currentMadness >= playerStatusData.maxMadness)
        {
            OnMadnessMaxReached?.Invoke();
            ModifyOxygen(-playerStatusData.madnessDamageOxygenRate);
        }
    }

    void Die()
    {
        isAlive = false;
        OnPlayerDeath?.Invoke();
        Debug.Log("Player died");
    }

        #region Métodos de prueba para el Editor
    [ContextMenu("Test/Recibir 10 de daño")]
    void TestTakeDamage() => ModifyHealth(-10f);

    [ContextMenu("Test/Curar 20 de salud")]
    void TestHeal() => ModifyHealth(20f);

    [ContextMenu("Test/Perder 15 de oxígeno")]
    void TestLoseOxygen() => ModifyOxygen(-15f);

    [ContextMenu("Test/Ganar 10 de oxígeno")]
    void TestGainOxygen() => ModifyOxygen(10f);

    [ContextMenu("Test/Aumentar 15 de locura")]
    void TestGainMadness() => ModifyMadness(15f);

    [ContextMenu("Test/Reducir 20 de locura")]
    void TestReduceMadness() => ModifyMadness(-20f);

    [ContextMenu("Test/Llevar locura a 75 (umbral)")]
    void TestMadnessThreshold()
    {
        currentMadness = playerStatusData.madnessSpawnsEnemiesThreshold;
        OnMadnessChanged?.Invoke(currentMadness, playerStatusData.maxMadness);
        if (currentMadness >= playerStatusData.madnessSpawnsEnemiesThreshold && !thresholdEventSent)
        {
            thresholdEventSent = true;
            OnMadnessSpawnTreesholdReached?.Invoke();
        }
    }

    [ContextMenu("Test/Llevar locura a 100")]
    void TestMaxMadness() => ModifyMadness(100f);

    [ContextMenu("Test/Matar jugador")]
    void TestKillPlayer() => Die();

    [ContextMenu("Test/Resetear estado")]
    void TestResetState()
    {
        currentHealth = playerStatusData.maxHealth;
        currentOxygen = playerStatusData.maxOxygen;
        currentMadness = 0f;
        isAlive = true;
        thresholdEventSent = false;
        OnHealthChanged?.Invoke(currentHealth, playerStatusData.maxHealth);
        OnOxygenChanged?.Invoke(currentOxygen, playerStatusData.maxOxygen);
        OnMadnessChanged?.Invoke(currentMadness, playerStatusData.maxMadness);
        Debug.Log("Estado del jugador resetearo");
    }
    #endregion
}