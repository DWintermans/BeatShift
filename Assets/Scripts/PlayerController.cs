using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Walking")]
    public InputAction MoveAction;
    public float walkSpeed = 1.0f;
    public float walkAccelerationForce = 100f;

    [Header("Rotating")]
    public InputAction RotateAction;
    public float RotateCooldown = 1.0f;

    public bool Rotated { get; private set; } = false;

    Rigidbody m_Rigidbody;
    float rotateCountdown = -1f;
    private Vector3 startPosition;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        startPosition = m_Rigidbody.position;
    }

    void OnEnable()
    {
        MoveAction.Enable();
        RotateAction.Enable();
    }

    void OnDisable()
    {
        MoveAction.Disable();
        RotateAction.Disable();
    }

    void FixedUpdate()
    {
        float movement = MoveAction.ReadValue<float>();
        Move(movement);

        bool rotate = RotateAction.ReadValue<float>() > 0.5f;
        HandleRotation(rotate);
    }

    void Move(float movement)
    {
        Vector3 moveDir;
        if (Rotated)
        {
            moveDir = new Vector3(0f, 0f, -movement);
        }
        else
        {
            moveDir = new Vector3(movement, 0f, 0f);
        }
        moveDir.Normalize();

        m_Rigidbody.AddForce(moveDir * walkAccelerationForce);

        Vector3 maxVelocity = moveDir * walkSpeed;
        maxVelocity.y = m_Rigidbody.linearVelocity.y;
        bool overMaxVelocityRotated = Rotated && Mathf.Abs(m_Rigidbody.linearVelocity.z) > Mathf.Abs(maxVelocity.z);
        bool overMaxVelocityNotRotated = !Rotated && Mathf.Abs(m_Rigidbody.linearVelocity.x) > Mathf.Abs(maxVelocity.x);
        if (overMaxVelocityRotated || overMaxVelocityNotRotated)
        {
            m_Rigidbody.linearVelocity = maxVelocity;
        }
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

        Rotated = !Rotated;

        Vector3 rotation = Rotated ? Vector3.up : Vector3.zero;
        rotation *= 90;
        m_Rigidbody.rotation = Quaternion.Euler(rotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Danger"))
        {
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;

            m_Rigidbody.position = startPosition;
        }
    }
}
