using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public ParticleSystem checkpointParticleSystem;
    [HideInInspector] public bool Rotated;
    public bool Activated { get; private set; }

    void Start()
    {
        Activated = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            RotationController playerRotationController = collision.gameObject.GetComponent<RotationController>();

            bool lastActivated = Activated;
            Activated = true;
            Rotated = playerRotationController.Rotated;


            if (!lastActivated && Activated)
            {
                Vector3 vfxLocation = transform.position;
                vfxLocation.y += GetComponent<Collider>().bounds.extents.y;
                Vector3 scale = transform.localScale;
                scale.y = Mathf.Min(scale.x, scale.z);
                ParticleSystem instantiatedParticaleSystem = Instantiate(checkpointParticleSystem, vfxLocation, checkpointParticleSystem.transform.rotation);
                instantiatedParticaleSystem.transform.localScale = scale;
            }
        }
    }
}
