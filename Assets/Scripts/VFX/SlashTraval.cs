using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashTraval : MonoBehaviour
{
    public Rigidbody rb;
    public float speed;

    private void Start()
    {
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);

    }
}
