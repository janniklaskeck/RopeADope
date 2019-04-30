using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    [SerializeField]
    protected float MinGroundNormalY = .65f;
    [SerializeField]
    protected float GravityModifier = 1.0f;

    protected Vector2 TargetVelocity;
    protected Vector2 Velocity;
    protected bool IsGrounded = false;
    protected Vector2 GroundNormal;

    [SerializeField]
    private Rigidbody2D _RigidBody;

    protected ContactFilter2D ContactFilter;
    protected RaycastHit2D[] HitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> HitBufferList = new List<RaycastHit2D>(16);
    protected const float ShellRadius = 0.01f;

    protected const float MinMoveDistance = 0.001f;

    protected virtual void OnEnable()
    {
        if (_RigidBody == null)
        {
            _RigidBody = GetComponent<Rigidbody2D>();
        }
    }

    protected virtual void Update()
    {
        TargetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {

    }

    protected virtual void Start()
    {
        ContactFilter.useTriggers = false;
        ContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        ContactFilter.useLayerMask = true;
    }

    protected virtual void FixedUpdate()
    {
        Velocity += Physics2D.gravity * GravityModifier * Time.fixedDeltaTime;
        Velocity.x = TargetVelocity.x;

        IsGrounded = false;

        Vector2 DeltaPosition = Velocity * Time.fixedDeltaTime;

        Vector2 MoveAlongGround = new Vector2(GroundNormal.y, -GroundNormal.x);
        Vector2 HorizontalMove = MoveAlongGround * DeltaPosition.x;
        Movement(HorizontalMove, false);

        Vector2 VerticalMove = Vector2.up * DeltaPosition.y;
        Movement(VerticalMove, true);

        Vector3 StartPos = gameObject.transform.position;
        Vector3 EndPos = StartPos + new Vector3(Velocity.x, Velocity.y, 0f);
        Debug.DrawLine(StartPos, EndPos, Color.red);
        StartPos = gameObject.transform.position;
        EndPos = StartPos + new Vector3(GroundNormal.x, GroundNormal.y, 0f);
        Debug.DrawLine(StartPos, EndPos, Color.yellow);
    }

    protected virtual void Movement(Vector2 Move, bool IsVerticalMovement)
    {
        float Distance = Move.magnitude;

        if (Distance > MinMoveDistance)
        {
            int Count = _RigidBody.Cast(Move, ContactFilter, HitBuffer, Distance + ShellRadius);
            HitBufferList.Clear();
            for (int Index = 0; Index < Count; Index++)
            {
                HitBufferList.Add(HitBuffer[Index]);
            }

            for (int Index = 0; Index < HitBufferList.Count; Index++)
            {
                Vector2 CurrentNormal = HitBufferList[Index].normal;
                if (CurrentNormal.y > MinGroundNormalY)
                {
                    if (!IsGrounded)
                        OnGrounded();
                    IsGrounded = true;
                    if (IsVerticalMovement)
                    {
                        GroundNormal = CurrentNormal;
                        CurrentNormal.x = 0;
                    }
                }
                float Projection = Vector2.Dot(Velocity, CurrentNormal);
                if (Projection < 0)
                {
                    Velocity -= Projection * CurrentNormal;
                }
                float ModifiedDistance = HitBufferList[Index].distance - ShellRadius;
                if (ModifiedDistance < Distance)
                    Distance = ModifiedDistance;
            }

        }
        _RigidBody.position = _RigidBody.position + Move.normalized * Distance;
    }

    protected virtual void OnGrounded()
    {

    }
}
