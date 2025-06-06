using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 6f;
    public float driveSpeed = 9f;
    public float driveDuration = 0.175f;
    public float doubleTapTime = 0.25f;
    public float driveCooldownTime = 0.5f;

    private Rigidbody2D rb;
    private bool isGrounded = false;

    private float lastLeftTapTime = -1f;
    private float lastRightTapTime = -1f;
    private bool isDriving = false;
    private float driveTimer = 0f;
    private int driveDirection = 0;
    private float driveCooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        driveCooldownTimer -= Time.deltaTime;

        // Movement
        float moveInput = 0f;
        if (!isDriving)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) moveInput = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) moveInput = 1f;
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }

        // Double-tap detection for driving
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Time.time - lastLeftTapTime < doubleTapTime && driveCooldownTimer <= 0f && isGrounded)
            {
                Drive(-1);
            }
            lastLeftTapTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Time.time - lastRightTapTime < doubleTapTime && driveCooldownTimer <= 0f && isGrounded)
            {
                Drive(1);
            }
            lastRightTapTime = Time.time;
        }

        // Drive movement
        if (isDriving)
        {
            rb.linearVelocity = new Vector2(driveDirection * driveSpeed, rb.linearVelocity.y);
            driveTimer -= Time.deltaTime;
            if (driveTimer <= 0f)
            {
                isDriving = false;
                driveCooldownTimer = driveCooldownTime;
            }
        }
    }

    private void Drive(int direction)
    {
        isDriving = true;
        driveDirection = direction;
        driveTimer = driveDuration;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }
}
