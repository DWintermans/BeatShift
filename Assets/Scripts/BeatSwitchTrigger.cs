using UnityEngine;

public class BeatSwitchTrigger : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BeatSequencer.Instance.SwitchBeat();
        }
    }
}
