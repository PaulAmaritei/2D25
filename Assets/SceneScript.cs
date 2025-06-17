using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SceneScript : MonoBehaviour
{
    public Button resetButton;
    public Button endGameButton;
    public TextMeshProUGUI winMessageText;
    public TextMeshProUGUI scoreDisplay;
    public PlayerMovement player1;
    public Player2Movement player2;
    public Transform player1Spawn;
    public Transform player2Spawn;

    private void Start()
    {
        // Assign button listeners
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(OnResetGameClicked);
            resetButton.gameObject.SetActive(false);
        }
        if (endGameButton != null)
        {
            endGameButton.onClick.RemoveAllListeners();
            endGameButton.onClick.AddListener(OnEndGameClicked);
            endGameButton.gameObject.SetActive(false);
        }
        if (winMessageText != null) winMessageText.text = "";
        // Unlock movement on scene start (in case of reset)
        if (player1 != null) player1.isMovementLocked = false;
        if (player2 != null) player2.isMovementLocked = false;
        // Update UI
        UpdateScoreDisplay();
    }

    private void Update()
    {
        if (!ScoreManager.Instance.gameOver)
        {
            ScoreManager.Instance.timeRemaining -= Time.deltaTime;
            if (ScoreManager.Instance.timeRemaining <= 0f)
            {
                ScoreManager.Instance.timeRemaining = 0f;
                ScoreManager.Instance.gameOver = true;
                string winner = ScoreManager.Instance.player1Score > ScoreManager.Instance.player2Score ? "P1 wins!" :
                               ScoreManager.Instance.player2Score > ScoreManager.Instance.player1Score ? "P2 wins!" : "Tie!";
                ShowGameEndUI(winner);
                // Lock movement when game ends
                if (player1 != null) player1.isMovementLocked = true;
                if (player2 != null) player2.isMovementLocked = true;
            }
            UpdateScoreDisplay();
        }
    }

    public void OnPlayerScored(int scoringPlayerNumber)
    {
        UpdateScoreDisplay();
        ResetAfterScore(scoringPlayerNumber);
    }

    private void ResetAfterScore(int scoringPlayerNumber)
    {
        DestroyAllBalls();
        if (scoringPlayerNumber == 1 && player2 != null && player2Spawn != null)
        {
            player2.transform.position = player2Spawn.position;
            player2.hasBall = true;
            if (player2.playerSpriteRenderer != null)
                player2.playerSpriteRenderer.sprite = player2.spriteWithBall;
        }
        else if (scoringPlayerNumber == 2 && player1 != null && player1Spawn != null)
        {
            player1.transform.position = player1Spawn.position;
            player1.hasBall = true;
            if (player1.playerSpriteRenderer != null)
                player1.playerSpriteRenderer.sprite = player1.spriteWithBall;
        }
    }

    private void DestroyAllBalls()
    {
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

    public void ShowGameEndUI(string winnerMessage)
    {
        if (winMessageText != null)
        {
            winMessageText.text = winnerMessage;
            StartCoroutine(FadeOutWinMessage(5f));
        }
        
    }

    private IEnumerator FadeOutWinMessage(float delay)
{
    if (winMessageText != null)
    {
        yield return new WaitForSeconds(delay); // Wait for the initial delay

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
        winMessageText.color = startColor;

        // NOW show the buttons after the message is gone
        if (resetButton != null) resetButton.gameObject.SetActive(true);
        if (endGameButton != null) endGameButton.gameObject.SetActive(true);
    }
}

    public void OnResetGameClicked()
    {
        // Reset the game state before reloading the scene
        ScoreManager.Instance.ResetGameState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnEndGameClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void UpdateScoreDisplay()
    {
        if (scoreDisplay != null)
        {
            scoreDisplay.text = $"P1: {ScoreManager.Instance.player1Score}   P2: {ScoreManager.Instance.player2Score}   Time: {Mathf.CeilToInt(ScoreManager.Instance.timeRemaining)}";
        }
    }
}
