//Fauto A. Gomez
using UnityEngine;

public class ItemCollectible : MonoBehaviour
{
    [Header("Datos del Ítem")]
    public ItemData itemData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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
}