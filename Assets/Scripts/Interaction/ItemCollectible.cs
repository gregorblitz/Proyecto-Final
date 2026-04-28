//Fauto A. Gomez
using UnityEngine;
using System.Collections;

public class ItemCollectible : MonoBehaviour
{
    [Header("Datos del Ítem")]
    public ItemData itemData;

    private bool canBeCollected;
    private bool doCollect;
    private PlayerInteractor playerInteractor;

    private void OnEnable() {
        StartCoroutine(waitForCollectibleOnDroped());
    }

    private void Start() {
        playerInteractor = GameObject.FindWithTag("Player").GetComponent<PlayerInteractor>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && canBeCollected && playerInteractor.doCollect)
        {
            Debug.Log($"Intentando recoger: {itemData.itemName}");

            // Intentamos añadirlo al inventario
            if (InventoryManager.Instance.AddItem(itemData))
            {
                Debug.Log($"Recogido con éxito.");
                // LE AVISA AL PLAYERCONTROLLER QUE HAGA LA ANIMACION
                PlayerController pController = other.GetComponent<PlayerController>();
                if (pController != null)
                {
                    pController.PlayPickupAnimation(); // Llama la funcion del PlayerController
                }
                // --------------------------------------------------------------------
                DestroyCollectible();
            }

            playerInteractor.doCollect = false;
        }
    }

    void DestroyCollectible()
    {
        // NUEVA LÍNEA: dispara el evento de sonido al recoger el ítem
        SFXEvents.instance?.OnItemPickup();

        // Eliminamos el objeto de la escena
        Destroy(gameObject);
    }
    
    IEnumerator waitForCollectibleOnDroped()
    {
        // Evita recoger el objeto inmediatamente después de soltarlo
        canBeCollected = false;

        yield return new WaitForSeconds(0.1f);

        canBeCollected = true; 
    }
}