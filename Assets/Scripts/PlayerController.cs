using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Walking")]
    public InputAction MoveAction;
    public float walkSpeed = 1.0f;
    public float walkAccelerationForce = 100f;

    [Header("Checkpoint")]
    public CheckpointManager checkpointManager;

    public float Movement { get; private set; }

    private Rigidbody m_Rigidbody;
    private RotationController rotationController;
    private Vector3 startPosition;
    private bool rotated { get { return rotationController.Rotated; } }

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        rotationController = GetComponent<RotationController>();
        startPosition = m_Rigidbody.position;
    }

    void OnEnable()
    {
        MoveAction.Enable();
    }

    void OnDisable()
    {
        MoveAction.Disable();
    }

    void FixedUpdate()
    {
        Movement = MoveAction.ReadValue<float>();
        Move(Movement);
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

        m_Rigidbody.AddForce(moveDir * walkAccelerationForce);
        PreventGoingOverMaxVelocity(moveDir);
    }

    void PreventGoingOverMaxVelocity(Vector3 moveDir)
    {
        Vector3 maxVelocity = moveDir * walkSpeed;
        maxVelocity.y = m_Rigidbody.linearVelocity.y;
        bool overMaxVelocityRotated = rotated && Mathf.Abs(m_Rigidbody.linearVelocity.z) > Mathf.Abs(maxVelocity.z);
        bool overMaxVelocityNotRotated = !rotated && Mathf.Abs(m_Rigidbody.linearVelocity.x) > Mathf.Abs(maxVelocity.x);
        if (overMaxVelocityRotated || overMaxVelocityNotRotated)
        {
            m_Rigidbody.linearVelocity = maxVelocity;
        }
    }

<<<<<<< HEAD


=======
>>>>>>> dev
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Danger"))
        {
            Respawn();
        }

        if (collision.gameObject.CompareTag("Finish"))
        {
<<<<<<< HEAD
            FindFirstObjectByType<LevelManager>().LoadNextLevel();
=======
            string currentScene = SceneManager.GetActiveScene().name;

            var sequencer = FindFirstObjectByType<BeatSequencer>();
            if (sequencer != null)
                sequencer.PrepareSceneTransition(currentScene);
>>>>>>> dev
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
            rotationController.Rotated = checkpoint.Rotated;
        }
        catch
        {
            m_Rigidbody.position = startPosition;
            rotationController.Rotated = false;
        }

        if (rotationController.enabled)
        {
            StartCoroutine(rotationController.RotateOverTime());
        }
    }
}
