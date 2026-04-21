using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    [Header("Panel de pausa")]
    public GameObject panelPausa;

    [Header("Botones")]
    public Button botonReanudar;
    public Button botonMenu;
    public Button botonSalir;

    void Start()
    {
        Time.timeScale = 1f;

        if (panelPausa != null)
            panelPausa.SetActive(false);

        // Asignar funciones a los botones
        if (botonReanudar != null)
            botonReanudar.onClick.AddListener(Reanudar);

        if (botonMenu != null)
            botonMenu.onClick.AddListener(MenuPrincipal);

        if (botonSalir != null)
            botonSalir.onClick.AddListener(Salir);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePausa();
        }
    }

    // ── CONTROL DE PAUSA ─────────────────────────

    public void TogglePausa()
    {
        if (panelPausa == null) return;

        if (Time.timeScale == 0f)
        {
            Reanudar();
            return;
        }

        panelPausa.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Reanudar()
    {
        if (panelPausa != null)
            panelPausa.SetActive(false);

        Time.timeScale = 1f;
    }

    // ── BOTONES ─────────────────────────

    public void MenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu Inicio");
    }

    public void Salir()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}