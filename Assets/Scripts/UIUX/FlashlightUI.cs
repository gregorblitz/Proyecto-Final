// Fausto A. Gómez
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Muestra en pantalla el estado de la linterna (encendida/apagada y batería)
public class FlashlightUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject flashlightHUD;       // Panel completo (se oculta si no tiene linterna)
    public Slider batterySlider;           // Barra de batería
    public Image statusIndicator;          // Bolita o ícono de encendido
    public TextMeshProUGUI statusText;     // "ON" / "OFF"

    [Header("Colores")]
    public Color colorOn = Color.green;
    public Color colorOff = Color.red;

    [Header("Referencia")]
    public FlashlightController flashlightController;

    private void Start()
    {
        // Empieza oculto hasta que el jugador consiga la linterna
        if (flashlightHUD != null)
            flashlightHUD.SetActive(false);

        if (flashlightController == null)
        {
            flashlightController = GameObject.FindWithTag("Player")
                                             .GetComponentInChildren<FlashlightController>();
        }

        if (flashlightController != null)
        {
            flashlightController.OnBatteryChanged.AddListener(UpdateBatteryUI);
            flashlightController.OnFlashlightToggled.AddListener(UpdateStatusUI);
        }
        else
        {
            Debug.LogWarning("FlashlightUI no encontró FlashlightController");
        }
    }

    // Cuando el jugador consigue la linterna, mostrar la UI
    public void ShowFlashlightHUD()
    {
        if (flashlightHUD != null)
            flashlightHUD.SetActive(true);
    }

    private void UpdateBatteryUI(float current, float max)
    {
        if (batterySlider != null)
            batterySlider.value = current / max;
    }

    private void UpdateStatusUI(bool isOn)
    {
        if (statusIndicator != null)
            statusIndicator.color = isOn ? colorOn : colorOff;

        if (statusText != null)
            statusText.text = isOn ? "ON" : "OFF";
    }
}