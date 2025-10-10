using UnityEngine;

public class AnimationContoller : MonoBehaviour
{
    public PlayerController PlayerController;
    public CollisionController CollisionController;
    public JumpController JumpController;
    public Animator Animator;
    public Transform ModelToRotate;

    void Update()
    {
        float movement = PlayerController.Movement;

        bool isRunning = Mathf.Abs(movement) > 0.5f;
        if (isRunning)
        {
            if (movement > 0f)
                ModelToRotate.localRotation = Quaternion.Euler(Vector3.up * 90f); //face left
            else
                ModelToRotate.localRotation = Quaternion.Euler(Vector3.down * 90f); //face right
        }

        Animator.SetBool("IsRunning", isRunning);
        Animator.SetBool("IsLanding", CollisionController.IsGrounded);
        Animator.SetBool("IsJumping", JumpController.IsJumping);
    }
}
