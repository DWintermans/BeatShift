using UnityEngine;

public class BeatSwitchTrigger : MonoBehaviour
{ 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BeatSequencer.Instance.SwitchBeat();
        }
    }
}
