// Fausto A. Gómez
using UnityEngine;
[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventario/Item Data")]
public class ItemData : ScriptableObject
{
    public enum ItemType { Consumable, Tool, Key, Flashlight, Battery }
    [Header("Identificación")]
    public string itemName;
    public ItemType type;

    [Header("Visuales")]
    public Sprite icon;
    public GameObject dropPrefab;

    [Header("Descripción")]
    [TextArea(5, 10)]
    public string description;

    [Header("Propiedades")]
    public bool isConsumable;

    public enum ItemEffect { None, Heal, RestoreOxygen, ReduceMadness }
    [Header("Efectos")]
    public ItemEffect effectType;
    public float effectAmount;

    [Header("Lógica")]
    public float value;
    public string interactionID;

    // NUEVO: para la linterna y batería
    [Header("Linterna / Batería")]
    public float batteryCapacity = 100f; // cuánta carga tiene este ítem
}