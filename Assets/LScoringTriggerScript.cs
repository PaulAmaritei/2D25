using UnityEngine;
using TMPro;
using System.Collections;

public class LeftRimScoreTrigger : MonoBehaviour
{
    public Player2Movement player2;
    public TextMeshProUGUI scoreMessageText;
    private float _entryY;

    private void Start()
    {
        if (scoreMessageText != null)
            scoreMessageText.text = "";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            _entryY = other.transform.position.y;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ball") && other.transform.position.y < _entryY)
        {
            if (other.gameObject.CompareTag("BallFromP2") || other.gameObject.CompareTag("Ball"))
            {
                Vector2 shotOrigin = player2.GetLastShotOrigin();
                bool isThreePointer = player2.IsThreePointer(shotOrigin);
                string message = isThreePointer ? "BANG" : "Score";
                scoreMessageText.text = message;

                // Add points and call scene logic
                int points = isThreePointer ? 3 : 2;
                ScoreManager.Instance.AddScore(2, points);

                // Call the scene-specific logic for Player 2 scoring
                SceneScript sceneScript = FindObjectOfType<SceneScript>();
                if (sceneScript != null)
                {
                    sceneScript.OnPlayerScored(2); // 2 for Player 2 scoring
                }
                else
                {
                    Debug.LogError("SceneScript not found in scene!");
                }

                StartCoroutine(ClearMessageAfterDelay(2f));
            }
        }
    }

    private IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        scoreMessageText.text = "";
    }
}
