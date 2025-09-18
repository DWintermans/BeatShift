using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Walking")]
    public InputAction MoveAction;
    public float walkSpeed = 1.0f;

    [Header("Rotating")]
    public InputAction RotateAction;
    public float RotateCooldown = 1.0f;

    [Header("Jumping")]
    public InputAction JumpAction;
    public float JumpForce = 1.0f;
    public float JumpCooldown = 0.5f;

    Rigidbody m_Rigidbody;
    Collider m_Collider;

    bool rotated = false;
    float rotateCountdown = -1f;
    float jumpCountdown = -1f;
    bool isGrounded = false;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
    }

    void OnEnable()
    {
        MoveAction.Enable();
        RotateAction.Enable();
        JumpAction.Enable();
    }

    void OnDisable()
    {
        MoveAction.Disable();
        RotateAction.Disable();
        JumpAction.Disable();
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    float GetBottomY(Collider collider)
    {
        return collider.bounds.center.y - collider.bounds.extents.y;
    }

    void OnCollisionStay(Collision collision)
    {
        float thisBottomY = GetBottomY(m_Collider);
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.point.y <= thisBottomY + 0.01f)
            {
                isGrounded = true;
                AdjustDepthPosition(collision);
                break;
            }
        }
    }

    void AdjustDepthPosition(Collision collision)
    {
        Vector3 position = transform.position;
        Bounds colBounds = collision.collider.bounds;

        if (rotated)
        {
            position.x = Mathf.Clamp(position.x, colBounds.min.x, colBounds.max.x);
        }
        else
        {
            position.z = Mathf.Clamp(position.z, colBounds.min.z, colBounds.max.z);
        }

        transform.position = position;
    }

    void FixedUpdate()
    {
        float movement = MoveAction.ReadValue<float>();
        Move(movement);

        bool rotate = RotateAction.ReadValue<float>() > 0.5f;
        HandleRotation(rotate);

        bool jump = JumpAction.ReadValue<float>() > 0.5f;
        Jump(jump);
    }

    void Move(float movement)
    {
        Vector3 moveDir;
        if (rotated)
        {
            moveDir = new Vector3(0f, 0f, movement);
        }
        else
        {
            moveDir = new Vector3(movement, 0f, 0f);
        }
        moveDir.Normalize();

        Vector3 velocity = moveDir * walkSpeed;
        velocity.y = m_Rigidbody.linearVelocity.y;

        m_Rigidbody.linearVelocity = velocity;
    }

    void HandleRotation(bool rotate)
    {
        if (rotateCountdown > 0f)
        {
            rotateCountdown -= Time.deltaTime;
            return;
        }

        if (rotate)
        {
            Rotate();
        }
    }

    void Rotate()
    {
        rotateCountdown = RotateCooldown;

        rotated = !rotated;

        Vector3 rotation = rotated ? Vector3.down : Vector3.zero;
        rotation *= 90;
        m_Rigidbody.rotation = Quaternion.Euler(rotation);
    }

    void Jump(bool jump)
    {
        if (jumpCountdown > 0f)
        {
            jumpCountdown -= Time.deltaTime;
            return;
        }

        if (jump && isGrounded)
        {
            m_Rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            jumpCountdown = JumpCooldown;
            isGrounded = false;
        }
    }
}
