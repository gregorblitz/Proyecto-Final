// Fausto A. Gómez
using UnityEngine;
using UnityEngine.Events;

// Controla la lógica de la linterna: carga, encendido y apagado
public class FlashlightController : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject lightObject;         // El objeto Light dentro de la linterna
    public float maxBattery = 100f;        // Carga máxima
    public float drainRate = 5f;           // Carga gastada por segundo mientras está encendida

    [Header("Estado")]
    public float currentBattery = 0f;      // Empieza sin batería hasta que se equipa
    public bool isOn = false;
    public bool hasFlashlight = false;     // ¿El jugador tiene la linterna en inventario?

    [Header("Eventos (para la UI)")]
    public UnityEvent<float, float> OnBatteryChanged;   // (current, max)
    public UnityEvent<bool> OnFlashlightToggled;        // (isOn)

    private void Start()
    {
        // Aseguramos que empieza apagada
        if (lightObject != null)
            lightObject.SetActive(false);
    }

    private void Update()
    {
        if (isOn && hasFlashlight)
        {
            // Gastar batería cada frame
            currentBattery -= drainRate * Time.deltaTime;
            currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

            // Avisar a la UI
            OnBatteryChanged?.Invoke(currentBattery, maxBattery);

            // Si se acaba la batería, apagar sola
            if (currentBattery <= 0f)
            {
                TurnOff();
                Debug.Log("Linterna apagada: batería agotada");
            }
        }
    }

    // Llamado desde SystemsController cuando el jugador presiona F
    public void TryToggle()
    {
        if (!hasFlashlight)
        {
            Debug.Log("No tienes linterna en el inventario");
            return;
        }

        if (currentBattery <= 0f)
        {
            Debug.Log("La linterna no tiene batería");
            return;
        }

        if (isOn)
            TurnOff();
        else
            TurnOn();
    }

    private void TurnOn()
    {
        isOn = true;
        if (lightObject != null) lightObject.SetActive(true);
        OnFlashlightToggled?.Invoke(true);
        Debug.Log("Linterna encendida");
    }

    private void TurnOff()
    {
        isOn = false;
        if (lightObject != null) lightObject.SetActive(false);
        OnFlashlightToggled?.Invoke(false);
        Debug.Log("Linterna apagada");
    }

    // Llamado cuando el jugador equipa una linterna desde el inventario
    public void EquipFlashlight(ItemData item)
    {
        if (item.type != ItemData.ItemType.Flashlight) return;

        // Solucion bug bateria infinita al seleccionar de nuevo en inventario
        // Absorbe batería del obj si es primera vez que se equipa
        if (!hasFlashlight)
        {
            hasFlashlight = true;
            currentBattery = item.batteryCapacity;  // La carga que trae
            maxBattery = item.batteryCapacity > 0 ? item.batteryCapacity : maxBattery;
        }
        // Siempre actualiza la UI al darle clic para que la pantalla refresque el valor real
        OnBatteryChanged?.Invoke(currentBattery, maxBattery);
        Debug.Log($"Linterna equipada. Batería restante: {currentBattery}");
    }

    // Llamado cuando se usa una batería (crafteo o uso directo)
    public void RechargeBattery(float amount)
    {
        currentBattery = Mathf.Clamp(currentBattery + amount, 0f, maxBattery);
        OnBatteryChanged?.Invoke(currentBattery, maxBattery);
        Debug.Log($"Batería recargada. Actual: {currentBattery}");
    }
}