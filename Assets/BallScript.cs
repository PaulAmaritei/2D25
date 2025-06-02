using UnityEngine;
public class BallScript : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Optional: Give initial velocity
        rb.velocity = new Vector2(2f, 5f); // for example
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Add a small random torque when hitting something
        float randomTorque = Random.Range(-50f, 50f);
        rb.AddTorque(randomTorque);
    }
}
