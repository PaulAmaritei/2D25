using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int player1Score = 0;
    public int player2Score = 0;
    public TextMeshProUGUI scoreDisplay;
    public TextMeshProUGUI winMessageText;

    public PlayerMovement player1; // Assign in Inspector
    public Player2Movement player2; // Assign in Inspector
    public Transform player1Spawn; // Assign in Inspector
    public Transform player2Spawn; // Assign in Inspector

    private float timeRemaining = 60f; // 1 minute
    private bool gameOver = false;

    public bool lockMovement = false;

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
        }
    }

    private void Update()
    {
        if (!gameOver)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                gameOver = true;
                EndGame();
            }
            UpdateScoreDisplay();
        }
    }

    public void AddScore(int playerNumber, int points = 1)
    {
        if (gameOver) return;

        if (playerNumber == 1)
            player1Score += points;
        else if (playerNumber == 2)
            player2Score += points;

        UpdateScoreDisplay();
        ResetAfterScore(playerNumber); // Teleport and give ball to scored-on player
    }

    private void UpdateScoreDisplay()
    {
        if (scoreDisplay != null)
            scoreDisplay.text = $"P1: {player1Score}   P2: {player2Score}   Time: {Mathf.CeilToInt(timeRemaining)}";
    }

    public void ResetAfterScore(int scoringPlayerNumber)
    {
        if (scoringPlayerNumber == 1)
        {
            // Player 2 was scored on, teleport and give ball
            player2.transform.position = player2Spawn.position;
            player2.hasBall = true;
        }
        else if (scoringPlayerNumber == 2)
        {
            // Player 1 was scored on, teleport and give ball
            player1.transform.position = player1Spawn.position;
            player1.hasBall = true;
        }
    }

    private void EndGame()
{
    lockMovement = true; // Add this line

    string winner;
    if (player1Score > player2Score)
        winner = "P1 wins!";
    else if (player2Score > player1Score)
        winner = "P2 wins!";
    else
        winner = "Tie!";

    if (winMessageText != null)
    {
        winMessageText.text = winner;
        StartCoroutine(ClearWinMessageAfterDelay(5f));
    }
}

    private IEnumerator ClearWinMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (winMessageText != null)
            winMessageText.text = "";
    }
}
