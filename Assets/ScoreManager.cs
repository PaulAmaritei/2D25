using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int player1Score = 0;
    public int player2Score = 0;
    public float timeRemaining = 60f;
    public bool gameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ResetGameState()
    {
        player1Score = 0;
        player2Score = 0;
        timeRemaining = 60f;
        gameOver = false;
    }

    public void AddScore(int playerNumber, int points = 1)
    {
        if (gameOver) return;
        if (playerNumber == 1)
            player1Score += points;
        else if (playerNumber == 2)
            player2Score += points;
    }
}
