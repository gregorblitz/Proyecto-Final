using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuCompleto : MonoBehaviour
{
    [Header("Botones del menú principal")]
    public GameObject botonInicio;
    public GameObject botonConfiguracion;
    public GameObject botonCreditos;
    public GameObject botonSalir;

    [Header("Paneles de secciones")]
    public GameObject panelConfiguracion;
    public GameObject panelCreditos;
    public GameObject panelSalir;

    [Header("Paneles adicionales")]
    public GameObject panelGameOver;

    void Start()
    {
        // Asegurar tiempo normal
        Time.timeScale = 1f;

        // Mostrar botones principales
        MostrarBotones();

        // Ocultar paneles
        if (panelConfiguracion != null) panelConfiguracion.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelSalir != null) panelSalir.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
    }

    // ── BOTONES PRINCIPALES ─────────────────────────

    public void Inicio()
    {
        SceneManager.LoadScene("TestScene");
    }

    public void AbrirConfiguracion()
    {
        OcultarBotones();
        if (panelConfiguracion != null) panelConfiguracion.SetActive(true);
    }

    public void AbrirCreditos()
    {
        OcultarBotones();
        if (panelCreditos != null) panelCreditos.SetActive(true);
    }

    public void AbrirSalir()
    {
        OcultarBotones();
        if (panelSalir != null) panelSalir.SetActive(true);
    }

    public void Salir()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ── BOTÓN ATRÁS ─────────────────────────

    public void Atras()
    {
        CerrarPaneles();
        MostrarBotones();
    }

    // ── GAME OVER ───────────────────────────

    public void MostrarGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MenuDesdeGameOver()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }

    // ── UTILIDADES ──────────────────────────

    void OcultarBotones()
    {
        if (botonInicio != null) botonInicio.SetActive(false);
        if (botonConfiguracion != null) botonConfiguracion.SetActive(false);
        if (botonCreditos != null) botonCreditos.SetActive(false);
        if (botonSalir != null) botonSalir.SetActive(false);
    }

    void MostrarBotones()
    {
        if (botonInicio != null) botonInicio.SetActive(true);
        if (botonConfiguracion != null) botonConfiguracion.SetActive(true);
        if (botonCreditos != null) botonCreditos.SetActive(true);
        if (botonSalir != null) botonSalir.SetActive(true);
    }

    void CerrarPaneles()
    {
        if (panelConfiguracion != null) panelConfiguracion.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelSalir != null) panelSalir.SetActive(false);
    }
}