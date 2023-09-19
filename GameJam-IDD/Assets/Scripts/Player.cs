using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public float acceleration;
    public float moveSpeed;
    public float moveSpeedRunning;
    public float airSpeed;
    public float airSpeedRunning;
    public float jumpForce;
    public float wallJumpLerp = 10;
    public float slideSpeed = 5;

    public LayerMask groundObjects;
    public LayerMask walls;
    private bool onRightWall;
    private bool onLeftWall;
    public float checkRadius;
    public float jumpDamping = 0.5f;

    private Rigidbody2D rb;
    private bool facingRight = true;
    private float startMoveSpeed;
    private float startAirMoveSpeed;
    private float moveDirection;
    private bool isGrounded = false;
    private float currentSpeed = 0.0f;

    [Header("Acceleration and Deceleration")]
    public float accelerationTime = 0.1f; // Tiempo de aceleración en segundos (6 frames a 60 FPS)
    public float decelerationTime = 3.0f; // Tiempo de desaceleración en segundos
    private float accelerationTimer;
    private float decelerationTimer;
    private float maxSpeed;

    [Header("Jump")]
    public float groundCheckDistance;
    private BoxCollider2D boxCollider2d;
    private SpriteRenderer sr;

    bool canMove = true;
    bool wallGrab = false;
    bool wallSlide = false;
    bool wallJumped = false;
    bool onWall = false;
    bool groundTouch = false;

    // Start is called before the first frame update
    void Awake()
    {
        #region GetComponents
        rb = GetComponent<Rigidbody2D>();
        boxCollider2d = GetComponent<BoxCollider2D>();
        #endregion
        #region GameObject.Find()
        #endregion
    }
    private void Start()
    {
        startMoveSpeed = moveSpeed;
        startAirMoveSpeed = airSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //Get Inputs
        Movement();

        //Animation
        Animate();

    }
    private void FixedUpdate()
    {
        //Check if ground
        //isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundObjects);

        CheckGroundForJump(); // this function now takes into account the whole player collider when checking ground
        
        //WallCheck();
        
    }
    private void WallJump()
    {
        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = onRightWall ? Vector2.left : Vector2.right;

        BasicJump((Vector2.up / 2f + wallDir / 2f), true);

        wallJumped = true;
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * moveSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * moveSpeed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }


    private void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        if (!isGrounded)
            dir /= airSpeed;

        Walk(dir);

        Debug.Log(((x > 0.5f && onRightWall) || (x < -0.5f && onLeftWall)));

        Debug.Log(onWall);
        Debug.Log(canMove);

        if (onWall && ((x > 0.5f && onRightWall) || (x < -0.5f && onLeftWall)) && canMove)
        {
            wallGrab = true;
            wallSlide = false;
        }
        else if ((x < 0.4 || x > -0.4) || !onWall)
        {
            wallGrab = false;
            wallSlide = false;
        }

        if (wallGrab)
        {
            rb.gravityScale = 0;
            if (x > .2f || x < -.2f)
                rb.velocity = new Vector2(rb.velocity.x, 0);

            float speedModifier = y > 0 ? .5f : 1;

            rb.velocity = new Vector2(rb.velocity.x, y * (moveSpeed * speedModifier));
        }
        else
        {
            rb.gravityScale = 3;
        }        

        if (onWall && isGrounded)
        {
            onWall = false;
            isGrounded = true;
            if (x > 0.8f || x < -0.8f)
            {
                onWall = true;
                isGrounded = false;
            }
        }
        else
        {
            if (onWall && !isGrounded)
            {
                if (x != 0 && !wallGrab)
                {
                    wallSlide = true;
                    WallSlide();
                }
            }
        }

        if (!onWall || isGrounded)
            wallSlide = false;

        if (Input.GetButtonDown("Jump"))
        {
            if(onWall && !isGrounded) 
            {
                WallJump(); // this means that the player is touching a wall and must wall jump
            }
            else if(isGrounded)
            {
                BasicJump(Vector2.up, false);
            }
            
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            
        }

        if(Input.GetButtonDown("Sprint"))
        {
            moveSpeed = moveSpeedRunning;
            airSpeed = airSpeedRunning; 
        }
        if (Input.GetButtonUp("Sprint"))
        {
            moveSpeed = startMoveSpeed;
            airSpeed = startAirMoveSpeed;
        }

        onWall = Physics2D.OverlapCircle(new Vector2(transform.position.x+ .875f, transform.position.y), .15f, walls)
            || Physics2D.OverlapCircle(new Vector2(transform.position.x - .875f, transform.position.y), .15f, walls);
        onRightWall = Physics2D.OverlapCircle(new Vector2(transform.position.x + .875f, transform.position.y), .15f, walls);
        onLeftWall = Physics2D.OverlapCircle(new Vector2(transform.position.x - .875f, transform.position.y), .15f, walls);
    }

    private void WallSlide()
    {
        if (!canMove)
            return;

        bool pushingWall = false;
        if ((rb.velocity.x > 0 && onRightWall) || (rb.velocity.x < 0 && onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(rb.velocity.x, -slideSpeed);
    }

    private void Animate()
    {
        if (moveDirection > 0 && !facingRight)
            Flip();
        else if (moveDirection < 0 && facingRight)
            Flip();
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector2(transform.position.x + .875f, transform.position.y), .15f);
        Gizmos.DrawSphere(new Vector2(transform.position.x - .875f, transform.position.y), .15f);
    }

    private void BasicJump(Vector2 dir, bool wall)
    {
        if(wall)
        {
            float newForce = jumpForce * 2f;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.velocity += dir * newForce;
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.velocity += dir * jumpForce;
        }        
            Debug.Log(rb.velocity);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }
    private void CheckGroundForJump()
    {
        Color c = Color.red;
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider2d.bounds.center, boxCollider2d.bounds.size,0f,Vector2.down,groundCheckDistance, groundObjects);
        if (hit.collider != null)
        {
            c = Color.blue;
            isGrounded = true;
        }
        else
        {
            c = Color.red;
            isGrounded = false;
        }
        Debug.DrawRay(boxCollider2d.bounds.center + new Vector3(boxCollider2d.bounds.extents.x, 0), Vector2.down * (boxCollider2d.bounds.extents.y + groundCheckDistance), c);
        Debug.DrawRay(boxCollider2d.bounds.center - new Vector3(boxCollider2d.bounds.extents.x, 0), Vector2.down * (boxCollider2d.bounds.extents.y + groundCheckDistance), c);
        Debug.DrawRay(boxCollider2d.bounds.center - new Vector3(boxCollider2d.bounds.extents.x, boxCollider2d.bounds.extents.y+groundCheckDistance), Vector2.down * (boxCollider2d.bounds.extents.y + groundCheckDistance), c);
    }
}
