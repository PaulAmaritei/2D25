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
            // Add 2 or 3 points
            if (isThreePointer)
                ScoreManager.Instance.AddScore(1, 3);
            else
                ScoreManager.Instance.AddScore(1, 2);
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
