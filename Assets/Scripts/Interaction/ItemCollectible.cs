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
            // Intentamos añadirlo al Manager. Si hay éxito (true), destruimos el objeto de la escena.
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
        Destroy(gameObject);
    }
    

    IEnumerator waitForCollectibleOnDroped()
    {
        canBeCollected = false; // Desactivamos el collider para evitar que el jugador lo recoja inmediatamente al soltarlo
        yield return new WaitForSeconds(0.1f);
        canBeCollected = true; 
    }
}