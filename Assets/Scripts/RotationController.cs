using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotationController : MonoBehaviour
{
    [SerializeField]  InputAction RotateAction;
    [SerializeField] private float RotateCooldown = 1.0f;
    [SerializeField] private float RotateDuration = 0.3f;

    [HideInInspector] public bool Rotated = false;
    [HideInInspector] public bool IsRotating { get; private set; } = false;

    private Rigidbody m_Rigidbody;
    private float rotateCountdown = -1f;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        RotateAction.Enable();
    }

    private void OnDisable()
    {
        RotateAction.Disable();
    }

    private void FixedUpdate()
    {
        bool rotate = RotateAction.ReadValue<float>() > 0.5f;

        if (rotateCountdown > 0f)
        {
            rotateCountdown -= Time.fixedDeltaTime;
            return;
        }

        if (rotate)
        {
            rotateCountdown = RotateCooldown;
            Rotated = !Rotated;
            StartCoroutine(RotateOverTime());
        }
    }

    private void HandleRotatedOnCheckpoint()
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

    public IEnumerator RotateOverTime()
    {
        Quaternion startRot = m_Rigidbody.rotation;
        Quaternion endRot = Rotated
            ? Quaternion.Euler(Vector3.up * 90f)
            : Quaternion.Euler(Vector3.zero);

        IsRotating = true;
        float elapsed = 0f;
        while (elapsed < RotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / RotateDuration);
            m_Rigidbody.MoveRotation(Quaternion.Slerp(startRot, endRot, t));
            yield return null;
        }

        m_Rigidbody.MoveRotation(endRot);
        IsRotating = false;

        HandleRotatedOnCheckpoint();
    }
}
