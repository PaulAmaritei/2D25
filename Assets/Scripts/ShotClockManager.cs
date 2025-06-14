using UnityEngine;
using TMPro;

public class ShotClockManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public int totalRounds = 4;
    public float roundDuration = 60f;

    private float currentTime;
    private int currentRound = 1;
    private bool isTimerRunning = false;

    void Start()
    {
        // Automatically find the text if it's not assigned manually
        if (timerText == null)
        {
            GameObject textObject = GameObject.Find("Shot Clock Text");
            if (textObject != null)
            {
                timerText = textObject.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogError("Shot Clock Text object not found in the scene.");
            }
        }

        StartNewRound();
    }

    void Update()
    {
        if (!isTimerRunning || timerText == null) return;

        currentTime -= Time.deltaTime;
        if (currentTime < 0)
            currentTime = 0;

        UpdateTimerDisplay();

        if (currentTime <= 0)
        {
            isTimerRunning = false;
            Debug.Log("Round " + currentRound + " over!");

            if (currentRound < totalRounds)
            {
                currentRound++;
                Invoke("StartNewRound", 2f);
            }
            else
            {
                Debug.Log("All rounds complete!");
                // End game logic
            }
        }
    }

    void StartNewRound()
    {
        currentTime = roundDuration;
        isTimerRunning = true;
        Debug.Log("Starting round " + currentRound);
        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        Debug.Log("Timer updated: " + minutes + ":" + seconds);
    }
}
