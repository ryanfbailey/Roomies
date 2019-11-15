using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DizzyEffect : MonoBehaviour
{
    // Spin speed
    public float spinSpeed = 10f;

    // Spin
    private void Update()
    {
        transform.Rotate(new Vector3(0f, spinSpeed * Time.deltaTime, 0f));
    }
}
