using UnityEngine;

public class AreaDetection : MonoBehaviour
{
    public enum AreaType { Effect, Victory, Death, Checkpoint}
    public AreaType areaType;

    private PlayerStatus playerStatus;

    private void Start() {
      //  gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
      playerStatus = GameObject.FindWithTag("Player").GetComponent<PlayerStatus>();
    }

    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag("Player"))
        {
            switch (areaType)
            {
                case AreaType.Effect:
                    Debug.Log("Area was of effect type");
                    break;

                case AreaType.Victory:
                    Debug.Log("Area was of victory type");
                    GameManager.OnGameVictory();
                    break;

                case AreaType.Death:
                    Debug.Log("Area was of death type");
                    playerStatus.ModifyHealth(-101);
                    break;

                case AreaType.Checkpoint:
                    Debug.Log("Area was of checkpoint type");
                    GameManager.SetNewCheckpoint(other.gameObject);
                    break;

                default:
                    Debug.Log("How???????");
                    break;
            }
        }
        
    }
}
