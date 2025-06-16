using UnityEngine;
using System.Collections;

public class Player2Movement : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform ballHoldPoint;
    public float moveSpeed = 5f;
    public float jumpForce = 6f;
    public float driveSpeed = 9f;
    public float driveDuration = 0.175f;
    public float doubleTapTime = 0.25f;
    public float driveCooldownTime = 0.5f;
    public float minShootPower = 3f;
    public float maxShootPower = 8f;
    public float maxChargeTime = 1f;
    public float stealRange = 1.5f;
    public float stealChance = 0.3f;

    public SpriteRenderer playerSpriteRenderer;
    public Sprite spriteNoBall;
    public Sprite spriteWithBall;
    public Sprite spriteShooting;
    public bool hasBall = false;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private float lastLeftTapTime = -1f;
    private float lastRightTapTime = -1f;
    private bool isDriving = false;
    private float driveTimer = 0f;
    private int driveDirection = 0;
    private float driveCooldownTimer = 0f;
    private bool isCharging = false;
    private float chargeStartTime = 0f;
    private Vector2 _lastShotOrigin;
    private bool isShooting = false;
    private int originalLayer;
    private int phasingLayer = 3;

    public bool isMovementLocked = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        playerSpriteRenderer.sprite = spriteNoBall;
        originalLayer = gameObject.layer;
        gameObject.layer = 7;
        originalLayer = 7;
    }

    void Update()
    {
        if (isMovementLocked) return;

        driveCooldownTimer -= Time.deltaTime;

        float moveInput = 0f;
        if (!isDriving)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) moveInput = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) moveInput = 1f;
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }

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

        if (Input.GetKeyDown(KeyCode.M) && hasBall)
        {
            isCharging = true;
            chargeStartTime = Time.time;
        }

        if (Input.GetKeyUp(KeyCode.M) && isCharging)
        {
            float chargeDuration = Time.time - chargeStartTime;
            chargeDuration = Mathf.Min(chargeDuration, maxChargeTime);
            float normalizedPower = chargeDuration / maxChargeTime;
            isShooting = true;
            playerSpriteRenderer.sprite = spriteShooting;
            ShootBall(normalizedPower);
            isCharging = false;
            hasBall = false;
            playerSpriteRenderer.sprite = spriteNoBall;
            isShooting = false;
        }

        // Steal logic
        if (Input.GetKeyDown(KeyCode.M) && !hasBall)
        {
            GameObject otherPlayer = GameObject.FindGameObjectWithTag("Player1");
            if (otherPlayer != null)
            {
                float distance = Vector2.Distance(transform.position, otherPlayer.transform.position);
                if (distance <= stealRange)
                {
                    PlayerMovement p1 = otherPlayer.GetComponent<PlayerMovement>();
                    if (p1 != null && p1.hasBall)
                    {
                        if (Random.value < stealChance)
                        {
                            p1.hasBall = false;
                            p1.playerSpriteRenderer.sprite = p1.spriteNoBall;
                            hasBall = true;
                            playerSpriteRenderer.sprite = spriteWithBall;
                        }
                    }
                }
            }
        }
    }

    private void Drive(int direction)
    {
        isDriving = true;
        driveDirection = direction;
        driveTimer = driveDuration;
        originalLayer = gameObject.layer;
        gameObject.layer = phasingLayer;
        StartCoroutine(ResetLayerAfterDrive());
    }

    private IEnumerator ResetLayerAfterDrive()
    {
        yield return new WaitForSeconds(driveDuration);
        gameObject.layer = originalLayer;
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

    // Player 2 can pick up "Ball" or "BallFromP1"
    if (collision.gameObject.CompareTag("Ball") || collision.gameObject.CompareTag("BallFromP1"))
    {
        Debug.Log(gameObject.name + " collided with ball, tag: " + collision.gameObject.tag);
        Destroy(collision.gameObject);
        hasBall = true;
        playerSpriteRenderer.sprite = spriteWithBall;
    }
        // Steal logic (if you want to keep it, but it's now handled in Update)
        // else if (collision.gameObject.CompareTag("Player1") && collision.gameObject.GetComponent<PlayerMovement>().hasBall)
        // {
        //     ... (your steal code)
        // }
    }

    private void ShootBall(float normalizedPower)
    {
        _lastShotOrigin = ballHoldPoint.position;

        Vector3 spawnPosition = ballHoldPoint.position;
        GameObject newBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
        newBall.tag = "BallFromP2";
        Debug.Log("Ball shot by " + gameObject.name + ", tag: " + newBall.tag);

        Rigidbody2D ballRb = newBall.GetComponent<Rigidbody2D>();
        if (ballRb == null)
        {
            Debug.LogError("No Rigidbody2D on ball!");
            return;
        }

        ballRb.bodyType = RigidbodyType2D.Dynamic;
        ballRb.mass = 0.2f;
        ballRb.linearDamping = 0.05f;
        ballRb.gravityScale = 2f;

        PhysicsMaterial2D mat = new PhysicsMaterial2D();
        mat.bounciness = 0.8f;
        mat.friction = 0.05f;
        Collider2D col = newBall.GetComponent<Collider2D>();
        if (col != null) col.sharedMaterial = mat;

        float direction = transform.localScale.x > 0 ? 1 : -1;
        newBall.transform.position += new Vector3(0.2f * direction, 0, 0);

        Vector2 shootDirection = new Vector2(-1*0.9f * direction, 0.8f).normalized;
        float power = Mathf.Lerp(minShootPower, maxShootPower, normalizedPower);

        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;
        ballRb.AddForce(shootDirection * power, ForceMode2D.Impulse);
    }

    public Vector2 GetLastShotOrigin()
    {
        return _lastShotOrigin;
    }

    public bool IsThreePointer(Vector2 shotPosition)
    {
        return shotPosition.x > -2.6f;
    }
}
