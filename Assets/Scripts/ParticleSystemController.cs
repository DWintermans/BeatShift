using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    [Header("Landing")]
    [SerializeField] private ParticleSystem landingParticleSystem;
    [SerializeField] private CollisionController collisionController;

    private bool lastIsGrounded;

    void FixedUpdate()
    {
        if (collisionController.IsGrounded && !lastIsGrounded)
        {
            landingParticleSystem.Play();
        }

        lastIsGrounded = collisionController.IsGrounded;
    }
}
