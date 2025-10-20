using UnityEngine;
using UnityEngine.InputSystem;

public class JumpController : MonoBehaviour
{
    public CollisionController collisionController;
    public InputAction JumpAction;
    public float JumpForce = 1.0f;
    public float JumpTime = 0.3f;
    public float StopJumpingMultiplier = 0.5f;
    public float JumpPressBeforeHittingGroundTime = 0.5f;

    Rigidbody m_Rigidbody;
    public bool IsJumping { get; private set; } = false;
    float jumpTimeCountdown = 0f;
    float jumpPressBeforeHittingGroundTimeCountdown = 0f;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        JumpAction.Enable();
    }

    void OnDisable()
    {
        JumpAction.Disable();
    }

    void Update()
    {
        bool startedPressingJump = JumpAction.WasPressedThisFrame();
        bool stoppedPressingJump = JumpAction.WasReleasedThisFrame();

        HandleJumpPressBeforeLanding();
        if (startedPressingJump && collisionController.IsGrounded)
        {
            StartJump();
        }
        else if (stoppedPressingJump)
        {
            IsJumping = false;
        }
    }

    void HandleJumpPressBeforeLanding()
    {
        bool startedPressingJump = JumpAction.WasPressedThisFrame();
        if (startedPressingJump && !collisionController.IsGrounded)
        {
            jumpPressBeforeHittingGroundTimeCountdown = JumpPressBeforeHittingGroundTime;
        }

        if (jumpPressBeforeHittingGroundTimeCountdown > 0f)
        {
            jumpPressBeforeHittingGroundTimeCountdown -= Time.deltaTime;

            if (collisionController.IsGrounded)
            {
                jumpPressBeforeHittingGroundTimeCountdown = -1f;
                StartJump();
            }
        }
    }

    void FixedUpdate()
    {
        bool holdingJump = JumpAction.ReadValue<float>() > 0.5f;

        if (holdingJump && IsJumping)
        {
            if (jumpTimeCountdown > 0f)
            {
                m_Rigidbody.AddForce(Vector3.up * JumpForce);
                jumpTimeCountdown -= Time.fixedDeltaTime;
            }
            else
            {
                IsJumping = false;
            }
        }

        if (!holdingJump && m_Rigidbody.linearVelocity.y > 0f)
        {
            Vector3 newVelocity = m_Rigidbody.linearVelocity;
            newVelocity.y *= StopJumpingMultiplier;
            m_Rigidbody.linearVelocity = newVelocity;
        }
    }

    void StartJump()
    {
        IsJumping = true;
        m_Rigidbody.AddForce(Vector3.up * JumpForce);
        jumpTimeCountdown = JumpTime;
    }
}
