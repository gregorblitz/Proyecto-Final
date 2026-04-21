using UnityEngine;

public class MenuPausa : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPausa;        // Panel de pausa
    public GameObject menuCompletoPanel; // Panel del menú completo

    void Start()
    {
        // Estado inicial
        Time.timeScale = 1f;

        if (panelPausa != null)
            panelPausa.SetActive(false);

        if (menuCompletoPanel != null)
            menuCompletoPanel.SetActive(false);
    }

    void Update()
    {
        // Tecla ESC para pausar / reanudar
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
        // Oculta el panel de pausa y muestra el menú completo
        if (panelPausa != null) panelPausa.SetActive(false);
        if (menuCompletoPanel != null) menuCompletoPanel.SetActive(true);

        // Reanuda el tiempo por si estaba en pausa
        Time.timeScale = 1f;
    }

    public void Salir()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}