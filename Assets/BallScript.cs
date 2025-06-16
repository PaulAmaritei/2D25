using System.Collections;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    private Rigidbody2D rb;
    public float stopThreshold = 0.5f;
    public float slowDownFactor = 0.98f;
    private bool isStopping = false;

    public float gravityScale = 2f;
    public float bounceDampening = 0.85f;
    public float torqueDampeningPerBounce = 0.85f;
    public float initialTorqueStrength = 10f;

    private bool hasStoppedCompletely = false;
    private int bounceCount = 0;
    public int maxBounces = 10;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    void Update()
    {
        if (hasStoppedCompletely) return;

        float speed = rb.linearVelocity.magnitude;
        float spin = Mathf.Abs(rb.angularVelocity);

        if ((speed < stopThreshold || spin < stopThreshold) && (speed > 0.01f || spin > 0.01f))
        {
            isStopping = true;
        }

        if (isStopping)
        {
            float friction = 1f - (1f - slowDownFactor) * Time.deltaTime * 60f;
            rb.linearVelocity *= friction;
            rb.angularVelocity *= friction;

            if (rb.linearVelocity.magnitude < 0.01f && Mathf.Abs(rb.angularVelocity) < 0.01f)
            {
                StopBallCompletely();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
         if (hasStoppedCompletely) return;

    bounceCount++;
    if (bounceCount >= maxBounces)
    {
        StopBallCompletely();
        return;
    }

    Vector2 velocity = rb.linearVelocity;
    velocity.y *= bounceDampening;
    rb.linearVelocity = velocity;

    rb.angularVelocity *= torqueDampeningPerBounce;

    // --- REMOVE OR COMMENT OUT THIS PART ---
    // if (collision.gameObject.CompareTag("Rim") ||
    //     collision.gameObject.CompareTag("Backboard") ||
    //     collision.gameObject.CompareTag("Player1") ||
    //     collision.gameObject.CompareTag("Player2"))
    // {
    //     if (gameObject.CompareTag("BallFromP1") || gameObject.CompareTag("BallFromP2"))
    //     {
    //         gameObject.tag = "Ball";
    //     }
    // }
    // --- END REMOVE ---

    // Only use the delay coroutine
    StartCoroutine(SetBallTagAfterDelay());
    }

    IEnumerator SetBallTagAfterDelay()
{
    yield return new WaitForSeconds(0.1f); // Adjust delay as needed
    if (gameObject != null) // Safety check
    {
        gameObject.tag = "Ball";
    }
}

    void StopBallCompletely()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;
        hasStoppedCompletely = true;
        // DO NOT change tag here! Only change tag when hitting rim, backboard, or another player.
    }
}