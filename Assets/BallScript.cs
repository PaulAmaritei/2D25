using UnityEngine;

public class BallScript : MonoBehaviour
{
    private Rigidbody2D rb;
    public float stopThreshold = 0.1f;
    public float slowDownFactor = 0.9999f;
    private bool isStopping = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Simulate jump ball
        float horizontalPush = Random.Range(-1f, 1f);
        float verticalPush = 8f;
        rb.linearVelocity = new Vector2(horizontalPush, verticalPush);
    }

    void Update()
    {
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
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            isStopping = false;
        }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        rb.AddTorque(Random.Range(-30f, 30f));
    }
}