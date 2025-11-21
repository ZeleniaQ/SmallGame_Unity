using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] int value = 1;              // 拾取加多少分

    void OnTriggerEnter2D(Collider2D other){
        if (!other.CompareTag("Player")) return;

        var gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.AddScore(value);

        Destroy(gameObject);
    }
}
