using System;
using System.Collections;
using UnityEngine;

public class RotationDisabler : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            RotationController playerRotationController = collision.gameObject.GetComponent<RotationController>();
            playerRotationController.enabled = false;
        }
    }
}
