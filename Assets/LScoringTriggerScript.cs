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
            if (isThreePointer)
                ScoreManager.Instance.AddScore(2, 3);
            else
                ScoreManager.Instance.AddScore(2, 2);
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
