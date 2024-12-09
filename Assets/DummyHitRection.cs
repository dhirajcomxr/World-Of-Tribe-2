using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyHitRection : MonoBehaviour
{
    public Animator anim;
    public CharacterController controller;
    public int hitCounter = 0;
    public void Damage()
    {
        hitCounter += 1;
        anim.SetTrigger("Hit");
        anim.SetInteger("HitNumber", 0);
    }

    #region PushBack
    public float pushbackForce = 5f; // Force of the pushback
    public float pushbackDuration = 0.2f; // Duration of the pushback effect

    private Vector3 pushbackDirection = Vector3.zero;
    private float pushbackTimer = 0f;

    public void PushBack()
    {
        if (pushbackTimer > 0)
        {
            controller.Move(pushbackDirection * pushbackForce * Time.deltaTime);
            pushbackTimer -= Time.deltaTime;
        }
    }

    public void PushBackOnCollesion(Collider other)
    {
        // Calculate pushback direction (away from the hit object)        
        pushbackDirection = (transform.position - other.transform.position).normalized;
        pushbackDirection.y = 0; // Ignore Y-axis to keep the pushback horizontal

        // Start the pushback timer
        pushbackTimer = pushbackDuration;
    }
    #endregion

    private void Update()
    {
        PushBack();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sword"))
        {
            Debug.Log(other.name + " Sword Got Hit");
            Damage();
            PushBackOnCollesion(other);
        }
    }
}
