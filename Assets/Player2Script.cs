using System.Collections;
using UnityEngine;

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
    public float stealDistance = 1.5f;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private float lastLeftTapTime = -1f;
    private float lastRightTapTime = -1f;
    private bool isDriving = false;
    private float driveTimer = 0f;
    private int driveDirection = 0;
    private float driveCooldownTimer = 0f;
    public bool hasBall = false;
    private bool isCharging = false;
    private float chargeStartTime = 0f;
    private Vector2 _lastShotOrigin;
    private int normalLayer, playerLayer, phasingLayer;
    private bool isPhasing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        normalLayer = gameObject.layer;
        playerLayer = LayerMask.NameToLayer("Player2");
        phasingLayer = LayerMask.NameToLayer("Phasing");
    }

    void Update()
    {
         if (ScoreManager.Instance != null && ScoreManager.Instance.lockMovement)
        return;
        
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
                isPhasing = false;
                gameObject.layer = playerLayer;
            }
        }

        if (Input.GetKeyDown(KeyCode.M) && hasBall)
        {
            isCharging = true;
            chargeStartTime = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.M) && !hasBall)
        {
            TrySteal();
        }

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
        isPhasing = true;
        gameObject.layer = phasingLayer;
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

        if (collision.gameObject.CompareTag("Ball"))
        {
            Destroy(collision.gameObject);
            hasBall = true;
        }
    }

    private void ShootBall(float normalizedPower)
    {
        _lastShotOrigin = ballHoldPoint.position;
        Vector3 spawnPosition = ballHoldPoint.position;
        GameObject newBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
        newBall.tag = "BallFromP2";

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

        Vector2 shootDirection = new Vector2(0.9f, 0.8f).normalized;
        float power = Mathf.Lerp(minShootPower, maxShootPower, normalizedPower);

        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;
        ballRb.AddForce(shootDirection * power, ForceMode2D.Impulse);

        StartCoroutine(DisablePickupBriefly(newBall));
    }

    IEnumerator DisablePickupBriefly(GameObject ball)
    {
        ball.tag = "Untagged";
        yield return new WaitForSeconds(0.1f);
        ball.tag = "Ball";
    }

    private void TrySteal()
    {
        PlayerMovement otherPlayer = FindObjectOfType<PlayerMovement>();
        if (otherPlayer != null && otherPlayer.hasBall)
        {
            float dist = Vector2.Distance(transform.position, otherPlayer.transform.position);
            if (dist < stealDistance)
            {
                if (Random.value < 0.5f)
                {
                    otherPlayer.hasBall = false;
                    hasBall = true;
                }
            }
        }
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
