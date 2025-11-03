using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 EndPosition;
    [SerializeField] private float travelTime = 0.5f;

    private Vector3 startPosition;
    private Rigidbody m_Rigidbody;

    private bool isMoving = false;
    private bool goingToEnd = true;
    private float timer = 0f;

    void Start()
    {
        startPosition = transform.position;
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!isMoving) return;

        timer += Time.fixedDeltaTime / travelTime;

        Vector3 from = goingToEnd ? startPosition : EndPosition;
        Vector3 to = goingToEnd ? EndPosition : startPosition;

        Vector3 newPos = Vector3.Lerp(from, to, timer);
        m_Rigidbody.MovePosition(newPos);

        if (timer >= 1f)
        {
            isMoving = false;
            timer = 0f;
            goingToEnd = !goingToEnd;
        }
    }

    public void Move()
    {
        if (isMoving) return;
        isMoving = true;
        timer = 0f;
    }

        private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}

