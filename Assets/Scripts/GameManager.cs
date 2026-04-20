using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    private GameObject playerRef;

    [Header("Game Over Config")]
    [SerializeField]private float totalSlowMoLenght = 2.0f;
    [SerializeField]private float delayBeforeRestar = 3.0f;

    [Header("Victory Config")]
    [SerializeField]protected Collider victoryTrigger;

    [Header("Victory Config")]
    [SerializeField]protected static GameObject currentCheckpoint;

    private void Start() {
        playerRef = GameObject.FindGameObjectWithTag("Player");
        playerRef.GetComponent<PlayerStatus>().OnPlayerDeath.AddListener(OnGameOver);
    }
    void OnGameOver()
    {
        Debug.Log("The game is over");
        StartCoroutine(slowTimeToStop());
    }

    IEnumerator slowTimeToStop(){
        float slowMoTimer = 0f; 

        while (slowMoTimer < totalSlowMoLenght)
        {
            slowMoTimer += Time.unscaledDeltaTime;

            float t = Mathf.InverseLerp(0f, totalSlowMoLenght, slowMoTimer);

            // 2. Use 0.75 to set alpha between 0.2 and 1.0 (Result: 0.8)
            float alpha = Mathf.Lerp(1f, 0f, t);

            Time.timeScale = alpha;
            yield return null;
        }

        Debug.Log("turning off player......");
        playerRef.GetComponent<PlayerStatus>().gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(delayBeforeRestar);

        Debug.Log("Reloading scene...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        
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
        GameObject.FindWithTag("Player").transform.position = currentCheckpoint.transform.position + Vector3.up *2;
    }

    

}
