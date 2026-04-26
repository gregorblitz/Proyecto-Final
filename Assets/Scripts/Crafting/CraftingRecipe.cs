using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Nueva Receta", menuName = "Inventario/Receta de Crafteo")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Info de la Receta")]
    public string recipeName = "Nueva Receta";
    
    [Header("Ingredientes necesarios")]
    public List<ItemData> ingredients; // Los objetos que se deben combinar

    [Header("Resultado")]
    public ItemData resultItem; // Lo que da al combinarlos

    [Header("Visual de Inventario")]
    public Color glowColor = Color.cyan; // Color del borde cuando se pueden combinar
}