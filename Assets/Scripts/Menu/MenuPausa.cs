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

    [Header("Referencia Game Over")]
    public GameOverUI gameOverUI;

    void Start()
    {
        Time.timeScale = 1f;

        if (panelPausa != null)
            panelPausa.SetActive(false);

        BloquearCursor();

        if (botonReanudar != null)
            botonReanudar.onClick.AddListener(Reanudar);

        if (botonMenu != null)
            botonMenu.onClick.AddListener(MenuPrincipal);

        if (botonSalir != null)
            botonSalir.onClick.AddListener(Salir);
    }

    void Update()
    {
        // 🚫 Si hay Game Over, ignorar ESC
        if (gameOverUI != null && gameOverUI.estaGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePausa();
        }
    }

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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        if (panelPausa != null)
            panelPausa.SetActive(false);

        Time.timeScale = 1f;

        BloquearCursor();
    }

    // 🔴 ESTE ES CLAVE
    public void CerrarPausaDesdeGameOver()
    {
        if (panelPausa != null)
            panelPausa.SetActive(false);

        // ❌ NO tocar cursor aquí
    }

    public void MenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuCompleto");
    }

    public void Salir()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void BloquearCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}