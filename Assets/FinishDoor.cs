using UnityEngine;

public class FinishDoor : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            FindObjectOfType<GameManager>()?.Win();
        }
    }
}
