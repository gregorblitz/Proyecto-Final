using UnityEngine;
using System.Collections.Generic;

//Script que evaluara el inventario

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [Header("Base de Datos de Recetas")]
    public List<CraftingRecipe> allRecipes;

    // Memoria del primer objeto seleccionado con la rueda del ratón
    [HideInInspector] public InventorySlotUI slotSeleccionadoParaFusion = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Revisa todos los slots y enciende brillos si hay coincidencias
    public void EvaluateCraftableItemsGlow(InventorySlotUI[] currentSlots)
    {
        // Limpia la memoria si se cierra el inventario a mitad de un crafteo
        slotSeleccionadoParaFusion = null;

        // Apaga brillos por defecto por seguridad
        foreach (InventorySlotUI slot in currentSlots)
        {
            if (slot != null)
            {
                slot.SetGlow(Color.white, false);
                slot.isCraftableReady = false;
            }
        }

        // Evalua receta por receta
        foreach (CraftingRecipe recipe in allRecipes)
        {
            // Busca # de ingredientes que hay de la receta en el inventario
            List<InventorySlotUI> slotsToGlow = new List<InventorySlotUI>();
            int ingredientsFound = 0;

            // Revisa cada ingrediente que pide la receta
            foreach (ItemData requiredIngredient in recipe.ingredients)
            {
                // Busca si algun slot tiene este ingrediente
                foreach (InventorySlotUI slot in currentSlots)
                {
                    // Si el slot tiene el ingrediente y no lo hemos marcado para brillar
                    if (slot.currentItem != null && slot.currentItem == requiredIngredient && !slotsToGlow.Contains(slot))
                    {
                        slotsToGlow.Add(slot);
                        ingredientsFound++;
                        break; // se encontro uno de este tipo, pasa al siguiente ingrediente
                    }
                }
            }

            // Si encuentras todos los ingredientes de la receta en el inventario
            // (SI SE QUIERE QUE brillen con solo tener 2 compatibles, cambiaR la condición)
            if (ingredientsFound >= recipe.ingredients.Count && recipe.ingredients.Count > 1)
            {
                // Sale en consola si la receta se cumple
                Debug.Log($"Receta detectada en el inventario : {recipe.recipeName}");
                // Enciende el brillo del color de la receta en esos slots especificos
                foreach (InventorySlotUI slot in slotsToGlow)
                {
                    slot.SetGlow(recipe.glowColor, true);
                    slot.isCraftableReady = true; // Activa la bandera para el mouse
                }
            }
        }
    }

    // *****FUSIÓN (CLICK RUEDA MOUSE)*****
    public void HandleMiddleClick(InventorySlotUI clickedSlot)
    {
        if (clickedSlot.currentItem == null) return;

        // No habia seleccionado nada aun
        if (slotSeleccionadoParaFusion == null)
        {
            // objeto brillando? (su pareja esta en inventario)
            if (clickedSlot.isCraftableReady)
            {
                slotSeleccionadoParaFusion = clickedSlot;
                
                // Efecto visual: Lo pone Blanco puro para indicar que lo selecciono
                slotSeleccionadoParaFusion.SetGlow(Color.white, true);
                Debug.Log($"[{clickedSlot.currentItem.itemName}] seleccionado. Selecciona su pareja...");
            }
            else
            {
                // SE EQUIPA Si no tiene pareja o no es crafteable
                clickedSlot.UseThisItem();
            }
        }
        // Ya habia uno seleccionado y se hace clic en el segundo
        else
        {
            // Si jugador se arrepiente y oprime el mismo objeto, cancela la seleccion
            if (clickedSlot == slotSeleccionadoParaFusion)
            {
                ReevaluarInventario(clickedSlot); // Restaura los colores originales
                return;
            }

            // Verifica si los dos objetos juntos forman una receta
            CraftingRecipe recetaEncontrada = ObtenerRecetaDePar(slotSeleccionadoParaFusion.currentItem, clickedSlot.currentItem);

            if (recetaEncontrada != null)
            {
                // COMPATIBLES! FUSION!!
                RealizarFusion(recetaEncontrada, slotSeleccionadoParaFusion, clickedSlot);
            }
            else
            {
                // Toca un objeto incorrecto. Cancela seleccion y equipa el nuevo.
                ReevaluarInventario(clickedSlot);
                clickedSlot.UseThisItem();
            }
        }
    }

    private CraftingRecipe ObtenerRecetaDePar(ItemData item1, ItemData item2)
    {
        foreach (CraftingRecipe recipe in allRecipes)
        {
            if (recipe.ingredients.Count == 2)
            {
                // Chequea si el Item1 y Item2 son exactamente los dos ingredientes de la receta
                if ((recipe.ingredients[0] == item1 && recipe.ingredients[1] == item2) ||
                    (recipe.ingredients[0] == item2 && recipe.ingredients[1] == item1))
                {
                    return recipe;
                }
            }
        }
        return null;
    }

    private void RealizarFusion(CraftingRecipe receta, InventorySlotUI slot1, InventorySlotUI slot2)
    {
        // Borra los ingredientes viejos de la mochila logicamente
        InventoryManager.Instance.RemoveItem(slot1.currentItem);
        InventoryManager.Instance.RemoveItem(slot2.currentItem);

        // Vacia las imagenes de los dos cuadritos
        slot1.ClearSlot();
        slot2.ClearSlot();

        // Añade el resultado de la receta
        // InventoryManager pone el nuevo objeto en el slot que esta mas arriba de los 2
        InventoryManager.Instance.AddItem(receta.resultItem);

        // Limpia la memoria y restaura luces de la mochila
        ReevaluarInventario(slot1);

        Debug.Log($"Fusión Exitosa, se ha creado: {receta.resultItem.itemName}");
    }

    // Vuelve a revisar luces sin tener que abrir/cerrar inventario
    private void ReevaluarInventario(InventorySlotUI unSlotCualquiera)
    {
        slotSeleccionadoParaFusion = null;
        InventorySlotUI[] allSlots = unSlotCualquiera.transform.parent.GetComponentsInChildren<InventorySlotUI>(true);
        EvaluateCraftableItemsGlow(allSlots);
    }
}