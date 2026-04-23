using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Panel Game Over")]
    public GameObject panelGameOver;

    [Header("Botones")]
    public Button botonReintentar;
    public Button botonMenu;

    [HideInInspector]
    public bool estaGameOver = false;

    void Start()
    {
        if (panelGameOver != null)
            panelGameOver.SetActive(false);

        if (botonReintentar != null)
            botonReintentar.onClick.AddListener(Reintentar);

        if (botonMenu != null)
            botonMenu.onClick.AddListener(MenuPrincipal);
    }

    // ── ACTIVAR GAME OVER ─────────────────────────

    public void MostrarGameOver()
    {
        if (panelGameOver != null)
            panelGameOver.SetActive(true);

        Time.timeScale = 0f;
        estaGameOver = true;

        // 🔓 FORZAR cursor visible SIEMPRE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 🔴 Cerrar pausa si está abierta (sin tocar cursor)
        MenuPausa pausa = FindFirstObjectByType<MenuPausa>();
        if (pausa != null)
            pausa.CerrarPausaDesdeGameOver();
    }

    // ── BOTONES ─────────────────────────

    public void Reintentar()
    {
        Time.timeScale = 1f;
        estaGameOver = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MenuPrincipal()
    {
        Time.timeScale = 1f;
        estaGameOver = false;

        SceneManager.LoadScene(0);
    }
}