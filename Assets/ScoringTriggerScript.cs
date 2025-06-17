using UnityEngine;
using TMPro;
using System.Collections;

public class RightRimScoreTrigger : MonoBehaviour
{
    public PlayerMovement player1;
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
            Debug.Log("Ball entered rim trigger");
            _entryY = other.transform.position.y;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ball") && other.transform.position.y < _entryY)
        {
            if (other.gameObject.CompareTag("BallFromP1") || other.gameObject.CompareTag("Ball"))
            {
                Vector2 shotOrigin = player1.GetLastShotOrigin();
                bool isThreePointer = player1.IsThreePointer(shotOrigin);
                string message = isThreePointer ? "BANG" : "Score";
                scoreMessageText.text = message;

                // Add points and call scene logic
                int points = isThreePointer ? 3 : 2;
                ScoreManager.Instance.AddScore(1, points);

                // Optionally, you can also call the scene-specific logic here:
                SceneScript sceneScript = FindObjectOfType<SceneScript>();
                if (sceneScript != null)
                {
                    sceneScript.OnPlayerScored(1); // 1 for Player 1 scoring
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
