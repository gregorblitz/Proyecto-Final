// Fausto A. Gómez
// Panel de crafteo que vive DENTRO del panel de inventario.
// Se abre con un botón (clic de mouse) que aparece en la UI del inventario.
// Muestra las recetas disponibles, las piezas necesarias y un botón para craftear.
//
// CÓMO USARLO EN UNITY:
//  1. Crea un panel hijo dentro del InventoryPanel llamado "CraftingPanel".
//  2. Dentro de CraftingPanel pon:
//     - Un objeto "RecipeContainer" con Vertical Layout Group (aquí se generan los slots de receta).
//     - Un botón "BotonCraftear" que el jugador clic para craftear.
//     - Un Text o TMP_Text "TextoFeedback" para mensajes de éxito/error.
//  3. Arrastra los refs al Inspector de este script.
//  4. Arrastra el botón de abrir crafteo (por ej. dentro del HUD del inventario) al campo "botonAbrirCrafteo".

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CraftingUI : MonoBehaviour
{
    [Header("Paneles")]
    // El panel raíz del crafteo (hijo del panel de inventario)
    public GameObject craftingPanel;

    [Header("Contenedor de Recetas")]
    // Aquí se crean dinámicamente las tarjetas de receta
    public Transform recipeContainer;
    // Prefab de una tarjeta de receta (ver instrucciones abajo)
    public GameObject recipePrefab;

    [Header("Botón de Craftear")]
    // Botón que ejecuta el crafteo de la receta seleccionada
    public Button botonCraftear;

    [Header("Feedback")]
    // Texto que muestra "¡Éxito!" o "Te falta: X"
    public TMP_Text textoFeedback;

    [Header("Botón Abrir/Cerrar Crafteo")]
    // Botón que el jugador usa para cambiar entre inventario y crafteo (clic de mouse)
    public Button botonAbrirCrafteo;

    // ─── PRIVADOS ─────────────────────────────────────────────────────────────
    private CraftingSystem craftingSystem;
    private int recetaSeleccionada = -1;     // Índice de la receta que el jugador eligió
    private bool craftingAbierto = false;

    // Colores para resaltar la receta seleccionada en la lista
    private readonly Color colorSeleccionado = new Color(1f, 0.85f, 0.2f); // amarillo
    private readonly Color colorNormal = Color.white;

    // ─── INICIO ───────────────────────────────────────────────────────────────
    private void Start()
    {
        craftingSystem = FindFirstObjectByType<CraftingSystem>();

        if (craftingSystem == null)
        {
            Debug.LogError("[CraftingUI] No se encontró CraftingSystem en la escena.");
            return;
        }

        // El panel de crafteo empieza oculto
        if (craftingPanel != null)
            craftingPanel.SetActive(false);

        // Asignamos el listener del botón principal de craftear
        if (botonCraftear != null)
            botonCraftear.onClick.AddListener(OnBotonCraftearClick);

        // El botón de abrir/cerrar crafteo usa solo el mouse (clic izquierdo)
        if (botonAbrirCrafteo != null)
            botonAbrirCrafteo.onClick.AddListener(ToggleCraftingPanel);

        // Generamos las tarjetas de receta
        GenerarTarjetasRecetas();
    }

    // ─── GENERAR TARJETAS DE RECETA DINÁMICAMENTE ─────────────────────────────
    // Crea una tarjeta por cada receta registrada en CraftingSystem
    private void GenerarTarjetasRecetas()
    {
        if (recipeContainer == null || recipePrefab == null) return;

        // Limpiamos tarjetas viejas (por si se llama varias veces)
        foreach (Transform child in recipeContainer)
            Destroy(child.gameObject);

        // Creamos una tarjeta por cada receta
        for (int i = 0; i < craftingSystem.todasLasRecetas.Count; i++)
        {
            int indice = i; // captura por valor para el lambda
            CraftingSystem.Receta receta = craftingSystem.todasLasRecetas[i];

            GameObject tarjeta = Instantiate(recipePrefab, recipeContainer);
            tarjeta.name = "Receta_" + i;

            // ── Nombre de la receta ──────────────────────────────────────────
            TMP_Text textoNombre = tarjeta.transform.Find("TextoNombreReceta")?.GetComponent<TMP_Text>();
            if (textoNombre != null)
                textoNombre.text = receta.nombreReceta;

            // ── Icono Ingrediente 1 ──────────────────────────────────────────
            Image iconoIng1 = tarjeta.transform.Find("IconoIngrediente1")?.GetComponent<Image>();
            if (iconoIng1 != null && receta.ingrediente1 != null)
            {
                iconoIng1.sprite = receta.ingrediente1.icon;
                iconoIng1.enabled = receta.ingrediente1.icon != null;
            }

            // ── Icono Ingrediente 2 ──────────────────────────────────────────
            Image iconoIng2 = tarjeta.transform.Find("IconoIngrediente2")?.GetComponent<Image>();
            if (iconoIng2 != null && receta.ingrediente2 != null)
            {
                iconoIng2.sprite = receta.ingrediente2.icon;
                iconoIng2.enabled = receta.ingrediente2.icon != null;
            }

            // ── Icono Resultado ──────────────────────────────────────────────
            Image iconoResultado = tarjeta.transform.Find("IconoResultado")?.GetComponent<Image>();
            if (iconoResultado != null)
            {
                if (receta.resultado != null)
                {
                    iconoResultado.sprite = receta.resultado.icon;
                    iconoResultado.enabled = receta.resultado.icon != null;
                }
                else
                {
                    // Receta especial (recarga de linterna): no hay ícono de resultado claro
                    iconoResultado.enabled = false;
                }
            }

            // ── Botón de la tarjeta para seleccionar la receta ───────────────
            Button botonTarjeta = tarjeta.GetComponent<Button>();
            if (botonTarjeta != null)
            {
                botonTarjeta.onClick.AddListener(() => SeleccionarReceta(indice, tarjeta));
            }
        }
    }

    // ─── SELECCIONAR RECETA (clic de mouse en la tarjeta) ─────────────────────
    private void SeleccionarReceta(int indice, GameObject tarjeta)
    {
        recetaSeleccionada = indice;

        // Resaltamos visualmente la tarjeta elegida y desresaltamos las demás
        for (int i = 0; i < recipeContainer.childCount; i++)
        {
            Image bg = recipeContainer.GetChild(i).GetComponent<Image>();
            if (bg != null)
                bg.color = (i == indice) ? colorSeleccionado : colorNormal;
        }

        // Actualizamos el estado del botón de craftear
        ActualizarBotonCraftear();

        Debug.Log($"[CraftingUI] Receta seleccionada: {craftingSystem.todasLasRecetas[indice].nombreReceta}");
    }

    // ─── ACTUALIZAR ESTADO DEL BOTÓN CRAFTEAR ─────────────────────────────────
    // Activa el botón solo si el jugador tiene los ingredientes necesarios
    private void ActualizarBotonCraftear()
    {
        if (botonCraftear == null) return;

        bool sePuede = recetaSeleccionada >= 0 && craftingSystem.PuedeCraftear(recetaSeleccionada);
        botonCraftear.interactable = sePuede;

        // Cambiamos el texto del botón para dar pista visual
        TMP_Text textoBtnCraft = botonCraftear.GetComponentInChildren<TMP_Text>();
        if (textoBtnCraft != null)
        {
            textoBtnCraft.text = sePuede ? "✓ Craftear" : "Faltan piezas";
        }
    }

    // ─── EJECUTAR CRAFTEO (clic en el botón "Craftear") ──────────────────────
    private void OnBotonCraftearClick()
    {
        if (recetaSeleccionada < 0)
        {
            MostrarFeedback("Selecciona una receta primero.", Color.red);
            return;
        }

        bool exito = craftingSystem.TryCraft(recetaSeleccionada);

        if (exito)
        {
            string nombreResultado = craftingSystem.todasLasRecetas[recetaSeleccionada].resultado?.itemName
                                     ?? "Linterna Recargada";
            MostrarFeedback($"¡{nombreResultado} creado!", new Color(0.2f, 0.8f, 0.2f)); // verde

            // Después de craftear, deseleccionamos y actualizamos las tarjetas
            recetaSeleccionada = -1;
            GenerarTarjetasRecetas(); // Regeneramos para reflejar inventario actualizado
        }
        else
        {
            // CraftingSystem ya logueó qué falta; aquí solo mostramos en UI
            string nombreReceta = craftingSystem.todasLasRecetas[recetaSeleccionada].nombreReceta;
            MostrarFeedback($"No tienes todo para: {nombreReceta}", Color.red);
        }
    }

    // ─── ABRIR / CERRAR PANEL DE CRAFTEO ─────────────────────────────────────
    // Se llama desde el botón de la UI (solo mouse)
    public void ToggleCraftingPanel()
    {
        craftingAbierto = !craftingAbierto;

        if (craftingPanel != null)
            craftingPanel.SetActive(craftingAbierto);

        if (craftingAbierto)
        {
            // Actualizamos las tarjetas al abrir (el inventario pudo haber cambiado)
            GenerarTarjetasRecetas();
            recetaSeleccionada = -1;
            ActualizarBotonCraftear();
        }

        Debug.Log("[CraftingUI] Panel de crafteo: " + (craftingAbierto ? "ABIERTO" : "CERRADO"));
    }

    // ─── MOSTRAR FEEDBACK ─────────────────────────────────────────────────────
    // Muestra un mensaje durante 2 segundos y desaparece
    private void MostrarFeedback(string mensaje, Color color)
    {
        if (textoFeedback == null) return;

        textoFeedback.text = mensaje;
        textoFeedback.color = color;
        textoFeedback.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(OcultarFeedbackDespues(2f));
    }

    private IEnumerator OcultarFeedbackDespues(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        if (textoFeedback != null)
            textoFeedback.gameObject.SetActive(false);
    }
}