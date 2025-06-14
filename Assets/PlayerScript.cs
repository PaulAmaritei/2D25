using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform ballHoldPoint;
    public float moveSpeed = 5f;
    public float jumpForce = 6f;
    public float driveSpeed = 9f;
    public float driveDuration = 0.175f;
    public float doubleTapTime = 0.25f;
    public float driveCooldownTime = 0.5f;

    // Shooting
    public float minShootPower = 3f;
    public float maxShootPower = 8f;
    public float maxChargeTime = 1f;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private float lastLeftTapTime = -1f;
    private float lastRightTapTime = -1f;
    private bool isDriving = false;
    private float driveTimer = 0f;
    private int driveDirection = 0;
    private float driveCooldownTimer = 0f;
    private bool hasBall = false;
    private bool isCharging = false;
    private float chargeStartTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
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

        // Double-tap dash
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

        // Dash movement
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

        // Start charging when M is pressed and player has a ball
        if (Input.GetKeyDown(KeyCode.M) && hasBall)
        {
            isCharging = true;
            chargeStartTime = Time.time;
        }

        // Release and shoot when M is released
        if (Input.GetKeyUp(KeyCode.M) && isCharging)
        {
            float chargeDuration = Time.time - chargeStartTime;
            chargeDuration = Mathf.Min(chargeDuration, maxChargeTime);
            float normalizedPower = chargeDuration / maxChargeTime;
            ShootBall(normalizedPower);
            isCharging = false;
            hasBall = false;
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
        // Ground check
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }

        // Ball pickup
        if (collision.gameObject.CompareTag("Ball"))
        {
            Destroy(collision.gameObject);
            hasBall = true;
        }
    }

    private void ShootBall(float normalizedPower)
    {
        Vector3 spawnPosition = ballHoldPoint.position;
        Debug.Log("BallHoldPoint position: " + spawnPosition);

        GameObject newBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Ball spawned at (actual): " + newBall.transform.position);

        Rigidbody2D ballRb = newBall.GetComponent<Rigidbody2D>();
        if (ballRb == null)
        {
            Debug.LogError("No Rigidbody2D on ball!");
            return;
        }

        // Set physics values by script
        ballRb.bodyType = RigidbodyType2D.Dynamic;
        ballRb.mass = 0.2f;
        ballRb.linearDamping = 0.05f;
        ballRb.gravityScale = 2f;

        // Set physics material by script (optional, but recommended)
        PhysicsMaterial2D mat = new PhysicsMaterial2D();
        mat.bounciness = 0.8f;
        mat.friction = 0.05f;
        Collider2D col = newBall.GetComponent<Collider2D>();
        if (col != null) col.sharedMaterial = mat;

        // Set shooting direction for a higher arc
        Vector2 shootDirection = new Vector2(1f, 0.7f).normalized;
        float power = Mathf.Lerp(minShootPower, maxShootPower, normalizedPower);

        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;
        ballRb.AddForce(shootDirection * power, ForceMode2D.Impulse);

        // Optional: Disable pickup briefly
        StartCoroutine(DisablePickupBriefly(newBall));
    }

    IEnumerator DisablePickupBriefly(GameObject ball)
    {
        ball.tag = "Untagged";
        yield return new WaitForSeconds(0.1f);
        ball.tag = "Ball";
    }
}
