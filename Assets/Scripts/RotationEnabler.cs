using UnityEngine;

public class RotationEnabler : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            RotationController playerRotationController = collision.gameObject.GetComponent<RotationController>();
            playerRotationController.enabled = true;
        }
    }
}
