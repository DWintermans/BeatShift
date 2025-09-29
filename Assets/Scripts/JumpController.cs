using UnityEngine;
using UnityEngine.InputSystem;

public class JumpController : MonoBehaviour
{
    public CollisionController collisionController;
    public InputAction JumpAction;
    public float JumpForce = 1.0f;
    public float JumpTime = 0.3f;
    public float StopJumpingMultiplier = 0.5f;

    Rigidbody m_Rigidbody;
    bool isJumping = false;
    float jumpTimeCountdown = 0f;

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
        bool holdingJump = JumpAction.ReadValue<float>() > 0.5f;
        bool stoppedPressingJump = JumpAction.WasReleasedThisFrame();

        if (startedPressingJump && collisionController.IsGrounded)
        {
            isJumping = true;
            m_Rigidbody.AddForce(Vector3.up * JumpForce * Time.deltaTime);
            jumpTimeCountdown = JumpTime;
        }
        else if (stoppedPressingJump)
        {
            isJumping = false;
        }

        if (holdingJump && isJumping)
        {
            if (jumpTimeCountdown > 0f)
            {
                m_Rigidbody.AddForce(Vector3.up * JumpForce * Time.deltaTime);
                jumpTimeCountdown -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (!holdingJump && m_Rigidbody.linearVelocity.y > 0f)
        {
            Vector3 newVelocity = m_Rigidbody.linearVelocity;
            newVelocity.y *= StopJumpingMultiplier;
            m_Rigidbody.linearVelocity = newVelocity;
        }
    }
}
