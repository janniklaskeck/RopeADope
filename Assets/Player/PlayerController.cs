using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    private Vector2 LastInput;

    private PlayerMovement MovementComp;

    private void OnEnable()
    {
        MovementComp = GetComponent<PlayerMovement>();
    }

    public void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        LastInput.x = Input.GetAxis("Horizontal");
        LastInput.y = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            MovementComp.Jump();
        }
        else if (Input.GetButtonUp("Jump"))
        {
            MovementComp.StopJump();
        }
    }

    public Vector2 GetLastMovementInput()
    {
        return LastInput;
    }
}
