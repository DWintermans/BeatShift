using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    [Header("Collision")]
    public Camera Camera;
    public float distance;

    Rigidbody m_Rigidbody;
    Collider m_Collider;

    bool rotated = false;
    float rotateCountdown = -1f;
    float jumpCountdown = -1f;
    bool isGrounded = false;

    RaycastHit hit;

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

    void AdjustDepthPosition(Collider collider)
    {
        if (m_Rigidbody.linearVelocity.y > 0f)
        {
            return;
        }

        Vector3 position = transform.position;
        Bounds colBounds = collider.bounds;

        if (rotated)
        {
            position.x = Mathf.Clamp(position.x, colBounds.min.x, colBounds.max.x);
            position.y -= GetDistanceDownByTilt(transform.position.x, position.x);
        }
        else
        {
            position.z = Mathf.Clamp(position.z, colBounds.min.z, colBounds.max.z);
            position.y -= GetDistanceDownByTilt(transform.position.z, position.z);
        }

        transform.position = position;
        PreventFallingThrough(colBounds);
    }

    float GetDistanceDownByTilt(float oldPlayerAxisPosition, float newPlayerAxisPosition)
    {
        float distanceMoved = oldPlayerAxisPosition - newPlayerAxisPosition;
        float tilt = Mathf.Tan(this.Camera.transform.eulerAngles.x * Mathf.Deg2Rad);
        return distanceMoved * tilt;
    }

    void PreventFallingThrough(Bounds colBounds)
    {
        if (m_Collider.bounds.min.y < colBounds.max.y && m_Rigidbody.linearVelocity.y < 0f)
        {
            Vector3 newVolecity = m_Rigidbody.linearVelocity;
            newVolecity.y = 0f;
            m_Rigidbody.linearVelocity = newVolecity;

            Vector3 position = transform.position;
            position.y = colBounds.max.y + m_Collider.bounds.extents.y;
            transform.position = position;
        }
    }

    void FixedUpdate()
    {
        CheckCollision();

        float movement = MoveAction.ReadValue<float>();
        Move(movement);

        bool rotate = RotateAction.ReadValue<float>() > 0.5f;
        HandleRotation(rotate);

        bool jump = JumpAction.ReadValue<float>() > 0.5f;
        Jump(jump);
    }

    Vector3 GetRayOrigin(bool right)
    {
        Vector3 rayOrigin = Camera.gameObject.transform.position;
        rayOrigin.y -= m_Collider.bounds.extents.y;
        if (rotated)
        {
            rayOrigin.x += m_Collider.bounds.extents.x;

            if (right)
                rayOrigin.z += m_Collider.bounds.extents.z;
            else
                rayOrigin.z -= m_Collider.bounds.extents.z;
        }
        else
        {
            rayOrigin.z += m_Collider.bounds.extents.z;

            if (right)
                rayOrigin.x -= m_Collider.bounds.extents.x;
            else
                rayOrigin.x += m_Collider.bounds.extents.x;
        }
        return rayOrigin;
    }

    void CheckCollision()
    {
        Vector3 rayOriginLeft = GetRayOrigin(false);
        Vector3 rayOriginRight = GetRayOrigin(true);
        Vector3 direction = Camera.gameObject.transform.forward;

        Ray rayLeft = new Ray(rayOriginLeft, direction);
        Ray rayRight = new Ray(rayOriginRight, direction);
        m_Collider.enabled = false;

        if (Physics.Raycast(rayLeft, out hit, distance) || Physics.Raycast(rayRight, out hit, distance))
        {
            m_Collider.enabled = true;
            AdjustDepthPosition(hit.collider);
            isGrounded = Mathf.Approximately(hit.collider.bounds.max.y, m_Collider.bounds.min.y);
        }
        else
        {
            isGrounded = false;
        }
        Debug.DrawRay(rayLeft.origin, rayLeft.direction * distance, Color.red);
        Debug.DrawRay(rayRight.origin, rayRight.direction * distance, Color.blue);
        m_Collider.enabled = true;
    }

    void Move(float movement)
    {
        Vector3 moveDir;
        if (rotated)
        {
            moveDir = new Vector3(0f, 0f, -movement);
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

        Vector3 rotation = rotated ? Vector3.up : Vector3.zero;
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
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Danger"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
