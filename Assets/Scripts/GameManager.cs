using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private GameObject playerRef;

    [Header("Game Over Config")]
    [SerializeField] private float totalSlowMoLenght = 2.0f;
    [SerializeField] private float delayBeforeRestar = 3.0f;
    public GameObject jumpscareUI;
    public GameOverUI gameOverUI;

    [Header("Victory Config")]
    [SerializeField] protected Collider victoryTrigger;

    [Header("Checkpoint Config")]
    [SerializeField] protected static GameObject currentCheckpoint;


    // ****CHECKPOINTS Y PERSISTENCIA****
    // ==========================================
    // El carrete de fotos (memoria estatica)
    private static float savedHealth;
    private static float savedOxygen;
    private static float savedMadness;
    private static float savedBattery;
    private static bool savedHasFlashlight;
    private static ItemData savedSelectedItem;
    private static List<ItemData> savedInventory = new List<ItemData>();
    private static bool savedIsFlashlightOn;//Guarda estado de lampara (on/off)

    private void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");

        if (playerRef != null)
            playerRef.GetComponent<PlayerStatus>().OnPlayerDeath.AddListener(OnGameOver);
        Debug.Log(gameOverUI + " is cool");
    }

    void OnGameOver()
    {
        StartCoroutine(slowTimeToStop());
    }

    IEnumerator slowTimeToStop()
    {
        float slowMoTimer = 0f;

        while (slowMoTimer < totalSlowMoLenght)
        {
            slowMoTimer += Time.unscaledDeltaTime;

            float t = Mathf.InverseLerp(0f, totalSlowMoLenght, slowMoTimer);
            float alpha = Mathf.Lerp(1f, 0f, t);

            Time.timeScale = alpha;
            yield return null;
        }
        // Apaga al jugador para que el mundo deje de interactuar con el
        //playerRef.GetComponent<PlayerStatus>().gameObject.SetActive(false);
        playerRef.SetActive(false);
        

        // 🔽 CAMBIO CLAVE AQUÍ 🔽
        Time.timeScale = 0f;
        if (jumpscareUI.activeInHierarchy) jumpscareUI.SetActive(false);
        //muestra menu de reintentar
        gameOverUI.MostrarGameOver();
        // 🔼 FIN DEL CAMBIO 🔼

    }

    public static void OnGameVictory()
    {
        Debug.Log("You won!");
    }

    // TOMA FOTO (al entrar al campamento)
    // =====================================
    // se editan metodos para la persistencia y se comentan como estaban
    /*
    public static void SetNewCheckpoint(GameObject newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;
    }

    [ContextMenu("Llevar jugador a checkpoint")]
    public static void TakePlayerToCheckpoint()
    {
        GameObject.FindWithTag("Player").transform.position = currentCheckpoint.transform.position + Vector3.up * 2;
    }
    */
    // Entran las estats del jugador
    public static void SetNewCheckpoint(GameObject newCheckpoint, PlayerStatus status)
    {
        currentCheckpoint = newCheckpoint;
        GameObject player = status.gameObject;

        // Foto de las Stats Vitales, llama script playerstatus
        status.GetCurrentStats(out savedHealth, out savedOxygen, out savedMadness);

        // Foto de la Mochila (COPIA de la lista) llama script InventoryManager
        savedInventory = new List<ItemData>(InventoryManager.Instance.currentItems);

        // Foto de la Linterna (llama script FlashlightController)
        FlashlightController fc = player.GetComponentInChildren<FlashlightController>();
        if (fc != null)
        {
            savedBattery = fc.currentBattery; //la carga que tiene
            savedHasFlashlight = fc.hasFlashlight; //tiene la linterna?
            savedIsFlashlightOn = fc.isOn; // Guarda si estaba prendida
        }

        // Foto de las Manos (llama script SystemsController)
        SystemsController sc = player.GetComponent<SystemsController>();
        if (sc != null) savedSelectedItem = sc.selectedItem;

        Debug.Log("Checkpoint Guardado: Stats, Mochila y Linterna");
    }

    // RESTAURA LA FOTO (Al revivir)
    public void TakePlayerToCheckpoint()
    {
        if (currentCheckpoint == null) 
        {
            Debug.LogWarning("No hay checkpoint. Reaparece al inicio del juego");
            return;
        }

        // Ocultar la pantalla de muerte al revivir
        if (gameOverUI != null) 
        {
            gameOverUI.gameObject.SetActive(false);
        }

        // Restaurar stats vitales
        //=========================
        PlayerStatus status = playerRef.GetComponent<PlayerStatus>();
        // envia los datos que se copiaron cuando entro al checkpoint
        status.RestoreStatsFromCheckpoint(savedHealth, savedOxygen, savedMadness);

        // Restaurar Mochila
        //====================
        // InventoryManager.Instance permitem llamarlo desde cualquier script
        // RestoreInventory metodo del script inventoryManager para restaurar el inventario guardado
        InventoryManager.Instance.RestoreInventory(savedInventory);

        // Restaura variables guardadas Linterna y UI de bateria
        FlashlightController fc = playerRef.GetComponentInChildren<FlashlightController>();
        if (fc != null)
        {
            fc.currentBattery = savedBattery;
            fc.hasFlashlight = savedHasFlashlight;
            fc.isOn = savedIsFlashlightOn; // Restaura el interruptor

            // Aplica el cambio visualmente al foco de luz
            if(fc.lightObject != null) fc.lightObject.SetActive(fc.isOn);
            fc.OnBatteryChanged?.Invoke(fc.currentBattery, fc.maxBattery);
            fc.OnFlashlightToggled?.Invoke(fc.isOn);
        }  

        // Restaura lo que tenia en las manos (independencia entre izq y der)
        SystemsController sc = playerRef.GetComponent<SystemsController>();
        PlayerInteractor interactor = playerRef.GetComponent<PlayerInteractor>();
        
        if (interactor != null) interactor.DesequiparTodo(); // Limpia manos 

        if (sc != null && interactor != null)
        {
            sc.selectedItem = savedSelectedItem; // restaura la memoria del objeto al llegar al checkpoint
  
            // Re-equipa visualmente si tenia algo
            if (savedSelectedItem != null && (savedSelectedItem.itemName == "Pica" || savedSelectedItem.itemName == "Pico" || savedSelectedItem.itemName == "Pickaxe"))
            {
                interactor.EquipPickaxe();   //Aparece pico si lo tenia en checkpoint
            }
            if (savedHasFlashlight)
            {
                interactor.EquipFlashlight(); //Aparece lampara si la tenia en checkpoint
            }
        }

        // Teletransporte Fisico Seguro
        
        Rigidbody rb = playerRef.GetComponent<Rigidbody>();
        PlayerController pc = playerRef.GetComponent<PlayerController>();

        if (pc != null) pc.enabled = false; // Evita que el script de movimiento interfiera

        if (rb != null)
        {
            rb.isKinematic = true; 
            Vector3 targetPos = currentCheckpoint.transform.position + Vector3.up * 2;
            
            playerRef.transform.position = targetPos;
            rb.position = targetPos;
            
            Physics.SyncTransforms(); // Fuerza a Unity a reconocer la nueva posicion

            rb.isKinematic = false; 
            rb.linearVelocity = Vector3.zero; // Limpia la inercia de la muerte
        }

        // Reactivacion total
        playerRef.SetActive(true); 
        Time.timeScale = 1f; 
        if (pc != null) pc.enabled = true;

        // *****ARREGLO BLANCO Y NEGRO*****
        // Busca el controlador de efectos de la pantalla
        ScreenEffectsController effectsController = FindFirstObjectByType<ScreenEffectsController>();
        if (effectsController != null)
        {
            // Lo vuelve a encender
            effectsController.enabled = true;
            
            // Apaga el estado de muerte a la fuerza
            if (effectsController.volumeForDeath != null)
            {
                effectsController.volumeForDeath.enabled = false;
                effectsController.volumeForDeath.weight = 0f;
            }

            // Enciende el volumen normal (estado neutral/seguro)
            if (effectsController.volumeBase != null)
            {
                effectsController.volumeBase.enabled = true;
                effectsController.volumeBase.weight = 1f;
            }
        }

        Debug.Log("Jugador restaurado al 100% como estaba en el checkpoint");
    }
        
}