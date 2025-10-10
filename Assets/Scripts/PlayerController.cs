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
    public float RotateDuration = 0.3f;
    [Header("Checkpoint")]
    public CheckpointManager checkpointManager;

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
        PreventGoingOverMaxVelocity(moveDir);
    }

    void PreventGoingOverMaxVelocity(Vector3 moveDir)
    {
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
            rotateCountdown = RotateCooldown;
            Rotated = !Rotated;
            StartCoroutine(RotateOverTime());
        }
    }

    void HandleRotatedOnCheckpoint()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, gameObject.GetComponent<Collider>().bounds.extents);
        foreach (var hit in hits)
        {
            Checkpoint checkpoint = hit.GetComponent<Checkpoint>();
            if (checkpoint != null)
            {
                checkpoint.Rotated = Rotated;
            }
        }
    }

    System.Collections.IEnumerator RotateOverTime()
    {
        Quaternion startRot = m_Rigidbody.rotation;
        Quaternion endRot = Rotated
            ? Quaternion.Euler(Vector3.up * 90f)
            : Quaternion.Euler(Vector3.zero);

        float elapsed = 0f;
        while (elapsed < RotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / RotateDuration);
            m_Rigidbody.MoveRotation(Quaternion.Slerp(startRot, endRot, t));
            yield return null;
        }

        m_Rigidbody.MoveRotation(endRot);

        HandleRotatedOnCheckpoint();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Danger"))
        {
            Respawn();
        }

        if (collision.gameObject.CompareTag("Finish"))
        {
            FindFirstObjectByType<LevelManager>().LoadNextLevel();
        }
    }

    private void Respawn()
    {
        m_Rigidbody.linearVelocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;

        try
        {
            Checkpoint checkpoint = checkpointManager.GetLatestCheckpoint();
            Vector3 respawnPosition = checkpoint.transform.position;
            respawnPosition.y += checkpoint.GetComponent<Collider>().bounds.extents.y + GetComponent<Collider>().bounds.extents.y;

            m_Rigidbody.position = respawnPosition;
            Rotated = checkpoint.Rotated;
        }
        catch
        {
            m_Rigidbody.position = startPosition;
            Rotated = false;
        }

        StartCoroutine(RotateOverTime());
    }
}
