using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI statusText;  // 屏幕中间的胜负提示
    [SerializeField] TextMeshProUGUI scoreText;   // 左上角计分

    bool gameOver = false;
    BallController playerCtrl;
    Rigidbody2D playerRb;

    int score = 0;

    void Start(){
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player){
            playerCtrl = player.GetComponent<BallController>();
            playerRb   = player.GetComponent<Rigidbody2D>();
        }
        if (statusText) statusText.text = "";
        UpdateScoreUI();
    }

    void Update(){
        if (!gameOver) return;
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void AddScore(int amount){
        if (gameOver) return;
        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI(){
        if (scoreText) scoreText.text = $"Score: {score}";
    }

    public void Win(){
        if (gameOver) return;
        gameOver = true;
        if (statusText) statusText.text = "YOU WIN!\nPress R to Restart";
        FreezePlayer();
    }

    public void Lose(){
        if (gameOver) return;
        gameOver = true;
        if (statusText) statusText.text = "YOU LOSE!\nPress R to Restart";
        FreezePlayer();
    }

    void FreezePlayer(){
        if (playerCtrl) playerCtrl.enabled = false;
        if (playerRb){
            playerRb.linearVelocity = Vector2.zero;
            playerRb.gravityScale = 0f;
            playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }
}
