using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int player1Score = 0;
    public int player2Score = 0;
    public TextMeshProUGUI scoreDisplay;
    public TextMeshProUGUI winMessageText;

    public PlayerMovement player1; // Assign in Inspector or find by name
    public Player2Movement player2; // Assign in Inspector or find by name
    public Transform player1Spawn; // Assign in Inspector or find by name
    public Transform player2Spawn; // Assign in Inspector or find by name

    public Button resetButton; // Assign in Inspector or find by name
    public Button endGameButton; // Assign in Inspector or find by name

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
            return;
        }

        // Disable buttons at start or on scene load
        if (resetButton != null) resetButton.gameObject.SetActive(false);
        if (endGameButton != null) endGameButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Reassign buttons after scene load
        if (resetButton == null)
            resetButton = GameObject.Find("ResetButton")?.GetComponent<Button>();
        if (endGameButton == null)
            endGameButton = GameObject.Find("EndGameButton")?.GetComponent<Button>();

        // Disable buttons
        if (resetButton != null) resetButton.gameObject.SetActive(false);
        if (endGameButton != null) endGameButton.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset timer and game state
        timeRemaining = 60f;
        gameOver = false;
        player1Score = 0;
        player2Score = 0;

        // Reset player references if needed
        player1 = GameObject.Find("Player1")?.GetComponent<PlayerMovement>();
        player2 = GameObject.Find("Player2")?.GetComponent<Player2Movement>();
        player1Spawn = GameObject.Find("Player1Spawn")?.transform;
        player2Spawn = GameObject.Find("Player2Spawn")?.transform;

        // Reset UI references
        scoreDisplay = GameObject.Find("ScoreDisplay")?.GetComponent<TextMeshProUGUI>();
        winMessageText = GameObject.Find("WinMessageText")?.GetComponent<TextMeshProUGUI>();

        // Reassign buttons
        resetButton = GameObject.Find("ResetButton")?.GetComponent<Button>();
        endGameButton = GameObject.Find("EndGameButton")?.GetComponent<Button>();

        // Disable buttons
        if (resetButton != null) resetButton.gameObject.SetActive(false);
        if (endGameButton != null) endGameButton.gameObject.SetActive(false);

        // Update score display
        UpdateScoreDisplay();
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
        // Delete all balls
        DestroyAllBalls();

        if (scoringPlayerNumber == 1)
        {
            // Player 2 was scored on, teleport and give ball
            player2.transform.position = player2Spawn.position;
            player2.hasBall = true;
            if (player2.playerSpriteRenderer != null)
                player2.playerSpriteRenderer.sprite = player2.spriteWithBall;
        }
        else if (scoringPlayerNumber == 2)
        {
            // Player 1 was scored on, teleport and give ball
            player1.transform.position = player1Spawn.position;
            player1.hasBall = true;
            if (player1.playerSpriteRenderer != null)
                player1.playerSpriteRenderer.sprite = player1.spriteWithBall;
        }
    }

    private void DestroyAllBalls()
    {
        // Destroy all balls with any relevant tag
        string[] ballTags = { "Ball", "BallFromP1", "BallFromP2" };
        foreach (string tag in ballTags)
        {
            GameObject[] balls = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject ball in balls)
            {
                Destroy(ball);
            }
        }
    }

    private void EndGame()
    {
        player1.isMovementLocked = true;
        player2.isMovementLocked = true;

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
            StartCoroutine(FadeOutWinMessage(5f));
        }
    }

    private IEnumerator FadeOutWinMessage(float delay)
    {
        // Wait before starting fade
        yield return new WaitForSeconds(delay);

        // Fade out the win message
        float fadeDuration = 1f;
        float timer = 0f;
        Color startColor = winMessageText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            winMessageText.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            yield return null;
        }

        winMessageText.text = "";
        winMessageText.color = startColor; // Reset alpha for next game

        // Show reset and end game buttons
        if (resetButton != null) resetButton.gameObject.SetActive(true);
        if (endGameButton != null) endGameButton.gameObject.SetActive(true);
    }

    // Call these from your UI buttons
    public void OnResetGameClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnEndGameClicked()
    {
        // Quit the game (works in builds, not in Editor)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
