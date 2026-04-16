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

    [Header("Descripción")]
    [TextArea(5, 10)]
    public string description;

    [Header("Propiedades")]
    public bool isConsumable;
    // Aquí podrías añadir un valor si es consumible, ej: cantidad de curación

    // En tu script ItemData.cs existente, añade:
    public enum ItemEffect { None, Heal, RestoreOxygen, ReduceMadness }
    [Header("Efectos")]
    public ItemEffect effectType;
    public float effectAmount;
    [Header("Lógica")]
    public float value; // Ej: Salud a recuperar o batería
    public string interactionID; // ID para puertas (Ej: "Llave_Sotano")
}