//Fauto A. Gomez
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class ItemCollectible : MonoBehaviour
{
    [Header("Datos del Ítem")]
    public ItemData itemData;
    private bool canBeCollected;
    private bool doCollect;

    private InputAction collectAction;
    [Header("Pasar a PlayerController")]
    public InputActionAsset inputActions;


    private void Awake() {
        collectAction = inputActions.FindActionMap("Player").FindAction("Interact");
    }

    private void OnEnable() {
        StartCoroutine(waitForCollectibleOnDroped());
    }

    private void Update() {

        
    }

    private void OnTriggerStay(Collider other)
    {
        

        if (other.CompareTag("Player") && canBeCollected && collectAction.IsPressed())
        {
            Debug.Log($"Intentando recoger: {itemData.itemName}");
            // Intentamos añadirlo al Manager. Si hay éxito (true), destruimos el objeto de la escena.
            if (InventoryManager.Instance.AddItem(itemData))
            {
                Debug.Log($"Recogido con éxito.");
                DestroyCollectible();
            }
        }
    }

    void DestroyCollectible()
    {
        Destroy(gameObject);
    }
    


    IEnumerator waitForCollectibleOnDroped()
    {
        canBeCollected = false; // Desactivamos el collider para evitar que el jugador lo recoja inmediatamente al soltarlo
        yield return new WaitForSeconds(0.1f);
        canBeCollected = true; 
    }
}