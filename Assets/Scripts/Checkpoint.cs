using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [HideInInspector] public bool Rotated;
    public bool Activated { get; private set; }

    [Header("Activation changes")]
    [SerializeField] private ParticleSystem[] glitchParticleSystems;
    [SerializeField] private float changeMaterialDelaytime;
    [SerializeField] private GameObject[] platforms;
    [SerializeField] private Material activatedMaterial;
    [SerializeField] private GameObject[] images;
<<<<<<< HEAD
    [SerializeField] private Material activatedImage;
=======
    [SerializeField] private Material[] activatedImages;
>>>>>>> dev
    [SerializeField] private Vector3 activatedImageScale;

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
                ShowActivationParticles();
                StartCoroutine(ChangeMaterials(changeMaterialDelaytime));
            }
        }
    }

    private void ShowActivationParticles()
    {
        foreach (var obj in glitchParticleSystems)
        {
            obj.Play();
        }
    }

    private IEnumerator ChangeMaterials(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        foreach (var obj in platforms)
        {
            obj.GetComponent<Renderer>().material = activatedMaterial;
        }
<<<<<<< HEAD
        foreach (var obj in images)
        {
            obj.GetComponent<Renderer>().material = activatedImage;
            obj.transform.localScale = activatedImageScale;
=======
        for (int i = 0; i < images.Length; i++)
        {
            images[i].GetComponent<Renderer>().material = activatedImages[i];
            images[i].transform.localScale = activatedImageScale;
>>>>>>> dev
        }
    }
}
