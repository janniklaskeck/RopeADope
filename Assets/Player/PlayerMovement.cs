using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float MaxMoveSpeed = 5f;
    public float Acceleration = 2f;
    public float Deceleration = 4f;

    public float JumpForce = 20f;
    public float JumpTimeMin = .5f;
    public float JumpTimeMax = 1f;
    public float JumpGravityScale = 15f;
    public float FallGravityScale = 50f;

    public float GroundRaycastDistance = .05f;
    public LayerMask GroundLayer;

    private Rigidbody2D _RigidBody;
    private CapsuleCollider2D _CapsuleCollider;
    private PlayerController _PlayerController;
    [SerializeField]
    private Animator PlayerAnimator;

    private Vector2 Velocity;
    private Vector2 TargetVelocity;

    private float JumpTimer;
    private bool IsGrounded = false;
    private bool IsJumping = false;
    private bool WantsJump = false;
    private ContactFilter2D ContactFilter;

    private void OnEnable()
    {
        _RigidBody = GetComponent<Rigidbody2D>();
        _CapsuleCollider = GetComponent<CapsuleCollider2D>();
        _PlayerController = GetComponent<PlayerController>();
    }

    private void Awake()
    {
        ContactFilter = new ContactFilter2D();
        ContactFilter.layerMask = GroundLayer;
        ContactFilter.useLayerMask = true;
        ContactFilter.useTriggers = false;
    }

    private void Update()
    {
        if (PlayerAnimator)
        {
            PlayerAnimator.SetBool("IsMoving", Mathf.Abs(Velocity.x) > .1f);
        }
    }

    private void FixedUpdate()
    {
        UpdateMovement();
        UpdateJump();

        _RigidBody.velocity = Velocity;

        // DEBUG

        Vector3 StartPos = gameObject.transform.position;
        Vector3 EndPos = StartPos + new Vector3(Velocity.x, Velocity.y, 0f);
        Debug.DrawLine(StartPos, EndPos, Color.red);
        //StartPos = gameObject.transform.position;
        //EndPos = StartPos + new Vector3(GroundNormal.x, GroundNormal.y, 0f);
        //Debug.DrawLine(StartPos, EndPos, Color.yellow);
    }

    private void UpdateMovement()
    {
        //Movement velocity
        TargetVelocity = _PlayerController.GetLastMovementInput();
        float desiredSpeed = Mathf.Abs(TargetVelocity.x) > 0.1f ? TargetVelocity.x * MaxMoveSpeed : 0f;
        float acceleration = Mathf.Abs(TargetVelocity.x) > 0.1f ? Acceleration : Deceleration;
        //acceleration = !is_grounded ? jump_move_percent * acceleration : acceleration;
        Velocity.x = Mathf.MoveTowards(Velocity.x, desiredSpeed, acceleration * Time.fixedDeltaTime);
    }

    private void UpdateJump()
    {
        CheckGrounded();
        JumpTimer += Time.fixedDeltaTime;
        bool StoppedEarly = IsJumping && !WantsJump && JumpTimer > JumpTimeMin;
        bool MaxJumpReached = IsJumping && JumpTimer > JumpTimeMax;
        if (StoppedEarly || MaxJumpReached)
            IsJumping = false;

        if (!IsGrounded)
        {
            float GravityScale = IsJumping ? JumpGravityScale : FallGravityScale;
            Velocity.y = Mathf.MoveTowards(Velocity.y, -MaxMoveSpeed * 2f, GravityScale * Time.fixedDeltaTime);
        }
        else if (!IsJumping)
        {
            Velocity.y = 0f;
        }
    }

    private void CheckGrounded()
    {
        bool grounded = false;
        Vector2[] raycastPositions = new Vector2[3];

        Vector2 raycast_start = _RigidBody.position;
        Vector2 orientation = /*detect_ceiled ? Vector2.up : */Vector2.down;
        float radius = _CapsuleCollider.size.x * 0.5f * transform.localScale.y; ;

        //Adapt raycast to collider
        Vector2 raycast_offset = _CapsuleCollider.offset + orientation * Mathf.Abs(_CapsuleCollider.size.y * 0.5f - _CapsuleCollider.size.x * 0.5f);
        raycast_start = _RigidBody.position + raycast_offset * transform.localScale.y;

        float ray_size = radius + GroundRaycastDistance;
        raycastPositions[0] = raycast_start + Vector2.left * radius / 2f;
        raycastPositions[1] = raycast_start;
        raycastPositions[2] = raycast_start + Vector2.right * radius / 2f;

        RaycastHit2D[] hitBuffer = new RaycastHit2D[5];
        for (int i = 0; i < raycastPositions.Length; i++)
        {
            Physics2D.Raycast(raycastPositions[i], orientation, ContactFilter, hitBuffer, ray_size);
            for (int j = 0; j < hitBuffer.Length; j++)
            {
                if (hitBuffer[j].collider != null)
                {
                    grounded = true;
                }
            }
        }
        IsGrounded = grounded;
    }

    public void Jump()
    {
        print("Jump");
        if (!IsGrounded)
        {
            return;
        }
        WantsJump = true;
        IsJumping = true;
        JumpTimer = 0f;
        Velocity.y = JumpForce;
    }

    public void StopJump()
    {
        print("StopJump");
        WantsJump = false;
    }

}
