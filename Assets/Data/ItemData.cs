// Fausto A. Gómez
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventario/Item Data")]
public class ItemData : ScriptableObject
{
    public enum ItemType { Consumable, Tool, Key, Flashlight }

    [Header("Identificación")]
    public string itemName;
    public ItemType type;

    [Header("Visuales")]
    public Sprite icon;
    public GameObject dropPrefab;

    [Header("Lógica")]
    public float value; // Ej: Salud a recuperar o batería
    public string interactionID; // ID para puertas (Ej: "Llave_Sotano")
}