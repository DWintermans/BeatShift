using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int Priority;
    public bool Rotated;
    public bool Activated{ get; private set; }

    void Start()
    {
        Activated = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            Activated = true;
            Rotated = player.Rotated;
        }
    }
}
