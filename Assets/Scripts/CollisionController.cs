using UnityEngine;

public class CollisionController : MonoBehaviour
{
    public PlayerController playerController;
    public Camera Camera;
    public float distance;
    public string platformsLayerName;

    public bool IsGrounded { get; private set; } = false;

    Rigidbody m_Rigidbody;
    Collider m_Collider;
    RaycastHit hit;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
    }

    void FixedUpdate()
    {
        Vector3 rayOriginLeft = GetRayOrigin(false);
        Vector3 rayOriginRight = GetRayOrigin(true);
        Vector3 direction = Camera.gameObject.transform.forward;

        Ray rayLeft = new Ray(rayOriginLeft, direction);
        Ray rayRight = new Ray(rayOriginRight, direction);
        bool hitLeft = Physics.Raycast(rayLeft, out hit, distance, LayerMask.GetMask(platformsLayerName));
        bool hitRight = Physics.Raycast(rayRight, out hit, distance, LayerMask.GetMask(platformsLayerName));

        if (hitLeft || hitRight)
        {
            AdjustDepthPosition(hit.collider);
            IsGrounded = Mathf.Approximately(hit.collider.bounds.max.y, m_Collider.bounds.min.y);
        }
        else
        {
            IsGrounded = false;
        }
        Debug.DrawRay(rayLeft.origin, rayLeft.direction * distance, Color.red);
        Debug.DrawRay(rayRight.origin, rayRight.direction * distance, Color.blue);
    }

    Vector3 GetRayOrigin(bool right)
    {
        Vector3 rayOrigin = Camera.gameObject.transform.position;
        rayOrigin.y -= m_Collider.bounds.extents.y;
        if (playerController.Rotated)
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

    void AdjustDepthPosition(Collider collider)
    {
        if (m_Rigidbody.linearVelocity.y > 0f)
        {
            return;
        }

        Vector3 position = transform.position;
        Bounds colBounds = collider.bounds;

        if (playerController.Rotated)
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
}
