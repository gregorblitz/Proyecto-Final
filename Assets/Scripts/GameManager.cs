using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private GameObject playerRef;

    [Header("Game Over Config")]
    [SerializeField] private float totalSlowMoLenght = 2.0f;
    [SerializeField] private float delayBeforeRestar = 3.0f;
    public GameObject jumpscareUI;

    [Header("Victory Config")]
    [SerializeField] protected Collider victoryTrigger;

    [Header("Checkpoint Config")]
    [SerializeField] protected static GameObject currentCheckpoint;

    public GameOverUI gameOverUI;

    private void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
        playerRef.GetComponent<PlayerStatus>().OnPlayerDeath.AddListener(OnGameOver);
        Debug.Log(gameOverUI + " is cool");
    }

    void OnGameOver()
    {
        StartCoroutine(slowTimeToStop());
    }

    IEnumerator slowTimeToStop()
    {
        float slowMoTimer = 0f;

        while (slowMoTimer < totalSlowMoLenght)
        {
            slowMoTimer += Time.unscaledDeltaTime;

            float t = Mathf.InverseLerp(0f, totalSlowMoLenght, slowMoTimer);
            float alpha = Mathf.Lerp(1f, 0f, t);

            Time.timeScale = alpha;
            yield return null;
        }

        playerRef.GetComponent<PlayerStatus>().gameObject.SetActive(false);

        // 🔽 CAMBIO CLAVE AQUÍ 🔽
        Time.timeScale = 0f;
        if(jumpscareUI.activeInHierarchy) jumpscareUI.SetActive(false);
        gameOverUI.MostrarGameOver();
        // 🔼 FIN DEL CAMBIO 🔼

    }

    public static void OnGameVictory()
    {
        Debug.Log("You won!");
    }

    public static void SetNewCheckpoint(GameObject newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;
    }

    [ContextMenu("Llevar jugador a checkpoint")]
    public static void TakePlayerToCheckpoint()
    {
        GameObject.FindWithTag("Player").transform.position = currentCheckpoint.transform.position + Vector3.up * 2;
    }
}