//Fauto A. Gomez
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventario/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identificación")]
    [Tooltip("El nombre que se mostrará en el inventario.")]
    public string itemName;
    
    [Header("Visuales y Prefabs")]
    [Tooltip("El icono que se mostrará en el slot de la UI.")]
    public Sprite icon;
    
    [Tooltip("El modelo 3D con físicas que se instanciará al tirar el objeto.")]
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
}