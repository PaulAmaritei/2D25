using UnityEngine;

public class BallScript : MonoBehaviour
{
    private Rigidbody2D rb;
    public float stopThreshold = 0.1f;
    public float slowDownFactor = 0.999f;
    private bool isStopping = false;

    public float gravityScale = 3f;
    public float bounceDampening = 0.8f;
    public float torqueDampeningPerBounce = 0.5f;
    public float initialTorqueStrength = 10f;

    private bool hasStoppedCompletely = false;
    private int bounceCount = 0;                    
    public int maxBounces = 7;                   

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;

        
        float horizontalPush = Random.Range(-1.5f, 1.5f);
        float verticalPush = 8f; 
        rb.linearVelocity = new Vector2(horizontalPush, verticalPush);
        rb.AddTorque(Random.Range(-initialTorqueStrength, initialTorqueStrength));
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

        bounceCount++;  // Increment bounce count

        // If 7 bounces, stop the ball
        if (bounceCount >= maxBounces)
        {
            StopBallCompletely();
            return;
        }

        // Damp bounce
        Vector2 velocity = rb.linearVelocity;
        velocity.y *= bounceDampening;
        rb.linearVelocity = new Vector2(velocity.x, velocity.y);

        // Dampen spin per bounce
        rb.angularVelocity *= torqueDampeningPerBounce;
    }

    void StopBallCompletely()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;
        hasStoppedCompletely = true;
    }
}
