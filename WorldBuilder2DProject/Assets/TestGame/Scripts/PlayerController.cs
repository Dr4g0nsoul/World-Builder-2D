using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * 
 * Controller from https://github.com/sergioadair/Simple-2D-Character-Controller
 * 
 */
public class PlayerController : MonoBehaviour
{
    public Animator animator;
    private Rigidbody2D rb;

    public float speed;
    public float jumpForce;
    public float jumpTime;
    [Range(0f,3f)]public float holdMultiplier;
    public float horizontalDrag;
    public float attackDrag;
    public float maxVerticalSpeed;

    private float hMoveInput;
    private bool facingRight = true;

    private bool isGrounded;
    public Transform feetTransform;
    public LayerMask groundLayer;
    private float feetRadius = 0.3f;

    private float jumpTimeCounter;
    private bool isJumping;
    private int extraJumpsValue = 1;
    private int extraJumps;

    private bool attacking;
    public float attackSpeed;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        extraJumps = extraJumpsValue;
        attacking = false;
    }

    private void Update()
    {
        hMoveInput = Input.GetAxisRaw("Horizontal");

        JumpUpdate();

        Attack();
    }

    void FixedUpdate()
    {
        

        animator.SetFloat("NormalizedRunSpeed", Mathf.Abs(rb.velocity.x) / speed);

        // Fliping the character
        if ((!facingRight && hMoveInput > 0) || (facingRight && hMoveInput < 0)) FlipCharacter();

        // Horizontal movement
        rb.AddForce(new Vector2(hMoveInput * speed, 0f), ForceMode2D.Impulse);

        float hDrag = attacking ? attackDrag : horizontalDrag;

        //Drag
        if (Mathf.Abs(rb.velocity.x) > 0.0001f)
            rb.velocity = new Vector2(rb.velocity.x * (1 - Mathf.Clamp(hDrag, 0, 1)), rb.velocity.y);
        else
            rb.velocity = new Vector2(0f, rb.velocity.y);

        // Is the player on the ground?
        isGrounded = Physics2D.OverlapCircle(feetTransform.position, feetRadius, groundLayer);
        animator.SetBool("Falling", rb.velocity.y < 0 && !isGrounded);

        if (isGrounded)
        {
            extraJumps = extraJumpsValue;
        }


        JumpFixedUpdate();
    }

    void FlipCharacter()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    void JumpFixedUpdate()
    {
        if (isJumping && jumpTimeCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + jumpForce * holdMultiplier);
            jumpTimeCounter -= Time.fixedDeltaTime;
        }

        rb.velocity = new Vector2(rb.velocity.x, Vector2.ClampMagnitude(new Vector2(0, rb.velocity.y), maxVerticalSpeed).y);
    }

    void JumpUpdate()
    {
        // Extra jumps
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                isJumping = true;
                jumpTimeCounter = jumpTime;
                animator.SetTrigger("Jump");
            }
            else if (extraJumps > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                isJumping = true;
                jumpTimeCounter = jumpTime;
                --extraJumps;
                animator.SetTrigger("Jump");
            }
        }

        // Hold to jump higher
        if (Input.GetButton("Jump"))
        {
            if (isJumping)
            {
                if (jumpTimeCounter > 0)
                {
                    jumpTimeCounter -= Time.deltaTime;
                }
                else
                {
                    isJumping = false;
                }
            }
        }
        else if (isJumping)
        {
            isJumping = false;
        }

        
    }

    void Attack()
    {
        if (!attacking && Input.GetButtonDown("Fire1"))
        {
            attacking = true;
            animator.SetTrigger("Attack");
            animator.SetFloat("AttackAnimationSpeed", 1f / attackSpeed);
        }
        
    }

    public void AttackFinished()
    {
        attacking = false;
    }
}
