using UnityEngine;
using UnityEngine.UI;

// Script creado por Danna. Contanctar con ella antes de hacer cambios significativos para evitar conflictos.
public class StatusBarUI : MonoBehaviour
{
    public PlayerStatus playerStatus;
    public Slider healthSlider;
    public Slider oxygenSlider;
    public Slider madnessSlider;

    void OnEnable()
    {
        playerStatus.OnHealthChanged.AddListener(UpdateHealthUI);
        playerStatus.OnOxygenChanged.AddListener(UpdateOxygenUI);
        playerStatus.OnMadnessChanged.AddListener(UpdateMadnessUI);
    }

    void OnDisable()
    {
        playerStatus.OnHealthChanged.RemoveListener(UpdateHealthUI);
        playerStatus.OnOxygenChanged.RemoveListener(UpdateOxygenUI);
        playerStatus.OnMadnessChanged.RemoveListener(UpdateMadnessUI);
    }

    void UpdateHealthUI(float current, float max) => healthSlider.value = current / max;
    void UpdateOxygenUI(float current, float max) => oxygenSlider.value = current / max;
    void UpdateMadnessUI(float current, float max) => madnessSlider.value = current / max;
}