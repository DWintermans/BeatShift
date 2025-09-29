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

    public bool Rotated { get; private set; } = false;

    Rigidbody m_Rigidbody;
    float rotateCountdown = -1f;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
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

        Rotated = !Rotated;

        Vector3 rotation = Rotated ? Vector3.up : Vector3.zero;
        rotation *= 90;
        m_Rigidbody.rotation = Quaternion.Euler(rotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Danger"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
